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
        #region Fields

        private readonly IDialogService _dialogService;
        private TreeViewItemBase _selectedItem;
        private int _totalTests;
        private int _passedTests;
        private int _failedTests;
        private int _skippedTests;
        private int _selectedCount;

        #endregion

        #region Properties
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
        #endregion

        #region Commands
        // Команды
        public RelayCommand LoadAssemblyCommand { get; }
        public RelayCommand DeleteAssemblyCommand { get; }
        public RelayCommand RunSelectedTestsCommand { get; }
        #endregion

        #region Constructors

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

        #endregion

        #region Load/delete assembly
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

        #endregion

        #region Run

        private async Task RunSelectedTestsAsync()
        {
            if (SelectedItem == null)
            {
                _dialogService.ShowMessage("Выберите тест для запуска", "Информация", MessageBoxImage.Information);
                return;
            }

            try
            {
                //Определяем тип выбранного элемента и запускаем соответствующие тесты
                switch (SelectedItem)
                {
                    case AssemblyViewModel assemblyVM:
                        await RunTestsInternalAsync(assemblyVM, RunAssemblyTestsAsync);
                        break;

                    case ClassViewModel classVM:
                        await RunTestsInternalAsync(classVM, RunClassTestsAsync);
                        break;

                    case MethodViewModel methodVM:
                        await RunTestsInternalAsync(methodVM, RunSingleTestAsync);
                        break;
                    default:
                        _dialogService.ShowMessage("Выбран неподдерживаемый тип элемента", "Ошибка", MessageBoxImage.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка выполнения тестов: {ex.Message}", "Ошибка", MessageBoxImage.Error);
            }
        }

        private async Task RunTestsInternalAsync<T>(T item, Func<T, Task> runTestsFunc) where T : TreeViewItemBase
        {
            await Task.Run(async () =>
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SetRunningStatus(item);
                    UpdateParentStatuses(item);
                    UpdateOverallStatistics();
                });

                await runTestsFunc(item);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdateFromModel(item);
                    UpdateParentStatuses(item);
                    UpdateOverallStatistics();
                });
            });
        }

        private async Task RunAssemblyTestsAsync(AssemblyViewModel assemblyVM)
        {
            await Task.Run(() => TestRunnerService.RunAssemblyTests(assemblyVM.Assembly));
        }

        private async Task RunClassTestsAsync(ClassViewModel classVM)
        {
            await Task.Run(() => TestRunnerService.RunClassTests(classVM.Class));
        }

        private async Task RunSingleTestAsync(MethodViewModel methodVM)
        {
            var classVM = FindParentClass(methodVM);
            if (classVM == null)
            {
                _dialogService.ShowMessage("Не удалось найти родительский класс для метода", "Ошибка", MessageBoxImage.Error);
                return;
            }

            if (methodVM.IsParameterized)
            {
                await RunParameterizedMethodAsync(methodVM, classVM);
            }
            else
            {
                await RunOrdinaryMethodAsync(methodVM, classVM);
            }
        }

        private async Task RunParameterizedMethodAsync(MethodViewModel methodVM, ClassViewModel classVM)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    methodVM.Status = TestStatus.Running;
                    foreach (var testCaseVM in methodVM.Children.OfType<TestCaseViewModel>())
                    {
                        testCaseVM.Status = TestStatus.Running;
                    }
                    UpdateClassStatus(classVM);
                });

                foreach (var testCaseVM in methodVM.Children.OfType<TestCaseViewModel>())
                {
                    TestRunnerService.RunTestCase(
                        testCaseVM.TestCase,
                        methodVM.Method.MethodInfo,
                        classVM.Class);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {

                    foreach (var testCaseVM in methodVM.Children.OfType<TestCaseViewModel>())
                    {
                        testCaseVM.UpdateFromModel();
                    }

                    methodVM.UpdateFromModel();

                    UpdateClassStatus(classVM);
                    if (classVM.Parent is AssemblyViewModel assemblyVM)
                    {
                        UpdateAssemblyStatus(assemblyVM);
                    }

                    UpdateOverallStatistics();
                });
            });
        }

        private async Task RunOrdinaryMethodAsync(MethodViewModel methodVM, ClassViewModel classVM)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    methodVM.Status = TestStatus.Running;
                    UpdateClassStatus(classVM);
                });

                TestRunnerService.RunTest(methodVM.Method, classVM.Class);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    methodVM.UpdateFromModel();
                    UpdateClassStatus(classVM);
                    if (classVM.Parent is AssemblyViewModel assemblyVM)
                    {
                        UpdateAssemblyStatus(assemblyVM);
                    }
                    UpdateOverallStatistics();
                });
            });
        }

        #endregion Run

        #region UI

        //Метод для установки статуса Running
        private void SetRunningStatus(TreeViewItemBase item)
        {
            switch (item)
            {
                case TestCaseViewModel testCaseVM:
                    testCaseVM.Status = TestStatus.Running;
                    break;

                case MethodViewModel methodVM:
                    if (methodVM.IsParameterized)
                    {
                        methodVM.Status = TestStatus.Running;
                        foreach (var child in methodVM.Children.OfType<TestCaseViewModel>())
                        {
                            child.Status = TestStatus.Running;
                        }
                    }
                    else
                    {
                        methodVM.Status = TestStatus.Running;
                    }
                    break;

                case ClassViewModel classVM:
                    foreach (var method in classVM.Children.OfType<MethodViewModel>())
                    {
                        SetRunningStatus(method);
                    }
                    break;

                case AssemblyViewModel assemblyVM:
                    foreach (var method in GetAllMethods(assemblyVM))
                    {
                        method.Status = TestStatus.Running;
                    }
                    break;
            }
        }

        //Обновленный UpdateFromModel
        private void UpdateFromModel(TreeViewItemBase item)
        {
            switch (item)
            {
                case TestCaseViewModel testCaseVM:
                    testCaseVM.UpdateFromModel();
                    break;

                case MethodViewModel methodVM:
                    methodVM.UpdateFromModel();
                    break;

                case ClassViewModel classVM:
                    foreach (var method in classVM.Children.OfType<MethodViewModel>())
                    {
                        method.UpdateFromModel();
                    }
                    break;

                case AssemblyViewModel assemblyVM:
                    foreach (var method in GetAllMethods(assemblyVM))
                    {
                        method.UpdateFromModel();
                    }
                    break;
            }
        }

        //Метод для обновления статусов родительских элементов
        private void UpdateParentStatuses(TreeViewItemBase item)
        {
            switch (item)
            {
                case MethodViewModel methodVM:
                    var parentClass = FindParentClass(methodVM);
                    if (parentClass != null)
                    {
                        UpdateClassStatus(parentClass);
                        if (parentClass.Parent is AssemblyViewModel assemblyVM)
                        {
                            UpdateAssemblyStatus(assemblyVM);
                        }
                    }
                    break;

                case ClassViewModel classVM:
                    UpdateClassStatus(classVM);
                    if (classVM.Parent is AssemblyViewModel parentAssembly)
                    {
                        UpdateAssemblyStatus(parentAssembly);
                    }
                    break;

                case AssemblyViewModel assemblyVM:
                    foreach (var classVM in assemblyVM.Children.OfType<ClassViewModel>())
                        UpdateClassStatus(classVM);
                    UpdateAssemblyStatus(assemblyVM);
                    break;
            }
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
                    {
                        if (child is MethodViewModel methodChild)
                            yield return methodChild;
                        else if (child is ClassViewModel nestedClass)
                            foreach (var m in GetAllMethods(nestedClass))
                                yield return m;
                    }
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

        #endregion
    }
}