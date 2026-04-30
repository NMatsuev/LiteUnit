using TestsRunner.Models;
using TestsRunner.Models.Filters;

namespace TestsRunner.ViewModels.Implementations
{
    public class FilterViewModel : ViewModelBase
    {
        private readonly FilterBase _filter;

        public FilterViewModel(FilterBase filter)
        {
            _filter = filter;
        }

        public string Name => _filter.Name;

        public IEnumerable<string> PossibleValues
        {
            get => _filter.PossibleValues;
            set
            {
                if (!Equals(_filter.PossibleValues, value))
                {
                    _filter.PossibleValues = value;
                    OnPropertyChanged(nameof(PossibleValues));
                }
            }
        }

        public string SelectedValue
        {
            get => _filter.SelectedValue;
            set
            {
                if (_filter.SelectedValue != value)
                {
                    _filter.SelectedValue = value;
                    OnPropertyChanged(nameof(SelectedValue));
                }
            }
        }

        public bool Filter(TestMethodModel testMethod) => _filter.Filter(testMethod);

        public void LoadPossibleValues(IEnumerable<TestAssemblyModel> assemblies)
        {
            _filter.LoadPossibleValues(assemblies);
            OnPropertyChanged(nameof(PossibleValues));
        }
    }
}
