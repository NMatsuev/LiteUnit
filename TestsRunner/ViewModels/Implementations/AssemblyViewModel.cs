using TestsRunner.Models;
using TestsRunner.Models.Enums;

namespace TestsRunner.ViewModels.Implementations
{
    public class AssemblyViewModel : TreeViewItemBase
    {
        private TestAssemblyModel _assembly;

        public AssemblyViewModel(TestAssemblyModel assembly)
        {
            _assembly = assembly;
            DisplayName = assembly.AssemblyName;
        }

        public TestAssemblyModel Assembly => _assembly;

        public override ItemType ItemType => ItemType.Assembly;

        public string FullPath => _assembly.AssemblyPath;
    }
}