using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuraWeb
{
    public static class Common
    {
        public static string SQLiteSDEPath = AppContext.BaseDirectory + Settings._SQLITE_SDE_DB_NAME;
        public static string SQLiteSDETempExtractionPath = AppContext.BaseDirectory + "temp/";

        public static async Task DownloadAsync(Uri requestUri, string filename)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (
                Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await contentStream.CopyToAsync(stream);
            }
        }
    }

    #region Downloading
    public class Downloader
    {
        private string _URL { get; set; }
        private string _Filename { get; set; }
        private string _UserAgent { get; set; }
        private bool _Verbose { get; set; }

        public Downloader(string url, string filename, bool verbose, string userAgent = "Polite EVE dev attempting to test his application!")
        {
            _URL = url;
            _Filename = filename;
            _UserAgent = userAgent;
            _Verbose = verbose;
        }

        public void DownloadFile()
        {
            if (_Verbose) // Don't show progress bar
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", _UserAgent);
                    wc.DownloadProgressChanged += HandleDownloadProgress;
                    wc.DownloadFileCompleted += HandleDownloadComplete;

                    var syncObject = new Object();
                    lock (syncObject)
                    {

                        wc.DownloadFileAsync(new Uri(_URL), _Filename, syncObject);
                        //This would block the thread until download completes
                        Monitor.Wait(syncObject);
                    }
                }
            }
            else 
            {
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", _UserAgent);
                    //wc.DownloadProgressChanged += HandleDownloadProgress;
                    wc.DownloadFileCompleted += HandleDownloadComplete;

                    var syncObject = new Object();
                    lock (syncObject)
                    {
                        wc.DownloadFileAsync(new Uri(_URL), _Filename, syncObject);
                        //This would block the thread until download completes
                        Monitor.Wait(syncObject);
                    }
                }
            }
        }

        public void HandleDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLine("\r  Download Complete!");
            lock (e.UserState)
            {
                //releases blocked thread
                Monitor.Pulse(e.UserState);
            }
        }

        public void HandleDownloadProgress(object sender, DownloadProgressChangedEventArgs args)
        {
            //Console.Write(String.Format("\r  Downloading file...{0}%", args.ProgressPercentage));
            double dlPerc = (double)args.ProgressPercentage / 100;
        }
    }
    #endregion

    #region Miscellaneous
    // https://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object
    public class ObjectDumperCustom
    {
        private int _level;
        private readonly int _indentSize;
        private readonly StringBuilder _stringBuilder;
        private readonly List<int> _hashListOfFoundElements;

        private ObjectDumperCustom(int indentSize)
        {
            _indentSize = indentSize;
            _stringBuilder = new StringBuilder();
            _hashListOfFoundElements = new List<int>();
        }

        public static string Dump(object element)
        {
            return Dump(element, 2);
        }

        public static string Dump(object element, int indentSize)
        {
            var instance = new ObjectDumperCustom(indentSize);
            return instance.DumpElement(element);
        }

        private string DumpElement(object element)
        {
            if (element == null || element is ValueType || element is string)
            {
                Write(FormatValue(element));
            }
            else
            {
                var objectType = element.GetType();
                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    Write("{{{0}}}", objectType.FullName);
                    _hashListOfFoundElements.Add(element.GetHashCode());
                    _level++;
                }

                var enumerableElement = element as IEnumerable;
                if (enumerableElement != null)
                {
                    foreach (object item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            _level++;
                            DumpElement(item);
                            _level--;
                        }
                        else
                        {
                            if (!AlreadyTouched(item))
                                DumpElement(item);
                            else
                                Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
                        }
                    }
                }
                else
                {
                    MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var memberInfo in members)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        var propertyInfo = memberInfo as PropertyInfo;

                        if (fieldInfo == null && propertyInfo == null)
                            continue;

                        var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                        object value = fieldInfo != null
                                           ? fieldInfo.GetValue(element)
                                           : propertyInfo.GetValue(element, null);

                        if (type.IsValueType || type == typeof(string))
                        {
                            Write("{0}: {1}", memberInfo.Name, FormatValue(value));
                        }
                        else
                        {
                            var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                            Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                            var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                            _level++;
                            if (!alreadyTouched)
                                DumpElement(value);
                            else
                                Write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
                            _level--;
                        }
                    }
                }

                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    _level--;
                }
            }

            return _stringBuilder.ToString();
        }

        private bool AlreadyTouched(object value)
        {
            if (value == null)
                return false;

            var hash = value.GetHashCode();
            for (var i = 0; i < _hashListOfFoundElements.Count; i++)
            {
                if (_hashListOfFoundElements[i] == hash)
                    return true;
            }
            return false;
        }

        private void Write(string value, params object[] args)
        {
            var space = new string(' ', _level * _indentSize);

            if (args != null)
                value = string.Format(value, args);

            _stringBuilder.AppendLine(space + value);
        }

        private string FormatValue(object o)
        {
            if (o == null)
                return ("null");

            if (o is DateTime)
                return (((DateTime)o).ToShortDateString());

            if (o is string)
                return string.Format("\"{0}\"", o);

            if (o is char && (char)o == '\0')
                return string.Empty;

            if (o is ValueType)
                return (o.ToString());

            if (o is IEnumerable)
                return ("...");

            return ("{ }");
        }
    }

    public static class RestClientExtensions
    {
        public static async Task<RestResponse> ExecuteAsync(this RestClient client, RestRequest request)
        {
            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();
            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));
            return (RestResponse)(await taskCompletion.Task);
        }
    }
    #endregion
}
