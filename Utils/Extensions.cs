#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

public static partial class Extensions
{
    public static System.Random rnd = new System.Random();

    public enum BooleanStringMode
    {
        Numbers,
        TrueFalse,
        EnabledDisabled,
        OnOff,
        YesNo,
    }

    #region object
    public static Dictionary<string, object> ToDictionary(this object instanceToConvert)
    {
        return instanceToConvert
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .ToDictionary(
                propertyInfo => propertyInfo.Name,
                propertyInfo => Extensions.ConvertPropertyToDictionary(propertyInfo, instanceToConvert)
            );
    }

    public static object ConvertPropertyToDictionary(PropertyInfo propertyInfo, object owner)
    {
        Type propertyType = propertyInfo.PropertyType;
        object propertyValue = propertyInfo.GetValue(owner);
        if (
            !propertyType.Equals(typeof(string))
            && (
                typeof(ICollection<>).Name.Equals(propertyValue.GetType().BaseType.Name)
                || typeof(Collection<>).Name.Equals(propertyValue.GetType().BaseType.Name)
            )
        )
        {
            var collectionItems = new List<Dictionary<string, object>>();
            var count = (int)propertyType.GetProperty("Count").GetValue(propertyValue);
            PropertyInfo indexerProperty = propertyType.GetProperty("Item");
            for (var index = 0; index < count; index++)
            {
                object item = indexerProperty.GetValue(propertyValue, new object[] { index });
                PropertyInfo[] itemProperties = item.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                if (itemProperties.Any())
                {
                    Dictionary<string, object> dictionary = itemProperties.ToDictionary(
                        subtypePropertyInfo => subtypePropertyInfo.Name,
                        subtypePropertyInfo => Extensions.ConvertPropertyToDictionary(subtypePropertyInfo, item)
                    );
                    collectionItems.Add(dictionary);
                }
            }
            return collectionItems;
        }
        if (propertyType.IsPrimitive || propertyType.Equals(typeof(string)))
        {
            return propertyValue;
        }
        PropertyInfo[] properties = propertyType.GetProperties(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance
        );
        if (properties.Any())
        {
            return properties.ToDictionary(
                subtypePropertyInfo => subtypePropertyInfo.Name,
                subtypePropertyInfo =>
                    (object)Extensions.ConvertPropertyToDictionary(subtypePropertyInfo, propertyValue)
            );
        }
        return propertyValue;
    }


    #endregion

    #region DateTime
    public static bool ExpiredSince(this DateTime dateTime, int minutes)
    {
        return (dateTime - DateTime.Now).TotalMinutes < minutes;
    }
    #endregion

    #region TimeSpan
    public static TimeSpan StripMilliseconds(this TimeSpan time)
    {
        return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
    }


    #endregion

    #region DirectoryInfo
    public static DirectoryInfo Combine(this DirectoryInfo dir, params string[] paths)
    {
        var final = dir.FullName;
        foreach (var path in paths)
        {
            final = Path.Combine(final, path);
        }
        return new DirectoryInfo(final);
    }

    public static bool IsEmpty(this DirectoryInfo directory)
    {
        return !Directory.EnumerateFileSystemEntries(directory.FullName).Any();
    }

    public static string StatusString(this DirectoryInfo directory, bool existsInfo = false)
    {
        if (directory is null)
            return " (is null ❌)";
        if (File.Exists(directory.FullName))
            return " (is file ❌)";
        if (!directory.Exists)
            return " (does not exist ❌)";
        if (directory.IsEmpty())
            return " (is empty ⚠️)";
        return existsInfo ? " (exists ✅)" : string.Empty;
    }

    public static void Copy(this DirectoryInfo source, DirectoryInfo target, bool overwrite = false)
    {
        Directory.CreateDirectory(target.FullName);
        foreach (FileInfo fi in source.GetFiles())
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), overwrite);
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            Copy(diSourceSubDir, target.CreateSubdirectory(diSourceSubDir.Name));
    }

    public static bool Backup(this DirectoryInfo directory, bool overwrite = false)
    {
        if (!directory.Exists)
            return false;
        var backupDirPath = directory.FullName + ".bak";
        if (Directory.Exists(backupDirPath) && !overwrite)
            return false;
        Directory.CreateDirectory(backupDirPath);
        foreach (FileInfo fi in directory.GetFiles())
            fi.CopyTo(Path.Combine(backupDirPath, fi.Name), overwrite);
        foreach (DirectoryInfo diSourceSubDir in directory.GetDirectories())
        {
            diSourceSubDir.Copy(Directory.CreateDirectory(Path.Combine(backupDirPath, diSourceSubDir.Name)), overwrite);
        }
        return true;
    }

    public static FileInfo CombineFile(this DirectoryInfo dir, params string[] paths)
    {
        var final = dir.FullName;
        foreach (var path in paths)
        {
            final = Path.Combine(final, path);
        }
        return new FileInfo(final);
    }



    public static string str(this DirectoryInfo directory)
    {
        return "\"" + directory.FullName + "\"";
    }
    #endregion

    #region FileInfo
    public static FileInfo Combine(this FileInfo file, params string[] paths)
    {
        var final = file.DirectoryName;
        foreach (var path in paths)
        {
            final = Path.Combine(final, path);
        }
        return new FileInfo(final);
    }

    public static string FileNameWithoutExtension(this FileInfo file)
    {
        return Path.GetFileNameWithoutExtension(file.Name);
    }

    public static string StatusString(this FileInfo file, bool existsInfo = false)
    {
        if (file is null)
            return "(is null ❌)";
        if (Directory.Exists(file.FullName))
            return "(is directory ❌)";
        if (!file.Exists)
            return "(does not exist ❌)";
        if (file.Length < 1)
            return "(is empty ⚠️)";
        return existsInfo ? "(exists ✅)" : string.Empty;
    }

    public static void AppendLine(this FileInfo file, string line)
    {
        try
        {
            if (!file.Exists)
                file.Create();
            File.AppendAllLines(file.FullName, new string[] { line });
        }
        catch { }
    }

    public static string ReadAllText(this FileInfo file) => File.ReadAllText(file.FullName);

    public static List<string> ReadAllLines(this FileInfo file) => File.ReadAllLines(file.FullName).ToList();

    public static bool Backup(this FileInfo file, bool overwrite = false)
    {
        if (!file.Exists)
            return false;
        var backupFilePath = file.FullName + ".bak";
        if (File.Exists(backupFilePath) && !overwrite)
            return false;
        File.Copy(file.FullName, backupFilePath, overwrite);
        return true;
    }

    public static bool Restore(this FileInfo file, bool overwrite = false)
    {
        if (!file.Exists || !File.Exists(file.FullName + ".bak"))
            return false;
        if (overwrite)
            File.Delete(file.FullName);
        File.Move(file.FullName + ".bak", file.FullName);
        return true;
    }

    public static bool WriteAllText(this FileInfo file, string content)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));
        if (file.Directory != null && !file.Directory.Exists)
            file.Directory.Create();
        try
        {
            File.WriteAllText(file.FullName, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file {file.FullName}: {ex.Message}");
            return false;
        }
        return true;
    }

    public static string str(this FileInfo file)
    {
        return "\"" + file.FullName + "\"";
    }

    public static string Extension(this FileInfo file)
    {
        return Path.GetExtension(file.Name);
    }

    public static bool IsDisabled(this FileInfo file) => file.FullName.EndsWith(".disabled");

    public static void Disable(this FileInfo file)
    {
        if (!file.IsDisabled())
            file.MoveTo(file.FullName + ".disabled");
    }

    public static void Enable(this FileInfo file)
    {
        if (file.IsDisabled())
            file.MoveTo(file.FullName.TrimEnd(".disabled"));
    }

    public static void Toggle(this FileInfo file)
    {
        if (file.IsDisabled())
            file.Enable();
        else
            file.Disable();
    }

    public static FileInfo CopyTo(this FileInfo file, FileInfo destination, bool overwrite = true)
    {
        try
        {
            return file.CopyTo(destination.FullName, overwrite);
        }
        catch (IOException)
        {
            return destination;
        }
    }

    public static void WriteAllText(this FileInfo file, string text, bool overwrite = true)
    {
        file.Directory.Create();
        if (file.Exists && !overwrite)
            return;
        if (!file.Exists)
            file.Create().Close();
        File.WriteAllText(file.FullName, text);
    }

    public static string RunWait(this FileInfo file, params string[] args)
    {
        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.FileName = file.FullName;
        p.StartInfo.Arguments = string.Join(" ", args);
        p.Start();
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        return output;
    }


    #endregion

    #region string
    public static IEnumerable<string> SplitToLines(this string input)
    {
        if (input == null)
        {
            yield break;
        }
        using (System.IO.StringReader reader = new System.IO.StringReader(input))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }

    public static string ToTitleCase(this string source, string langCode = "en-US")
    {
        return new CultureInfo(langCode, false).TextInfo.ToTitleCase(source);
    }

    public static bool Contains(this string source, string toCheck, StringComparison comp)
    {
        return source?.IndexOf(toCheck, comp) >= 0;
    }

    public static bool IsNullOrEmpty(this string source)
    {
        return string.IsNullOrEmpty(source);
    }

    public static string[] Split(
        this string source,
        string split,
        int count = -1,
        StringSplitOptions options = StringSplitOptions.None
    )
    {
        if (count != -1)
            return source.Split(new string[] { split }, count, options);
        return source.Split(new string[] { split }, options);
    }

    public static string Remove(this string Source, string Replace)
    {
        return Source.Replace(Replace, string.Empty);
    }

    public static string ReplaceLastOccurrence(this string Source, string Find, string Replace)
    {
        int place = Source.LastIndexOf(Find);
        if (place == -1)
            return Source;
        string result = Source.Remove(place, Find.Length).Insert(place, Replace);
        return result;
    }

    public static string EscapeLineBreaks(this string source)
    {
        return Regex.Replace(source, @"\r\n?|\n", @"\$&");
    }

    public static string Ext(this string text, string extension)
    {
        return text + "." + extension;
    }

    public static string Quote(this string text)
    {
        return SurroundWith(text, "\"");
    }

    public static string Enclose(this string text)
    {
        return SurroundWith(text, "(", ")");
    }

    public static string Brackets(this string text)
    {
        return SurroundWith(text, "[", "]");
    }

    public static string SurroundWith(this string text, string surrounds)
    {
        return surrounds + text + surrounds;
    }

    public static string SurroundWith(this string text, string starts, string ends)
    {
        return starts + text + ends;
    }

    public static string GetDigits(this string input)
    {
        return new string(input.Where(char.IsDigit).ToArray());
    }

    public static string Format(this string input, params string[] args)
    {
        return string.Format(input, args);
    }

    public static bool IsNullOrWhiteSpace(this string source)
    {
        return string.IsNullOrWhiteSpace(source);
    }



    public static string ToMd5(this string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }



    public static bool ToBoolean(this string input)
    {
        var stringTrueValues = new[] { "true", "ok", "yes", "1", "y", "enabled", "on" };
        return stringTrueValues.Contains(input.ToLower());
    }

    public static string ReplaceFirst(this string text, string search, string replace)
    {
        int num = text.IndexOf(search);
        return num < 0 ? text : text.Substring(0, num) + replace + text.Substring(num + search.Length);
    }

    public static string str(this string value)
    {
        return value.Quote();
    }

    public static bool IsHash(this string source)
    {
        return source.StartsWith("0x");
    }





    public static string TrimStart(
        this string inputText,
        string value,
        StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase
    )
    {
        if (!string.IsNullOrEmpty(value))
        {
            while (!string.IsNullOrEmpty(inputText) && inputText.StartsWith(value, comparisonType))
            {
                inputText = inputText.Substring(value.Length - 1);
            }
        }
        return inputText;
    }

    public static string TrimEnd(
        this string inputText,
        string value,
        StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase
    )
    {
        if (!string.IsNullOrEmpty(value))
        {
            while (!string.IsNullOrEmpty(inputText) && inputText.EndsWith(value, comparisonType))
            {
                inputText = inputText.Substring(0, (inputText.Length - value.Length));
            }
        }
        return inputText;
    }

    public static string Trim(
        this string inputText,
        string value,
        StringComparison comparisonType = StringComparison.CurrentCultureIgnoreCase
    )
    {
        return TrimStart(TrimEnd(inputText, value, comparisonType), value, comparisonType);
    }
    #endregion

    #region IDictionary<string, string>
    public static void AddIfNotExists(this IDictionary<string, string> dictionary, string key, string value)
    {
        if (!dictionary.ContainsKey(key))
            dictionary.Add(key, value);
    }
    #endregion

    #region Dictionary<string, object?>
    public static Dictionary<string, object?> MergeWith(
        this Dictionary<string, object?> sourceDict,
        Dictionary<string, object?> destDict
    )
    {
        foreach (var kvp in sourceDict)
        {
            if (destDict.ContainsKey(kvp.Key))
            {
                Console.WriteLine($"Key '{kvp.Key}' already exists and will be overwritten.");
            }
            destDict[kvp.Key] = kvp.Value;
        }
        return destDict;
    }
    #endregion

    #region Dictionary<string, object>
    public static Dictionary<string, object?> MergeRecursiveWith(
        this Dictionary<string, object?> sourceDict,
        Dictionary<string, object?> targetDict
    )
    {
        foreach (var kvp in sourceDict)
        {
            if (targetDict.TryGetValue(kvp.Key, out var existingValue))
            {
                if (
                    existingValue is Dictionary<string, object?> existingDict
                    && kvp.Value is Dictionary<string, object?> sourceDictValue
                )
                {
                    sourceDictValue.MergeRecursiveWith(existingDict);
                }
                else if (kvp.Value is null)
                {
                    targetDict.Remove(kvp.Key);
                    Console.WriteLine($"Removed key '{kvp.Key}' as it was set to null in the source dictionary.");
                }
                else
                {
                    targetDict[kvp.Key] = kvp.Value;
                    Console.WriteLine($"Overwriting existing value for key '{kvp.Key}'.");
                }
            }
            else
            {
                targetDict[kvp.Key] = kvp.Value;
            }
        }
        return targetDict;
    }
    #endregion

    #region NameValueCollection
    public static string ToQueryString(this NameValueCollection nvc)
    {
        if (nvc == null)
            return string.Empty;
        StringBuilder sb = new StringBuilder();
        foreach (string key in nvc.Keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;
            string[]? values = nvc.GetValues(key);
            if (values == null)
                continue;
            foreach (string value in values)
            {
                sb.Append(sb.Length == 0 ? "?" : "&");
                sb.AppendFormat("{0}={1}", key, value);
            }
        }
        return sb.ToString();
    }

    public static bool GetBool(this NameValueCollection collection, string key, bool defaultValue = false)
    {
        if (!collection.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase))
            return false;
        var trueValues = new string[] { true.ToString(), "yes", "1" };
        if (trueValues.Contains(collection[key], StringComparer.OrdinalIgnoreCase))
            return true;
        var falseValues = new string[] { false.ToString(), "no", "0" };
        if (falseValues.Contains(collection[key], StringComparer.OrdinalIgnoreCase))
            return true;
        return defaultValue;
    }

    public static string? GetString(this NameValueCollection collection, string key)
    {
        if (!collection.AllKeys.Contains(key))
            return collection[key];
        return null;
    }

    public static bool ContainsKey(this NameValueCollection collection, string key)
    {
        if (collection.Get(key) == null)
        {
            return collection.AllKeys.Contains(key);
        }
        return true;
    }
    #endregion

    #region IEnumerable<T>
    public static T PopFirst<T>(this IEnumerable<T> list) => list.ToList().PopAt(0);

    public static T PopLast<T>(this IEnumerable<T> list) => list.ToList().PopAt(list.Count() - 1);

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self) =>
        self.Select((item, index) => (item, index));
    #endregion

    #region List<T>
    public static T PopAt<T>(this List<T> list, int index)
    {
        T r = list.ElementAt<T>(index);
        list.RemoveAt(index);
        return r;
    }
    #endregion

    #region Uri
    public static readonly Regex QueryRegex = new Regex(@"[?&](\w[\w.]*)=([^?&]+)");

    public static IReadOnlyDictionary<string, string> ParseQueryString(this Uri uri)
    {
        var match = QueryRegex.Match(uri.PathAndQuery);
        var paramaters = new Dictionary<string, string>();
        while (match.Success)
        {
            paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
            match = match.NextMatch();
        }
        return paramaters;
    }





    public static Uri AddQuery(this Uri uri, string key, string value, bool encode = true)
    {
        var uriBuilder = new UriBuilder(uri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        if (encode)
        {
            query[key] = value;
            uriBuilder.Query = query.ToString();
        }
        else
        {
            var queryDict = query.AllKeys.Where(k => k != null).ToDictionary(k => k, k => query[k]);
            queryDict[key] = value;
            var queryString = string.Join(
                "&",
                queryDict.Select(kvp =>
                    $"{HttpUtility.UrlEncode(kvp.Key)}={(kvp.Key == key ? value : HttpUtility.UrlEncode(kvp.Value))}"
                )
            );
            uriBuilder.Query = queryString;
        }
        return uriBuilder.Uri;
    }
    #endregion

    #region Generic
    public static DescriptionAttribute? GetEnumDescriptionAttribute<T>(this T value)
        where T : struct
    {
        Type type = typeof(T);
        if (!type.IsEnum)
            throw new InvalidOperationException("The type parameter T must be an enum type.");
        if (!Enum.IsDefined(type, value))
            throw new InvalidEnumArgumentException("value", Convert.ToInt32(value), type);
        FieldInfo fi = type.GetField(value.ToString(), BindingFlags.Static | BindingFlags.Public);
        return fi.GetCustomAttributes(typeof(DescriptionAttribute), true)
            .Cast<DescriptionAttribute>()
            .SingleOrDefault();
    }




    #endregion

    #region Type
    public static string? GetName(this Type enumType, object value) => Enum.GetName(enumType, value);
    #endregion

    #region Task<TResult>
    public static async Task<TResult?> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        using (var timeoutCancellationTokenSource = new CancellationTokenSource())
        {
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;
            }
            else
            {
                return default(TResult);
            }
        }
    }
    #endregion

    #region bool
    public static string ToYesNo(this bool input) => input ? "Yes" : "No";

    public static string ToEnabledDisabled(this bool input) => input ? "Enabled" : "Disabled";

    public static string ToOnOff(this bool input) => input ? "On" : "Off";

    public static string ToString(
        this bool value,
        BooleanStringMode mode = BooleanStringMode.TrueFalse,
        bool capitalize = true
    )
    {
        string str;
        switch (mode)
        {
            case BooleanStringMode.Numbers:
                str = value ? "1" : "0";
                break;
            case BooleanStringMode.TrueFalse:
                str = value ? "True" : "False";
                break;
            case BooleanStringMode.EnabledDisabled:
                str = value ? "Enabled" : "Disabled";
                break;
            case BooleanStringMode.OnOff:
                str = value ? "On" : "Off";
                break;
            case BooleanStringMode.YesNo:
                str = value ? "Yes" : "No";
                break;
            default:
                throw new ArgumentNullException("mode");
        }
        return capitalize ? str : str.ToLower();
    }
    #endregion

    #region FileSystemInfo
    public static string PrintablePath(this FileSystemInfo file) => file.FullName.Replace(@"\\", @"\");
    #endregion

    #region TreeNodeCollection

    #endregion

    #region DataGridView

    #endregion

    #region int
    public static int Percentage(this int total, int part)
    {
        return (int)((double)part / total * 100);
    }
    #endregion

    #region List<string>
    public static string Join(this List<string> strings, string separator)
    {
        return string.Join(separator, strings);
    }
    #endregion

    #region Enum
    public static string? GetDescription(this Enum value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                            DescriptionAttribute? attr =
                Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }

    public static T? GetValueFromDescription<T>(string description, bool returnDefault = false)
    {
        var type = typeof(T);
        if (!type.IsEnum)
            throw new InvalidOperationException();
        foreach (var field in type.GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute != null)
            {
                if (attribute.Description == description)
                    return (T)field.GetValue(null);
            }
            else
            {
                if (field.Name == description)
                    return (T)field.GetValue(null);
            }
        }
        if (returnDefault)
            return default(T);
        else
            throw new ArgumentException("Not found.", "description");
    }
    #endregion

    #region Image

    #endregion

    #region Color

    #endregion

    #region IList<T>
    public static T PickRandom<T>(this IList<T> source)
    {
        int randIndex = rnd.Next(source.Count);
        return source[randIndex];
    }
    #endregion

    #region IEnumerable<float>
    public static string ToString(this IEnumerable<float>? floats)
    {
        return floats != null ? string.Join(", ", floats) : string.Empty;
    }

    public static string ToString(this IEnumerable<string>? strings)
    {
        return strings != null ? string.Join(", ", strings) : string.Empty;
    }


    #endregion

    #region IDictionary<string, object>
    public static string? GetValue(this IDictionary<string, object> dict, string key, string? _default = null)
    {
        dict.TryGetValue(key, out object ret);
        return ret as string ?? _default;
    }
    #endregion

    #region float
    public static float RoundAmount(this float i, float nearestFactor)
    {
        return (float)Math.Round(i / nearestFactor) * nearestFactor;
    }


    #endregion

    #region Delegate
    public static void DelegateSafeInvoke(this Delegate @delegate, params object[] args)
    {
        Delegate[] invocationList = @delegate.GetInvocationList();
        for (int i = 0; i < invocationList.Length; i++)
        {
            try
            {
                _ = invocationList[i].DynamicInvoke(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while executing delegate:\n" + ex);
            }
        }
    }
    #endregion







    #region CookieContainer
    public static IEnumerable<Cookie> GetAllCookies(this CookieContainer c)
    {
        Hashtable k = (Hashtable)
            c.GetType().GetField("m_domainTable", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(c);
        foreach (DictionaryEntry element in k)
        {
            SortedList l = (SortedList)
                element
                    .Value.GetType()
                    .GetField("m_list", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(element.Value);
            foreach (var e in l)
            {
                var cl = (CookieCollection)((DictionaryEntry)e).Value;
                foreach (Cookie fc in cl)
                {
                    yield return fc;
                }
            }
        }
    }
    #endregion
}
