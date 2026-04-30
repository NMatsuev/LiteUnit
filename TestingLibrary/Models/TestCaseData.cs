namespace TestingLibrary.Models
{
    public class TestCaseData
    {
        public string Name { get; set; } = string.Empty;
        public object[] TestParams { get; set; }
        public TestCaseData(string name, params object[] testParams)
        {
            Name = name;
            TestParams = testParams;
        }
    }
}
