using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Linq;

namespace EyeOfTheTagger.Converters
{
    /// <summary>
    /// Transforms a <see cref="IEnumerable{T}"/> to a displayable string.
    /// </summary>
    public class ListToCellConverter : IValueConverter
    {
        private const string _SEPARATOR = "; ";

        /// <summary>
        /// Proceeds to convert.
        /// </summary>
        /// <param name="value">The <see cref="IEnumerable{T}"/> value.</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>Displayable string.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IEnumerable<object>))
            {
                return string.Empty;
            }

            List<string> displayableStrings = (value as IEnumerable<object>)
                                                .Where(o => o != null)
                                                .Select(o => o.ToString().Trim())
                                                .Distinct()
                                                .ToList();

            return string.Join(_SEPARATOR, displayableStrings);
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
