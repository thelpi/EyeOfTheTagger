using System.Collections.Generic;
using System.Linq;

namespace EyeOfTheTagger
{
    /// <summary>
    /// Tool methods.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Checks the equality between two strings.
        /// <list type="bullet">
        /// <item><c>Null</c> versus <c>Null</c> is considered equal.</item>
        /// <item>Case insensitive.</item>
        /// <item>Strings are trimmed before comparison.</item>
        /// </list>
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns><c>True</c> if equals; <c>False</c> otherwise.</returns>
        public static bool TrueEquals(this string s1, string s2)
        {
            return s1?.Trim()?.ToLowerInvariant() == s2?.Trim()?.ToLowerInvariant();
        }

        /// <summary>
        /// Sets a clean <see cref="List{T}"/> from an <see cref="IEnumerable{T}"/> without reference duplicates.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="datas">The datas to add.</param>
        /// <param name="defaultData">Default value for the collection if empty; <c>Null</c> to let the collection empty.</param>
        /// <param name="hasDuplicates">Out; indicates if the source list contains duplicates.</param>
        /// <returns>The final collection; can't be <c>Null</c>.</returns>
        public static List<T> EnumerableToDistinctList<T>(this IEnumerable<T> datas, T defaultData, out bool hasDuplicates) where T : class
        {
            List<T> finalDatas = new List<T>();
            hasDuplicates = false;

            if (datas != null)
            {
                datas = datas.Where(d => d != null);
                finalDatas.AddRange(datas.Distinct());
                hasDuplicates = finalDatas.Count < datas.Count();
            }

            if (finalDatas.Count == 0 && defaultData != null)
            {
                finalDatas.Add(defaultData);
            }

            return finalDatas;
        }

        /// <summary>
        /// Parse a list of values stored in configuration as a single string.
        /// </summary>
        /// <param name="configurationValue">Configuration raw value.</param>
        /// <returns>List of values.</returns>
        public static List<string> ParseConfigurationList(string configurationValue)
        {
            return (configurationValue ?? string.Empty).Split(Constants.ConfigurationSeparator).ToList();
        }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        /// <returns>The application name.</returns>
        public static string GetAppName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}
