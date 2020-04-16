using System;
using System.IO;
using System.Windows;
using EyeOfTheTagger.Events;
using EyeOfTheTagger.ViewDatas;

namespace EyeOfTheTagger
{
    /// <summary>
    /// Interaction logic for the console window.
    /// </summary>
    /// <seealso cref="Window"/>
    public partial class ConsoleWindow : Window
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ConsoleWindow()
        {
            InitializeComponent();
            LogsView.ItemsSource = ConsoleViewData.Default.Logs;
            ConsoleViewData.Default.AddLogHandler += delegate (object sender, AddLogEvent e)
            {
                Dispatcher.Invoke(() => LogsView.Items.Refresh());
            };
        }

        private void DumpLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConsoleViewData.Default.Logs.Count == 0)
            {
                MessageBox.Show("No logs to dump.", $"{Tools.GetAppName()} - information");
                return;
            }
            else if (!Directory.Exists(Properties.Settings.Default.DumpLogPath))
            {
                MessageBox.Show("Invalid log folder path. Please check your configuration.", $"{Tools.GetAppName()} - error");
                return;
            }
            else if (!Tools.HasWriteAccessToFolder(Properties.Settings.Default.DumpLogPath))
            {
                MessageBox.Show("The current user can't write dump file into the specified folder. Please check your configuration.", $"{Tools.GetAppName()} - error");
                return;
            }

            string filePath = Path.Combine(Properties.Settings.Default.DumpLogPath,
                $"{Tools.GetAppName()}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_logs.csv");

            Tools.DumpLogsIntoFile(filePath, ConsoleViewData.Default.Logs,
                () => MessageBox.Show($"Log file created:\r\n\r\n{filePath}", $"{Tools.GetAppName()} - information"),
                (string msg) => MessageBox.Show($"The following error occured while dumping:\r\n\r\n{msg}", $"{Tools.GetAppName()} - error"));
        }
    }
}
