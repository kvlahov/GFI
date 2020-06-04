using GFIManager.Properties;
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
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GFIManager.View
{
    /// <summary>
    /// Interaction logic for ChooseRootFolderView.xaml
    /// </summary>
    public partial class ChooseRootFolderView : Window
    {
        public string ChosenFolder { get; set; }
        public ChooseRootFolderView()
        {
            InitializeComponent();
        }

        private void BtnSaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChosenFolder))
            {
                Settings.Default.RootDir = ChosenFolder;
                Settings.Default.Save();
            }
        }

        private void BtnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            //dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ChosenFolder = dialog.FileName;
                MessageBox.Show("You selected: " + dialog.FileName);
            }
        }
    }
}
