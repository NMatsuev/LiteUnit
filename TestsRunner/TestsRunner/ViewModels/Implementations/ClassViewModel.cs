using TestsRunner.Models;
using TestsRunner.Models.Enums;

namespace TestsRunner.ViewModels.Implementations
{
    public class ClassViewModel : TreeViewItemBase
    {
        private TestClassModel _class;

        public ClassViewModel(TestClassModel classModel)
        {
            _class = classModel;
            DisplayName = classModel.ClassName;
        }

        public TestClassModel Class => _class;

        public override ItemType ItemType => _class.NestedClasses.Any() ? ItemType.NestedClass : ItemType.Class;

        public string FullClassName => _class.ClassType.FullName;
    }
}