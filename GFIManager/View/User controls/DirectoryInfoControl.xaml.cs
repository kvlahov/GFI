using GFIManager.Models;
using GFIManager.Properties;
using GFIManager.Services;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GFIManager.View.User_controls
{
    /// <summary>
    /// Interaction logic for DirectoryInfoControl.xaml
    /// </summary>
    public partial class DirectoryInfoControl : UserControl
    {
        public DirectoryInfoControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var service = new DirectoryService(Settings.Default.RootDir);

            AddTextBlocksToContainer(ExistingGfisContainer, service.GetCompaniesWithCreatedGfi());
            AddTextBlocksToContainer(MissingFilesContainer, service.GetCompaniesWithMissingFiles());
        }

        private void AddTextBlocksToContainer(TreeViewItem container, IEnumerable<Company> companies)
        {
            container.Items.Clear();
            companies
                .Select(c => new TreeViewItem() { Header = c.DisplayName })
                .ToList()
                .ForEach(tb => container.Items.Add(tb));
        }
    }
}