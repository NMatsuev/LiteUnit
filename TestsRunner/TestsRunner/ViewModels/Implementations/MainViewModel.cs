using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using TestsRunner.Models;
using TestsRunner.Services;
using TestsRunner.Helpers;
using TestsRunner.Models.Enums;

namespace TestsRunner.ViewModels.Implementations
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private TreeViewItemBase _selectedItem;
        private int _totalTests;
        private int _passedTests;
        private int _failedTests;
        private int _skippedTests;
        private int _selectedCount;

        public ObservableCollection<TreeViewItemBase> TestAssemblies { get; }

        public TreeViewItemBase SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    UpdateSelectedCount();
                }
            }
        }

        public int TotalTests
        {
            get => _totalTests;
            private set => SetProperty(ref _totalTests, value);
        }

        public int PassedTests
        {
            get => _passedTests;
            private set => SetProperty(ref _passedTests, value);
        }

        public int FailedTests
        {
            get => _failedTests;
            private set => SetProperty(ref _failedTests, value);
        }

        public int SkippedTests
        {
            get => _skippedTests;
            private set => SetProperty(ref _skippedTests, value);
        }

        public int SelectedCount
        {
            get => _selectedCount;
            private set => SetProperty(ref _selectedCount, value);
        }

        public string SelectedCountText => $"Выбрано: {SelectedCount} {GetTestWord(SelectedCount)}";

        // Команды
        public RelayCommand LoadAssemblyCommand { get; }
        public RelayCommand DeleteAssemblyCommand { get; }
        public RelayCommand RunSelectedTestsCommand { get; }

        public MainViewModel() : this(new DialogService()) { }

        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            TestAssemblies = new ObservableCollection<TreeViewItemBase>();
            TestAssemblies.CollectionChanged += TestAssemblies_CollectionChanged;

            // Инициализация команд
            LoadAssemblyCommand = new RelayCommand(async () => await LoadAssemblyAsync());
            DeleteAssemblyCommand = new RelayCommand(DeleteAssembly, () => SelectedItem is AssemblyViewModel);
            RunSelectedTestsCommand = new RelayCommand(async () => await RunSelectedTestsAsync(), () => SelectedItem != null);
        }

        private async Task LoadAssemblyAsync()
        {
            var filePath = _dialogService.OpenFileDialog("Assembly files (*.dll;*.exe)|*.dll;*.exe", "Выберите тестовую сборку");
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                var assemblyModel = await Task.Run(() => TestLoaderService.LoadAssembly(filePath));
                var assemblyVM = new AssemblyViewModel(assemblyModel);

                foreach (var classModel in assemblyModel.Classes)
                {
                    AddClassToTree(assemblyVM, classModel);
                }

                TestAssemblies.Add(assemblyVM);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки сборки: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private void AddClassToTree(TreeViewItemBase parentVM, TestClassModel classModel)
        {
            var classVM = new ClassViewModel(classModel);

            foreach (var methodModel in classModel.Methods)
            {
                classVM.AddChild(new MethodViewModel(methodModel));
            }

            foreach (var nestedClass in classModel.NestedClasses)
            {
                AddClassToTree(classVM, nestedClass);
            }

            parentVM.AddChild(classVM);
        }

        private void DeleteAssembly()
        {
            if (SelectedItem is AssemblyViewModel assemblyVM)
            {
                TestAssemblies.Remove(assemblyVM);
                SelectedItem = null;
            }
        }

        private async Task RunSelectedTestsAsync()
        {
            if (SelectedItem == null)
            {
                _dialogService.ShowMessage("Выберите тест для запуска", "Информация", MessageBoxImage.Information);
                return;
            }

            try
            {
                var testMethods = CollectTestMethods(SelectedItem);

                if (!testMethods.Any())
                {
                    _dialogService.ShowMessage("В выбранном элементе нет тестов для запуска", "Информация", MessageBoxImage.Information);
                    return;
                }

                await Task.Run(() =>
                {
                    foreach (var (methodVM, classVM) in testMethods)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            methodVM.Status = TestStatus.Running;
                            UpdateClassStatus(classVM);
                            if (classVM.Parent is AssemblyViewModel assemblyVM)
                                UpdateAssemblyStatus(assemblyVM);
                        });

                        TestRunnerService.RunTest(methodVM.Method, classVM.Class);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            methodVM.UpdateFromModel();
                            UpdateClassStatus(classVM);
                            if (classVM.Parent is AssemblyViewModel assemblyVM)
                                UpdateAssemblyStatus(assemblyVM);

                            UpdateOverallStatistics();
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка выполнения тестов: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private List<(MethodViewModel, ClassViewModel)> CollectTestMethods(TreeViewItemBase item)
        {
            var methods = new List<(MethodViewModel, ClassViewModel)>();

            void Collect(TreeViewItemBase current)
            {
                switch (current)
                {
                    case MethodViewModel methodItem:
                        var parentClass = FindParentClass(methodItem);
                        if (parentClass != null)
                            methods.Add((methodItem, parentClass));
                        break;

                    case ClassViewModel classItem:
                        foreach (var child in classItem.Children)
                            Collect(child);
                        break;

                    case AssemblyViewModel assemblyItem:
                        foreach (var child in assemblyItem.Children)
                            Collect(child);
                        break;
                }
            }

            Collect(item);
            return methods;
        }

        private ClassViewModel FindParentClass(TreeViewItemBase item)
        {
            var current = item.Parent;
            while (current != null)
            {
                if (current is ClassViewModel classVM)
                    return classVM;
                current = current.Parent;
            }
            return null;
        }

        private void UpdateClassStatus(ClassViewModel classVM)
        {
            var methods = classVM.Children.OfType<MethodViewModel>().ToList();

            if (methods.All(m => m.Status == TestStatus.Passed))
                classVM.Status = TestStatus.Passed;
            else if (methods.Any(m => m.Status == TestStatus.Failed))
                classVM.Status = TestStatus.Failed;
            else if (methods.Any(m => m.Status == TestStatus.Running))
                classVM.Status = TestStatus.Running;
            else
                classVM.Status = TestStatus.None;
        }

        private void UpdateAssemblyStatus(AssemblyViewModel assemblyVM)
        {
            var classes = assemblyVM.Children.OfType<ClassViewModel>().ToList();

            if (classes.All(c => c.Status == TestStatus.Passed))
                assemblyVM.Status = TestStatus.Passed;
            else if (classes.Any(c => c.Status == TestStatus.Failed))
                assemblyVM.Status = TestStatus.Failed;
            else if (classes.Any(c => c.Status == TestStatus.Running))
                assemblyVM.Status = TestStatus.Running;
            else
                assemblyVM.Status = TestStatus.None;
        }

        private void UpdateOverallStatistics()
        {
            var allMethods = TestAssemblies
                .OfType<AssemblyViewModel>()
                .SelectMany(a => GetAllMethods(a))
                .ToList();

            TotalTests = allMethods.Count;
            PassedTests = allMethods.Count(m => m.Status == TestStatus.Passed);
            FailedTests = allMethods.Count(m => m.Status == TestStatus.Failed);
            SkippedTests = allMethods.Count(m => m.Status == TestStatus.None || m.Status == TestStatus.Running);
        }

        private IEnumerable<MethodViewModel> GetAllMethods(TreeViewItemBase item)
        {
            switch (item)
            {
                case MethodViewModel method:
                    yield return method;
                    break;

                case ClassViewModel classVM:
                    foreach (var child in classVM.Children)
                        foreach (var m in GetAllMethods(child))
                            yield return m;
                    break;

                case AssemblyViewModel assembly:
                    foreach (var child in assembly.Children)
                        foreach (var m in GetAllMethods(child))
                            yield return m;
                    break;
            }
        }

        private void UpdateSelectedCount()
        {
            if (SelectedItem == null)
            {
                SelectedCount = 0;
                return;
            }

            var methods = new List<MethodViewModel>();

            void Collect(TreeViewItemBase current)
            {
                switch (current)
                {
                    case MethodViewModel methodItem:
                        methods.Add(methodItem);
                        break;
                    case ClassViewModel classItem:
                    case AssemblyViewModel assemblyItem:
                        foreach (var child in current.Children)
                            Collect(child);
                        break;
                }
            }

            Collect(SelectedItem);
            SelectedCount = methods.Count;
            OnPropertyChanged(nameof(SelectedCountText));
        }

        private void TestAssemblies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateOverallStatistics();
        }

        private string GetTestWord(int count)
        {
            if (count % 10 == 1 && count % 100 != 11) return "тест";
            if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20)) return "теста";
            return "тестов";
        }
    }
}