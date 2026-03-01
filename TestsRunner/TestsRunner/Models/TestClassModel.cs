using System.Collections.ObjectModel;
using System.Reflection;
using TestsRunner.Models.Enums;

namespace TestsRunner.Models
{
    public class TestClassModel
    {
        public Type ClassType { get; set; }
        public string ClassName => ClassType.Name;
        public ObservableCollection<TestClassModel> NestedClasses { get; set; } = new ObservableCollection<TestClassModel>();
        public ObservableCollection<TestMethodModel> Methods { get; set; } = new ObservableCollection<TestMethodModel>();
        public TestStatus Status { get; set; }

        //Специальные методы
        public MethodInfo SetUpMethod { get; set; }
        public MethodInfo TearDownMethod { get; set; }
        public MethodInfo FixtureSetUpMethod { get; set; }
        public MethodInfo FixtureTearDownMethod { get; set; }
    }
}