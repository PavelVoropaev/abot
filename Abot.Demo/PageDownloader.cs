using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Abot.Poco;
using log4net;

namespace Abot.Demo
{
    internal static class PageDownloader
    {
        private static readonly ILog Logger = LogManager.GetLogger("AbotLogger");

        public static void DownloadPage(CrawledPage crawledPage)
        {
            var tmpFolder = Path.Combine(@"download sites", crawledPage.ParentUri.Host);
            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }

            var pageUri = crawledPage.Uri;
            var text = new StringBuilder(crawledPage.Content.Text);
            var links = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//link[@href]")?.Select(x => x?.Attributes["href"]?.Value);
            var imgs = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//img[@src]")?.Select(x => x?.Attributes["src"]?.Value);
            var scripts = crawledPage.HtmlDocument.DocumentNode.SelectNodes("//script[@src]")?.Select(x => x?.Attributes["src"]?.Value);

            SaveContent(links, pageUri, tmpFolder, text);
            SaveContent(imgs, pageUri, tmpFolder, text);
            SaveContent(scripts, pageUri, tmpFolder, text);

            var localPath = pageUri.LocalPath.Trim('/');
            if (string.IsNullOrWhiteSpace(localPath))
            {
                localPath = "index";
            }

            var pathToHtml = Path.Combine(tmpFolder, localPath + ".html");
            var tmpDirectory = new FileInfo(pathToHtml).DirectoryName;
            if (!Directory.Exists(tmpDirectory))
            {
                Directory.CreateDirectory(tmpDirectory);
            }

            File.WriteAllText(pathToHtml, text.Replace(pageUri.Host, "/" + pageUri.LocalPath).ToString());
        }

        private static void SaveContent(IEnumerable<string> links, Uri pageUri, string tmpFolder, StringBuilder text)
        {
            if (links == null)
            {
                return;
            }

            foreach (var link in links.Where(link => !string.IsNullOrWhiteSpace(link) && link != "#"))
            {
                try
                {
                    var href = link.StartsWith("http") ? new Uri(link) : new Uri(Path.Combine(pageUri.AbsoluteUri, link));

                    if (href.IsAbsoluteUri && href.Host != pageUri.Host)
                    {
                        continue;
                    }

                    using (var downloader = new WebClient())
                    {
                        if (!href.IsAbsoluteUri)
                        {
                            href = new Uri(pageUri.AbsoluteUri + href.LocalPath);
                        }

                        var result = downloader.DownloadData(href);
                        var contentPageName = href.LocalPath.Trim('/');

                        var fullPathToTempFile = Path.Combine(tmpFolder, contentPageName);

                        var tempDirectory = new FileInfo(fullPathToTempFile).DirectoryName;
                        if (!Directory.Exists(tempDirectory))
                        {
                            Directory.CreateDirectory(tempDirectory);
                        }

                        File.WriteAllBytes(fullPathToTempFile, result);

                        if (!string.IsNullOrWhiteSpace(pageUri.LocalPath) && pageUri.LocalPath != "/")
                        {
                            text.Replace(link, Path.Combine(pageUri.LocalPath, link).TrimStart('/'));
                        }
                    }
                }
                catch (UriFormatException ex)
                {
                    Logger.Info($"Skip invalidUrl. Url {link}", ex);
                }
            }
        }
    }
}
