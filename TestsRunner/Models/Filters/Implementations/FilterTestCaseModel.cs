namespace TestsRunner.Models.Filters.Implementations
{
    public class FilterTestCaseModel : FilterBase
    {
        public override string Name => "Наличие тест-кейсов";

        public override bool Filter(TestMethodModel testMethod)
        {
            if (SelectedValue == "Все")
                return true;

            return testMethod.IsParameterized == (SelectedValue == "Да");
        }

        public override void LoadPossibleValues(IEnumerable<TestAssemblyModel> testAssemblyModels)
        {
            PossibleValues = ["Все", "Да", "Нет"];
        }
    }
}
