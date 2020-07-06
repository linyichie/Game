using System;
using System.Text;

public static class StringUtil
{
    public readonly static string[] splitSeparator = new string[] { "|" };

    static StringBuilder stringBuilder = new StringBuilder();

    static object lockObject = new object();

    public static string Concat(params object[] objects) {
        if (objects == null) {
            return string.Empty;
        }

        lock (lockObject) {
            stringBuilder.Length = 0;
            foreach (var item in objects) {
                if (item != null) {
                    stringBuilder.Append(item);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
