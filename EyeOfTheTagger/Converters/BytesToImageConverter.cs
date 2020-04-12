using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EyeOfTheTagger.Converters
{
    /// <summary>
    /// Transforms a <see cref="IEnumerable{Byte}"/> to an <see cref="ImageSource"/>.
    /// </summary>
    public class BytesToImageConverter : IValueConverter
    {
        /// <summary>
        /// Proceeds to convert.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Foreground brush.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Tools.GetImageSourceFromDatas(value as IEnumerable<byte>);
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
