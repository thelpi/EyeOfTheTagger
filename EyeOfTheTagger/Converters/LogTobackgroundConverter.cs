using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EyeOfTheTaggerLib.Enum;

namespace EyeOfTheTagger.Converters
{
    /// <summary>
    /// Transforms a <see cref="EyeOfTheTaggerLib.Enum.LogLevel"/> to a background brush.
    /// </summary>
    public class LogToBackgroundConverter : IValueConverter
    {
        /// <summary>
        /// Proceeds to convert.
        /// </summary>
        /// <param name="value">The <see cref="EyeOfTheTaggerLib.Enum.LogLevel"/> value.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Background brush.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(LogLevel))
            {
                return Brushes.Transparent;
            }

            return (LogLevel)value == LogLevel.Critical ? Brushes.Red : Brushes.Transparent;
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Not implemented.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
