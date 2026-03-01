using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TestsRunner.Enums;
using TestsRunner.Models;
using TestsRunner.Services;
using TestsRunner.ViewModels;

namespace TestsRunner
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<TreeViewItemViewModel> TestAssemblies { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            TestAssemblies = new ObservableCollection<TreeViewItemViewModel>();
            DataContext = this;
        }

        private void btnLoadAssembly_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Assembly files (*.dll;*.exe)|*.dll;*.exe",
                Title = "Выберите тестовую сборку"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var assemblyModel = TestLoaderService.LoadAssembly(dialog.FileName);

                    // Создаем ViewModel для дерева
                    var assemblyVM = new TreeViewItemViewModel
                    {
                        DisplayName = assemblyModel.AssemblyName,
                        Status = TestStatus.None,
                        Model = assemblyModel,
                        ItemType = ItemType.Assembly
                    };

                    // Добавляем классы в дерево
                    foreach (var classModel in assemblyModel.Classes)
                    {
                        AddClassToTree(assemblyVM, classModel);
                    }

                    TestAssemblies.Add(assemblyVM);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки сборки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddClassToTree(TreeViewItemViewModel parentVM, TestClassModel classModel)
        {
            var classVM = new TreeViewItemViewModel
            {
                DisplayName = classModel.ClassName,
                Status = TestStatus.None,
                Model = classModel,
                ItemType = classModel.NestedClasses.Any() ? ItemType.NestedClass : ItemType.Class
            };

            // Добавляем методы
            foreach (var methodModel in classModel.Methods)
            {
                var methodVM = new TreeViewItemViewModel
                {
                    DisplayName = methodModel.MethodName,
                    Status = TestStatus.None,
                    Model = methodModel,
                    ItemType = ItemType.Method
                };
                classVM.AddChild(methodVM);
            }

            // Добавляем вложенные классы
            foreach (var nestedClass in classModel.NestedClasses)
            {
                AddClassToTree(classVM, nestedClass);
            }

            parentVM.AddChild(classVM);
        }

        private void btnDeleteAssembly_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный элемент
            var selectedItem = GetSelectedItem();

            if (selectedItem != null && selectedItem.ItemType == ItemType.Assembly)
            {
                TestAssemblies.Remove(selectedItem);
                UpdateSelectedCountText(0);
            }
        }

        private async void btnRunSelectedTests_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = GetSelectedItem();

            if (selectedItem == null)
            {
                MessageBox.Show("Выберите тест для запуска", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Собираем все тестовые методы из выбранного элемента
                var testMethods = new System.Collections.Generic.List<(TestMethodModel method, TestClassModel classModel)>();

                CollectTestMethods(selectedItem, testMethods);

                if (!testMethods.Any())
                {
                    MessageBox.Show("В выбранном элементе нет тестов для запуска", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Запускаем тесты асинхронно
                await System.Threading.Tasks.Task.Run(() =>
                {
                    foreach (var (method, classModel) in testMethods)
                    {
                        TestRunnerService.RunTest(method, classModel);

                        // Обновляем статус в UI
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateItemStatus(item => item.TestMethod == method, method.Status);
                        });
                    }
                });

                // Обновляем статусы родительских элементов
                UpdateParentStatuses();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выполнения тестов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод для получения выбранного элемента (один элемент)
        private TreeViewItemViewModel GetSelectedItem()
        {
            return TestTreeView.SelectedItem as TreeViewItemViewModel;
        }

        private void CollectTestMethods(TreeViewItemViewModel item,
            System.Collections.Generic.List<(TestMethodModel, TestClassModel)> methods)
        {
            switch (item.ItemType)
            {
                case ItemType.Method:
                    if (item.TestMethod != null)
                    {
                        // Ищем родительский класс
                        var classVM = FindParentClass(item);
                        if (classVM?.TestClass != null)
                        {
                            methods.Add((item.TestMethod, classVM.TestClass));
                        }
                    }
                    break;

                case ItemType.Class:
                case ItemType.NestedClass:
                    if (item.TestClass != null)
                    {
                        foreach (var method in item.Children.Where(c => c.ItemType == ItemType.Method))
                        {
                            methods.Add((method.TestMethod, item.TestClass));
                        }
                    }
                    break;

                case ItemType.Assembly:
                    foreach (var child in item.Children)
                    {
                        CollectTestMethods(child, methods);
                    }
                    break;
            }
        }

        // Метод для поиска родительского класса
        private TreeViewItemViewModel FindParentClass(TreeViewItemViewModel item)
        {
            var current = item.Parent;
            while (current != null)
            {
                if (current.ItemType == ItemType.Class || current.ItemType == ItemType.NestedClass)
                {
                    return current;
                }
                current = current.Parent;
            }
            return null;
        }

        private void UpdateItemStatus(Func<TreeViewItemViewModel, bool> predicate, TestStatus status)
        {
            foreach (var assembly in TestAssemblies)
            {
                UpdateItemStatusRecursive(assembly, predicate, status);
            }
        }

        private bool UpdateItemStatusRecursive(TreeViewItemViewModel item,
            Func<TreeViewItemViewModel, bool> predicate, TestStatus status)
        {
            if (predicate(item))
            {
                item.Status = status;
                return true;
            }

            foreach (var child in item.Children)
            {
                if (UpdateItemStatusRecursive(child, predicate, status))
                    return true;
            }

            return false;
        }

        private void UpdateParentStatuses()
        {
            foreach (var assembly in TestAssemblies)
            {
                UpdateParentStatusRecursive(assembly);
            }
        }

        private TestStatus UpdateParentStatusRecursive(TreeViewItemViewModel item)
        {
            if (item.ItemType == ItemType.Method)
                return item.Status;

            foreach (var child in item.Children)
            {
                var childStatus = UpdateParentStatusRecursive(child);
                if (childStatus != TestStatus.None)
                {
                    // Обновляем статус родителя на основе статусов детей
                    if (item.Status == TestStatus.None || item.Status == TestStatus.Running)
                    {
                        item.Status = childStatus;
                    }
                    else if (item.Status != childStatus && childStatus != TestStatus.None)
                    {
                       // item.Status = TestStatus.Warning; // Смешанный статус
                    }
                }
            }

            return item.Status;
        }

        private void TestTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = GetSelectedItem();
            int count = 0;

            if (selectedItem != null)
            {
                var testMethods = new System.Collections.Generic.List<TreeViewItemViewModel>();
                CollectTestMethodsForCount(selectedItem, testMethods);
                count = testMethods.Count;
            }

            UpdateSelectedCountText(count);
        }

        private void CollectTestMethodsForCount(TreeViewItemViewModel item,
            System.Collections.Generic.List<TreeViewItemViewModel> methods)
        {
            switch (item.ItemType)
            {
                case ItemType.Method:
                    methods.Add(item);
                    break;

                case ItemType.Class:
                case ItemType.NestedClass:
                case ItemType.Assembly:
                    foreach (var child in item.Children)
                    {
                        CollectTestMethodsForCount(child, methods);
                    }
                    break;
            }
        }

        private void UpdateSelectedCountText(int count)
        {
            // Ищем TextBlock по имени в визуальном дереве
            var textBlock = this.FindName("txtSelectedCount") as TextBlock;
            if (textBlock != null)
            {
                string testWord = GetTestWord(count);
                textBlock.Text = $"Выбрано: {count} {testWord}";
            }
        }

        private string GetTestWord(int count)
        {
            if (count % 10 == 1 && count % 100 != 11)
                return "тест";
            if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20))
                return "теста";
            return "тестов";
        }
    }
}