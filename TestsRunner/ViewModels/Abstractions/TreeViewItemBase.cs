using System.Collections.ObjectModel;
using TestsRunner.Models.Enums;

namespace TestsRunner.ViewModels
{
    public abstract class TreeViewItemBase : ViewModelBase
    {
        private string _displayName;
        private TestStatus _status;
        private bool _isExpanded = true;
        private bool _isSelected;

        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public TestStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ObservableCollection<TreeViewItemBase> Children { get; } = new ObservableCollection<TreeViewItemBase>();

        public abstract ItemType ItemType { get; }

        public TreeViewItemBase Parent { get; set; }

        public void AddChild(TreeViewItemBase child)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }
}