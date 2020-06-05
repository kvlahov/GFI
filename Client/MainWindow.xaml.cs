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

        private void BtnBuildGfi_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompanies = LbDirectories.SelectedItems.Cast<Company>().ToList();
            var service = new GfiCreatorService(selectedCompanies);

            service.BuildGfis();
            
        }

        private void LbDirectories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.AreItemsSelected = LbDirectories.SelectedItems.Count > 0;
        }
    }
}
