using System.Collections.ObjectModel;
using TestsRunner.Models.Enums;

namespace TestsRunner.Models
{
    public class TestAssemblyModel
    {
        public string AssemblyPath { get; set; }
        public string AssemblyName => System.IO.Path.GetFileName(AssemblyPath);
        public ObservableCollection<TestClassModel> Classes { get; set; } = new ObservableCollection<TestClassModel>();
        public TestStatus Status { get; set; }

        public TestAssemblyModel(string path)
        {
            AssemblyPath = path;
        }
    }
}