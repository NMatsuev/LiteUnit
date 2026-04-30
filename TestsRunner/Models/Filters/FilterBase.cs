namespace TestsRunner.Models.Filters
{
    public abstract class FilterBase
    {
        public abstract string Name { get; }
        public virtual IEnumerable<string> PossibleValues { get;  set; } = new List<string>() {"Все"};
        public virtual string SelectedValue { get; set; } = "Все";
        public abstract void LoadPossibleValues(IEnumerable<TestAssemblyModel> testAssemblyModels);
        public abstract bool Filter (TestMethodModel testMethod);
    }
}
