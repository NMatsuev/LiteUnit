using System.Windows;

namespace TestsRunner.Services
{
    public interface IDialogService
    {
        string OpenFileDialog(string filter, string title);
        void ShowMessage(string message, string caption, MessageBoxImage icon);
    }
}