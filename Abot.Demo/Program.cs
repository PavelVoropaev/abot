
using Abot.Crawler;
using Abot.Poco;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abot.Demo
{
    class Program
    {

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            List<Uri> urlsToCrawl = GetSiteToCrawl(args);
            Task[] listTasks = new Task[urlsToCrawl.Count];
            for (int index = 0; index < urlsToCrawl.Count; index++)
            {
                var uri = urlsToCrawl[index];
                var crawler = GetManuallyConfiguredWebCrawler();
                crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

                listTasks[index] = Task.Factory.StartNew(x => crawler.Crawl((Uri)x), uri);
            }

            Task.WaitAll(listTasks);
        }

        private static IWebCrawler GetManuallyConfiguredWebCrawler()
        {
            CrawlConfiguration config = new CrawlConfiguration
            {
                CrawlTimeoutSeconds = 0,
                DownloadableContentTypes = "text/html, text/plain",
                IsExternalPageCrawlingEnabled = false,
                IsExternalPageLinksCrawlingEnabled = false,
                IsRespectRobotsDotTextEnabled = true,
                IsUriRecrawlingEnabled = false,
                MaxConcurrentThreads = 10,
                MaxPagesToCrawl = 1000,
                MaxPagesToCrawlPerDomain = 0,
                MinCrawlDelayPerDomainMilliSeconds = 1000
            };

            return new PoliteWebCrawler(config, null, null, null, null, null, null, null, null);
        }

        private static List<Uri> GetSiteToCrawl(string[] args)
        {
            var resultUrls = new List<Uri>();
            do
            {

                string userInput;
                if (args.Length < 1)
                {
                    Console.WriteLine("Please enter ABSOLUTE url to crawl:");
                    userInput = Console.ReadLine();
                }
                else
                {
                    userInput = args[0];
                }

                if (string.IsNullOrWhiteSpace(userInput))
                    throw new ApplicationException("Site url to crawl is as a required parameter");

                try
                {
                    resultUrls.Add(new Uri(userInput));
                }
                catch (UriFormatException)
                {
                    Console.WriteLine("Please enter ABSOLUTE url to crawl. Try again:");
                }

                Console.WriteLine("Add more link? Y - yes");
            } while (Console.ReadKey().Key == ConsoleKey.Y);


            return resultUrls;
        }

        private static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            PageDownloader.DownloadPage(e.CrawledPage);
        }
    }
}
