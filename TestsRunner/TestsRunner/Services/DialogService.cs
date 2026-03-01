using System.Windows;
using Microsoft.Win32;

namespace TestsRunner.Services
{
    public class DialogService : IDialogService
    {
        public string OpenFileDialog(string filter, string title)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public void ShowMessage(string message, string caption, MessageBoxImage icon)
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, icon);
        }
    }
}