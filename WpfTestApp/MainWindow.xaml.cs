using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WpfTestApp.Extensions;
using WpfTestApp.Model;
using MessageBox = System.Windows.MessageBox;

namespace WpfTestApp
{
    public partial class MainWindow : Window
    {
        public UIModel Model { get; private set; }

        public MainWindow()
        {
            Model = new UIModel();
            InitializeComponent();
        }

        private void Initialize()
        {
            Model.InvokeBackgroundAction(Model.JobRunner.Start);

            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastKeyword))
            {
                Model.Keyword = Properties.Settings.Default.LastKeyword;
            }
            if (!String.IsNullOrEmpty(Properties.Settings.Default.LastDirectory))
            {
                Model.DirectoryToSearch = Properties.Settings.Default.LastDirectory;
            }
        }

        private void PromptForSearchDirectory()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select search path";
                dialog.SelectedPath = Model.DirectoryToSearch;
                
                if (dialog.ShowDialog(this.GetWin32Window()) == System.Windows.Forms.DialogResult.OK)
                {
                    Model.DirectoryToSearch = dialog.SelectedPath;
                }
            }
        }

        private void PerformSearch()
        {
            this.InvokeOnDispatcher(async () =>
            {
                Model.Busy = true;
                Model.AddLog("Queuing search");

                try
                {
                    await Model.InvokeBackgroundAction(() =>
                    {
                        Model.BeginOperations();
                        foreach (var file in Directory.GetFiles(Model.DirectoryToSearch, "*.*", SearchOption.AllDirectories))
                        {
                            Model.JobRunner.AddJob(new Job(file, Model.Keyword));
                        }
                    });
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show(this, "Comparison was cancelled by user.", "User Cancelled Comparison",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show(this, message, "Comparison Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Model.Busy = false;
                }
            });
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Model.InvokeBackgroundAction(Model.CancelOperations);

            Properties.Settings.Default.LastKeyword = Model.Keyword;
            Properties.Settings.Default.LastDirectory = Model.DirectoryToSearch;
            Properties.Settings.Default.Save();
        }

        private void DirectoryBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            PromptForSearchDirectory();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Model.InvokeBackgroundAction(Model.CancelOperations);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }
    }
}
