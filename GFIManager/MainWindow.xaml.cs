﻿using GFIManager.Models;
using GFIManager.Properties;
using GFIManager.Services;
using GFIManager.View;
using GFIManager.View.User_controls;
using GFIManager.ViewModels;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GFIManager
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

            NotesControl.OnBackgroundWorkStart += () =>
            {
                ElapsedTimeContainer.Visibility = Visibility.Collapsed;
                Loader.Visibility = Visibility.Visible;
            };

            NotesControl.OnBackgroundWorkEnd += () =>
            {
                ElapsedTimeContainer.Visibility = Visibility.Visible;
                Loader.Visibility = Visibility.Hidden;
            };
        }

        private void LoadCompanies()
        {
            //load directories from root, which represent companies
            var rootDir = Settings.Default.RootDir;
            try
            {
                var service = new DirectoryService(rootDir);
                var companies = service.GetCompaniesWithoutNewGfi();
                ViewModel.SetCompanies(companies);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
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

        #region Dialogs

        private void ShowErrorMessage(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Greška",
                Content = message,
                CloseButtonText = "Ok",
                DefaultButton = ContentDialogButton.Close
            };

            dialog.ShowAsync();
        }

        private void ShowChooseFolderDialog()
        {
            var chooseFileDialog = new ChooseRootFolderView
            {
                ChosenFolder = Settings.Default.RootDir,
                Owner = this
            };

            chooseFileDialog.ShowDialog();

            if (string.IsNullOrEmpty(Settings.Default.RootDir))
            {
                Environment.Exit(0);
            }
        }

        private async Task ShowInfoDialog(string message, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok",
                DefaultButton = ContentDialogButton.Close
            };

            await dialog.ShowAsync();
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

        #endregion Dialogs

        #region Events

        private async void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            ShowChooseFolderDialog();
            LoadCompanies();
            await NotesControl.RefreshCompaniesAsync();
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e) => LbDirectories.SelectAll();

        private void BtnDeselectAll_Click(object sender, RoutedEventArgs e) => LbDirectories.UnselectAll();

        private async void BtnBuildGfi_Click(object sender, RoutedEventArgs e)
        {
            var selectedCompanies = LbDirectories.SelectedItems.Cast<Company>().ToList();
            var validCompanies = new DirectoryService(Settings.Default.RootDir).GetCompaniesWithoutNewGfi().Intersect(selectedCompanies);

            var skipCompanies = selectedCompanies.Except(validCompanies).Select(c => c.DisplayName);
            if (skipCompanies.Any())
            {
                var msg = $"Preskačem sljedeće firme jer nedostaje datoteka ili imaju već izrađen GFI: {string.Join(", ", skipCompanies)}";
                await ShowInfoDialog(msg, "Neispravne firme označene");
            }

            var service = new GfiBuilderService(validCompanies);

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
                HandleException(ex);
            }

            sw.Stop();
            dispatcherTimer.Stop();

            _ = Dispatcher.Invoke(async () =>
              {
                  Loader.Visibility = Visibility.Hidden;
                  var sb = new StringBuilder();
                  sb.AppendLine("Obrada završena");
                  sb.Append($"Proteklo vremena: {sw.ElapsedMilliseconds / 1000f}s");
                  await ShowInfoDialog(sb.ToString(), "Završeno");
                  LoadCompanies();

                  await NotesControl.RefreshCompaniesAsync();
              });
        }

        private void HandleException(Exception ex)
        {
            var messages = new List<string>();
            do
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }
            while (ex != null);
            var message = string.Join(Environment.NewLine, messages);

            Dispatcher.Invoke(() => ShowErrorMessage(message));
        }

        private async void BtnDirInfo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                IsShadowEnabled = true,
                Content = new DirectoryInfoControl(),
                Title = "Informacije o firmama",
                CloseButtonText = "Zatvori"
            };
            _ = await dialog.ShowAsync();
        }

        private void BtnRefreshDirs_Click(object sender, RoutedEventArgs e) => LoadCompanies();

        private void LbDirectories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.AreItemsSelected = LbDirectories.SelectedItems.Count > 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var option = ShowConfirmationDialog("Jeste li sigurni da želite izaći iz aplikacije?", "Izlazak");
            if (option == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        #endregion Events
    }
}