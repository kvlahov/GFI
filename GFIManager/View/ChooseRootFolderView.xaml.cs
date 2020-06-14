using GFIManager.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace GFIManager.View
{
    /// <summary>
    /// Interaction logic for ChooseRootFolderView.xaml
    /// </summary>
    public partial class ChooseRootFolderView : Window
    {
        private string _chosenFolder;

        public string ChosenFolder
        {
            get => _chosenFolder;
            set
            {
                _chosenFolder = value;
                TbChosenFolder.Text = value;
            }
        }

        public ChooseRootFolderView()
        {
            InitializeComponent();
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ChosenFolder))
            {
                Settings.Default.RootDir = ChosenFolder;
                Settings.Default.Save();
            }

            Close();
        }

        private void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ChosenFolder = dialog.FileName;
            }
        }
    }
}