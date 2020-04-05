using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace EyeOfTheTagger
{
    public static class Tools
    {
        public static void ManageException(this Exception ex, params Tuple<string, string>[] additionalDatas)
        {
            if (ex != null)
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine(ex.Message);
                messageBuilder.AppendLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    messageBuilder.AppendLine($"Inner message - {ex.InnerException.Message}");
                }
                if (additionalDatas != null)
                {
                    foreach (Tuple<string, string> dataTuple in additionalDatas.Where(ad => !string.IsNullOrWhiteSpace(ad?.Item1)))
                    {
                        messageBuilder.AppendLine($"{dataTuple.Item1} - {dataTuple.Item2 ?? "<no value>"}");
                    }
                }
                MessageBox.Show(messageBuilder.ToString(), Constants.ErrorLabel, MessageBoxButton.OK);
            }
        }

        public static bool TrueEquals(this string s1, string s2)
        {
            return s1?.Trim()?.ToLowerInvariant() == s2?.Trim()?.ToLowerInvariant();
        }
    }
}
