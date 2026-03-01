using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestsRunner.Enums;
using TestsRunner.Models;

namespace TestsRunner.ViewModels
{
    public class TreeViewItemViewModel : INotifyPropertyChanged
    {
        private TreeViewItemViewModel _parent;
        private TestStatus _status;
        private string _displayName;

        public event PropertyChangedEventHandler PropertyChanged;

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged();
                }
            }
        }

        public TestStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public object Model { get; set; }
        public ObservableCollection<TreeViewItemViewModel> Children { get; set; } = new ObservableCollection<TreeViewItemViewModel>();

        public ItemType ItemType { get; set; }

        public TreeViewItemViewModel Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                OnPropertyChanged();
            }
        }

        public TestMethodModel TestMethod => Model as TestMethodModel;
        public TestClassModel TestClass => Model as TestClassModel;
        public TestAssemblyModel TestAssembly => Model as TestAssemblyModel;

        public void AddChild(TreeViewItemViewModel child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}