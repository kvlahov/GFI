using GFIManager.Properties;
using GFIManager.Services;
using GFIManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GFIManager.View.User_controls
{
    /// <summary>
    /// Interaction logic for PrepareNotesControl.xaml
    /// </summary>
    public partial class PrepareNotesControl : UserControl
    {
        public PrepareNotesViewModel ViewModel { get; private set; }
        public PrepareNotesControl()
        {
            InitializeComponent();
            ViewModel = new PrepareNotesViewModel();
            DataContext = ViewModel;
            LoadCompaniesAsync();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private async void LoadCompaniesAsync()
        {
            var service = new DirectoryService(Settings.Default.RootDir);

            var createdNotesTask = service.GetCompaniesWithCreatedNotes().ConfigureAwait(false);
            var invalidCompaniesTask = service.GetCompaniesWithInvalidGfi().ConfigureAwait(false);
            var validCompaniesTask = service.GetCompaniesWithoutNotes().ConfigureAwait(false);

            var generatedNotesCompanies = await createdNotesTask;
            var invalidCompanies = await invalidCompaniesTask;
            var validCompanies = await validCompaniesTask;
            
            Dispatcher.Invoke(() =>
            {
                ViewModel.SetGeneratedNotesCompanies(generatedNotesCompanies);
                ViewModel.SetInvalidCompanies(invalidCompanies);
                ViewModel.SetValidCompanies(validCompanies);
            });
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e) => LbValidCompanies.SelectAll();

        private void BtnDeselectAll_Click(object sender, RoutedEventArgs e) => LbValidCompanies.UnselectAll();

        private void MultiselectListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.AreItemsSelected = LbValidCompanies.SelectedItems.Count > 0 || LbCreatedNotesCompanies.SelectedItems.Count > 0;
        }
    }
}
