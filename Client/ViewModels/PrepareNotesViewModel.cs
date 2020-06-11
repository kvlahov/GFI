using GFIManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GFIManager.ViewModels
{
    public class PrepareNotesViewModel : ViewModelBase
    {
		public ObservableCollection<Company> ValidCompanies { get; private set; }
		public ObservableCollection<Company> InvalidCompanies { get; private set; }
		public ObservableCollection<Company> GeneratedNotesCompanies { get; private set; }
		
		private bool _areItemsSelected;
		public bool AreItemsSelected
		{
			get { return _areItemsSelected; }
			set { SetProperty(ref _areItemsSelected,  value); }
		}

		public PrepareNotesViewModel()
		{
			ValidCompanies = new ObservableCollection<Company>();
			InvalidCompanies = new ObservableCollection<Company>();
			GeneratedNotesCompanies = new ObservableCollection<Company>();
		}

		public void SetValidCompanies(IEnumerable<Company> companies)
		{
			ValidCompanies.Clear();
			companies.ToList().ForEach(ValidCompanies.Add);
		}

		public void SetInvalidCompanies(IEnumerable<Company> companies)
		{
			InvalidCompanies.Clear();
			companies.ToList().ForEach(InvalidCompanies.Add);
		}
		public void SetGeneratedNotesCompanies(IEnumerable<Company> companies)
		{
			GeneratedNotesCompanies.Clear();
			companies.ToList().ForEach(GeneratedNotesCompanies.Add);
		}

	}
}
