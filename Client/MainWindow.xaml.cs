using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using System.Windows.Threading;
using GFIManager.Models;
using GFIManager.Properties;
using GFIManager.Services;
using GFIManager.View;
using GFIManager.ViewModels;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainWindowViewModel();
            DataContext = ViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var rootDir = Settings.Default.RootDir;
            if (string.IsNullOrEmpty(rootDir))
            {
                ShowChooseFolderDialog();
            }

            LoadCompanies();
        }

        private void LoadCompanies()
        {
            //load directories from root, which represent companies
            var rootDir = Settings.Default.RootDir;
            try
            {
                var companies = Directory.GetDirectories(rootDir)
                        .Select(dir => new Company(dir));

                ViewModel.SetCompanies(companies);
            }

            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK);
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowChooseFolderDialog();
        }

        private void ShowChooseFolderDialog()
        {
            var chooseFileDialog = new ChooseRootFolderView
            {
                ChosenFolder = Settings.Default.RootDir
            };

            chooseFileDialog.ShowDialog();

            if (string.IsNullOrEmpty(Settings.Default.RootDir))
            {
                Environment.Exit(0);
            }
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e) => LbDirectories.SelectAll();

        private void BtnDeselectAll_Click(object sender, RoutedEventArgs e) => LbDirectories.UnselectAll();

        private async void BtnBuildGfi_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompanies = LbDirectories.SelectedItems.Cast<Company>().ToList();
            var service = new GfiCreatorService(selectedCompanies);

            Loader.Visibility = Visibility.Visible;

            var sw = Stopwatch.StartNew();
            var dispatcherTimer = PrepareTimer(sw);
            dispatcherTimer.Start();
            try
            {
                await Task.Run(() => service.BuildGfis()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }

            sw.Stop();
            dispatcherTimer.Stop();

            Dispatcher.Invoke(() => Loader.Visibility = Visibility.Hidden);

            MessageBox.Show($"Elapsed time: {sw.ElapsedMilliseconds / 1000}s", "Elapsed time");

        }

        private DispatcherTimer PrepareTimer(Stopwatch sw)
        {
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += (sender, e) => DispatcherTimer_Tick(sw.ElapsedMilliseconds);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            return dispatcherTimer;
        }

        private void DispatcherTimer_Tick(long elapsedMiliseconds)
        {
            var elapsedSeconds = TimeSpan.FromMilliseconds(elapsedMiliseconds);
            LbElapsedTime.Text = elapsedSeconds.ToString(@"mm\:ss");
            CommandManager.InvalidateRequerySuggested();
        }

        private void LbDirectories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.AreItemsSelected = LbDirectories.SelectedItems.Count > 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var option = MessageBox.Show("Jeste li sigurni da želite izaći iz aplikacije?", "Izlazak", MessageBoxButton.OKCancel);
            if(option == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}
