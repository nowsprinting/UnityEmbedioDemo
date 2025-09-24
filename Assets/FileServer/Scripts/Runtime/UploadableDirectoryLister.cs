// Copyright (c) 2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Files;
using EmbedIO.Utilities;
using Swan;

namespace FileServer
{
    /// <summary>
    /// Custom directory lister that produces a HTML listing of a directory.
    /// </summary>
    /// <remarks>
    /// This class is a modified version of <see cref="EmbedIO.Files.Internal.HtmlDirectoryLister"/>.
    /// </remarks>
    public class UploadableDirectoryLister : IDirectoryLister
    {
        private static readonly Lazy<IDirectoryLister> LazyInstance =
            new Lazy<IDirectoryLister>(
                (Func<IDirectoryLister>)(() => (IDirectoryLister)new UploadableDirectoryLister()));

        private UploadableDirectoryLister()
        {
        }

        public static IDirectoryLister Instance => LazyInstance.Value;

        public string ContentType { get; } = "text/html; encoding=" + WebServer.DefaultEncoding.WebName;

        public async Task ListDirectoryAsync(
            MappedResourceInfo info,
            string absoluteUrlPath,
            IEnumerable<MappedResourceInfo> entries,
            Stream stream,
            CancellationToken cancellationToken)
        {
            if (!info.IsDirectory)
                throw SelfCheck.Failure("HtmlDirectoryLister.ListDirectoryAsync invoked with a file, not a directory.",
                    "C:\\Unosquare\\embedio\\src\\EmbedIO\\Files\\Internal\\HtmlDirectoryLister.cs", 37);
            string str = WebUtility.UrlDecode(absoluteUrlPath);
            string current = absoluteUrlPath[1..]; // not encode/decode.
            if (current.Length > 0 && current[^1] != '/')
                current += '/';
            using (StreamWriter text = new StreamWriter(stream, WebServer.DefaultEncoding))
            {
                text.Write("<html><head><title>Index of ");
                text.Write(str);
                text.Write("</title></head><body><h1>Index of ");
                text.Write(str);
                text.Write("</h1><hr/><pre>");
                if (str.Length > 1)
                    text.Write("<a href='../'>../</a>\n");
                entries = (IEnumerable<MappedResourceInfo>)entries.ToArray<MappedResourceInfo>();
                foreach (MappedResourceInfo mappedResourceInfo in (IEnumerable<MappedResourceInfo>)entries
                             .Where<MappedResourceInfo>((Func<MappedResourceInfo, bool>)(m => m.IsDirectory))
                             .OrderBy<MappedResourceInfo, string>((Func<MappedResourceInfo, string>)(e => e.Name)))
                {
                    text.Write(
                        $"<a href=\"{current}{Uri.EscapeDataString(mappedResourceInfo.Name)}\">{WebUtility.HtmlEncode(mappedResourceInfo.Name)}</a>");
                    text.Write(new string(' ', Math.Max(1, 50 - mappedResourceInfo.Name.Length + 1)));
                    text.Write(HttpDate.Format(mappedResourceInfo.LastModifiedUtc)); // TODO: to local time
                    text.Write('\n');
                    await Task.Yield();
                }

                foreach (MappedResourceInfo mappedResourceInfo in (IEnumerable<MappedResourceInfo>)entries
                             .Where<MappedResourceInfo>((Func<MappedResourceInfo, bool>)(m => m.IsFile))
                             .OrderBy<MappedResourceInfo, string>((Func<MappedResourceInfo, string>)(e => e.Name)))
                {
                    text.Write(
                        $"<a href=\"{current}{Uri.EscapeDataString(mappedResourceInfo.Name)}\">{WebUtility.HtmlEncode(mappedResourceInfo.Name)}</a>");
                    text.Write(new string(' ', Math.Max(1, 50 - mappedResourceInfo.Name.Length + 1)));
                    text.Write(HttpDate.Format(mappedResourceInfo.LastModifiedUtc)); // TODO: to local time
                    text.Write(
                        $" {mappedResourceInfo.Length.ToString("#,###", (IFormatProvider)CultureInfo.InvariantCulture),-20}\n");
                    await Task.Yield();
                }

                text.Write("</pre><hr/><pre>");
                text.Write("<form action=\"/api/upload\" method=\"POST\" enctype=\"multipart/form-data\">");
                text.Write("<input required type=\"file\" name=\"file\" /><br/>");
                text.Write("<input type=\"submit\" value=\"Upload\" /></form>");
                text.Write("</pre></body></html>");
            }
        }
    }
}
