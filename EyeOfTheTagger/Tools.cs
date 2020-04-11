using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EyeOfTheTagger
{
    /// <summary>
    /// Tool methods.
    /// </summary>
    public static class Tools
    {
        private const char _CONFIGURATION_SEPARATOR = ';';

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
    }
}
