using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EyeOfTheTaggerLib.Enum;

namespace EyeOfTheTagger.Converters
{
    /// <summary>
    /// Transforms a <see cref="EyeOfTheTaggerLib.Enum.LogLevel"/> to a foreground brush.
    /// </summary>
    public class LogToForegroundConverter : IValueConverter
    {
        /// <summary>
        /// Proceeds to convert.
        /// </summary>
        /// <param name="value">The <see cref="EyeOfTheTaggerLib.Enum.LogLevel"/> value.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Foreground brush.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(LogLevel))
            {
                return Brushes.Black;
            }

            switch ((LogLevel)value)
            {
                case LogLevel.Critical:
                    return Brushes.Black;
                case LogLevel.Error:
                    return Brushes.Red;
                case LogLevel.Warning:
                    return Brushes.Orange;
                case LogLevel.Information:
                    return Brushes.Green;
                default:
                    return Brushes.Black;
            }
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
