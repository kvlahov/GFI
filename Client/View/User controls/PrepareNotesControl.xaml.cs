using GFIManager.Models;
using GFIManager.Properties;
using GFIManager.Services;
using GFIManager.ViewModels;
using ModernWpf.Controls;
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
            Task.Run(() => LoadCompaniesAsync().ConfigureAwait(false));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public async Task RefreshCompaniesAsync()
        {
            await LoadCompaniesAsync().ConfigureAwait(false);
        }

        private async Task LoadCompaniesAsync()
        {
            var service = new DirectoryService(Settings.Default.RootDir);

            var createdNotesTask = service.GetCompaniesWithCreatedNotes().ConfigureAwait(false);
            var invalidCompaniesTask = service.GetCompaniesWithInvalidGfi().ConfigureAwait(false);            

            var generatedNotesCompanies = await createdNotesTask;
            var invalidCompanies = await invalidCompaniesTask;
            var validCompanies = service.GetCompaniesWithCreatedGfi().Except(generatedNotesCompanies).Except(invalidCompanies);

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

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadCompaniesAsync().ConfigureAwait(false);
        }

        private async void BtnPrepareNotes_Click(object sender, RoutedEventArgs e)
        {
            var service = new NotesBuildingService(Settings.Default.RootDir);
            var notesToOverride = LbCreatedNotesCompanies.SelectedItems.Cast<Company>().ToList();
            var notesToAdd = LbValidCompanies.SelectedItems.Cast<Company>().ToList();

            var msg = GetOverrideMessage(notesToOverride);

            var answer = ShowConfirmationDialog(msg, "Prepisivanje podataka");

            if (answer == MessageBoxResult.Cancel) return;

            //Task addNotesTask = service.AddNotesForCompanies(notesToAdd);
            //Task updateNotesTask = service.UpdateNotesForCompanies(notesToOverride);

            //await Task.WhenAll(addNotesTask, updateNotesTask);

            ShowInfoDialog("Podaci za bilješke spremljeni.", "Kraj operacije");
        }

        private string GetOverrideMessage(List<Company> notesToOverride)
        {
            var sb = new StringBuilder("Odabrane firme se već nalaze u tablici za bilješke: ");
            sb.Append(Environment.NewLine);
            notesToOverride.Select(c => c.DisplayName)
                .ToList()
                .ForEach(c => sb.AppendLine(c));
            sb.Append("Podaci za odabrane firme će se prepisati. Jeste li sigurni da to želite?");
            return sb.ToString();
        }

        private MessageBoxResult ShowConfirmationDialog(string message, string title)
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Cancel
            );
        }

        private void ShowInfoDialog(string message, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok",
                DefaultButton = ContentDialogButton.Close
            };

            dialog.ShowAsync();
        }
    }
}
