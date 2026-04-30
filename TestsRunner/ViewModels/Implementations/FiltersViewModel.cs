using System.Diagnostics;
using System.Reflection;
using TestsRunner.Models;
using TestsRunner.Models.Filters;

namespace TestsRunner.ViewModels.Implementations
{
    public class FiltersViewModel : ViewModelBase
    {
        public IEnumerable<FilterViewModel> Filters { get; init; } = LoadFilters();
        private bool _isFiltersEnabled;
        public bool IsFiltersEnabled
        {
            get { return _isFiltersEnabled; }
            set { SetProperty(ref _isFiltersEnabled, value); }
        }

        public void UpdateFiltersPossibleValues(IEnumerable<TestAssemblyModel> testAssemblyModels)
        {
            foreach (var filter in Filters)
                filter.LoadPossibleValues(testAssemblyModels);
        }
        private static IEnumerable<FilterViewModel> LoadFilters()
        {
            try
            {
                var currentAssembly = Assembly.GetExecutingAssembly();

                var filterTypes = currentAssembly.GetTypes()
                    .Where(type => type.IsClass &&                        
                                  !type.IsAbstract &&                       
                                  type.IsSubclassOf(typeof(FilterBase)))    
                    .ToList();

                var filters = new List<FilterViewModel>();
                foreach (var filterType in filterTypes)
                {
                    try
                    {
                        var filter = (FilterBase)Activator.CreateInstance(filterType);
                        if (filter != null)
                            filters.Add(new FilterViewModel(filter));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка создания фильтра {filterType.Name}: {ex.Message}");
                    }
                }

                return filters;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка загрузки фильтров: {ex.Message}");
                return Enumerable.Empty<FilterViewModel>();
            }
        }

    }
}
