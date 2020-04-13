using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EyeOfTheTaggerLib.Datas;

namespace EyeOfTheTagger
{
    /// <summary>
    /// Tool methods.
    /// </summary>
    public static class Tools
    {
        private const char _CONFIGURATION_SEPARATOR = ';';
        // Do not use directly.
        private static BitmapImage _defaultImageSource = null;

        /// <summary>
        /// Parse a list of values stored in configuration as a single string.
        /// </summary>
        /// <param name="configurationValue">Configuration raw value.</param>
        /// <returns>List of values.</returns>
        public static List<string> ParseConfigurationList(string configurationValue)
        {
            return (configurationValue ?? string.Empty).Split(_CONFIGURATION_SEPARATOR).ToList();
        }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        /// <returns>The application name.</returns>
        public static string GetAppName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }

        /// <summary>
        /// Checks if the current user can write files into the specified folder.
        /// </summary>
        /// <param name="folderPath">The folder path to check.</param>
        /// <returns><c>True</c> if the user can write files into the folder; <c>False</c> otherwise.</returns>
        public static bool HasWriteAccessToFolder(string folderPath)
        {
            try
            {
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get an <see cref="ImageSource"/> from bytes.
        /// If no data or failure, gets <see cref="Properties.Resources.cdaudio_unmount"/>.
        /// </summary>
        /// <param name="datas">Bytes.</param>
        /// <returns><see cref="ImageSource"/></returns>
        public static ImageSource GetImageSourceFromDatas(IEnumerable<byte> datas)
        {
            ImageSource source = null;

            if (datas != null && datas.Count() > 0)
            {
                System.Drawing.Image img = null;
                try
                {
                    using (MemoryStream ms1 = new MemoryStream(datas.ToArray()),
                        ms2 = new MemoryStream())
                    {
                        img = System.Drawing.Image.FromStream(ms1);

                        img.Save(ms2, ImageFormat.Bmp);
                        ms2.Seek(0, SeekOrigin.Begin);

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = ms2;
                        bitmapImage.EndInit();

                        source = bitmapImage;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Parse image failure: {ex.Message}");
                }
            }

            if (source == null)
            {
                source = GetDefaultImageSource();
            }

            return source;
        }

        private static BitmapImage GetDefaultImageSource()
        {
            if (_defaultImageSource == null)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    Properties.Resources.cdaudio_unmount.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    _defaultImageSource = new BitmapImage();
                    _defaultImageSource.BeginInit();
                    _defaultImageSource.StreamSource = memory;
                    _defaultImageSource.CacheOption = BitmapCacheOption.OnLoad;
                    _defaultImageSource.EndInit();
                }
            }

            return _defaultImageSource;
        }

        /// <summary>
        /// Proceeds to dump logs into a file asynchronously.
        /// </summary>
        /// <param name="filePath">Logs file path.</param>
        /// <param name="logs">List of <see cref="LogData"/>.</param>
        /// <param name="onSuccessCallback">Method to call on success.</param>
        /// <param name="onFailureCallback">Method to call on failure; the argument is the exception message.</param>
        public static void DumpLogsIntoFile(string filePath, IEnumerable<LogData> logs,
            Action onSuccessCallback, Action<string> onFailureCallback)
        {
            var dumpWorker = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            // TODO: GUID should be disabled while processing.
            dumpWorker.DoWork += delegate (object subSender, DoWorkEventArgs subE)
            {
                using (var sw = new StreamWriter(filePath, false))
                {
                    sw.WriteLine($"Date\tType\tMessage");
                    foreach (LogData log in subE.Argument as IEnumerable<LogData>)
                    {
                        sw.WriteLine($"{log.Date.ToString("dd/MM/yyyy HH:mm:ss")}\t{log.Level}\t{log.Message}");
                        foreach (string adKey in log.AdditionalDatas.Keys)
                        {
                            sw.WriteLine($"{log.Date.ToString("dd/MM/yyyy HH:mm:ss")}\t{adKey}\t{log.AdditionalDatas[adKey]}");
                        }
                    }
                }
            };
            dumpWorker.RunWorkerCompleted += delegate (object subSender, RunWorkerCompletedEventArgs subE)
            {
                if (subE.Error != null)
                {
                    onFailureCallback.Invoke(subE.Error.Message);
                }
                else
                {
                    onSuccessCallback.Invoke();
                }
            };
            dumpWorker.RunWorkerAsync(logs);
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> associated to <paramref name="propertyName"/> for the specified type.
        /// Cannot fail.
        /// </summary>
        /// <typeparam name="T">The targeted type.</typeparam>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The <see cref="PropertyInfo"/> or <c>Null</c>.</returns>
        public static PropertyInfo GetProperty<T>(string propertyName)
        {
            try
            {
                return typeof(T).GetProperty(propertyName);
            }
            catch
            {
                return null;
            }
        }
    }
}
