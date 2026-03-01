using System.Windows;
using TestsRunner.ViewModels;
using TestsRunner.ViewModels.Implementations;

namespace TestsRunner.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TestTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.SelectedItem = TestTreeView.SelectedItem as TreeViewItemBase;
            }
        }
    }
}