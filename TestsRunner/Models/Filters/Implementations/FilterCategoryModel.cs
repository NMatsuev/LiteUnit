namespace TestsRunner.Models.Filters.Implementations
{
    public class FilterCategoryModel : FilterBase
    {
        public override string Name => "Категория";

        public override bool Filter(TestMethodModel testMethod)
        {
            if (SelectedValue == "Все")
                return true;

            if (testMethod.Categories == null)
                return false;

            return testMethod.Categories.Contains(SelectedValue);
        }

        public override void LoadPossibleValues(IEnumerable<TestAssemblyModel> testAssemblyModels)
        {
            HashSet<string> result = new (){ "Все" };

            foreach (var testAssemblyModel in testAssemblyModels)
                foreach (var testClassModel in testAssemblyModel.Classes)
                    foreach (var testMethodModel in testClassModel.Methods)
                    {
                        if (testMethodModel.Categories != null)
                            foreach (var category in testMethodModel.Categories)
                                result.Add(category);
                    }

            PossibleValues = result.ToList();
        }
    }
}
