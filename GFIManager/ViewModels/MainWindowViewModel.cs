using GFIManager.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GFIManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<Company> Companies { get; private set; }

        private bool _areItemsSelected;

        public bool AreItemsSelected
        {
            get { return _areItemsSelected; }
            set { SetProperty(ref _areItemsSelected, value); }
        }

        public MainWindowViewModel()
        {
            Companies = new ObservableCollection<Company>();
        }

        public void SetCompanies(IEnumerable<Company> companies)
        {
            Companies.Clear();
            companies.ToList().ForEach(Companies.Add);
        }
    }
}