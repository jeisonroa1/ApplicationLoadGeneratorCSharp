using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace mParticle.LoadGenerator
{
    public class Content
    {
        public string name { get; set; }
        public string date { get; set; }
    }
    public sealed class Program
    {
        private static SemaphoreSlim semaphore;
        private static HttpClient HttpClient = new HttpClient();
        
        public static async Task Main(string[] args)
        {
            string configFile = "config.json";

            if (args.Length > 0)
            {
                configFile = args[0];
            }

            Config config = Config.GetArguments(configFile);
            if (config == null)
            {
                Console.WriteLine("Failed to parse configuration.");
                return;
            }
            
            SetHttpRequest(config);
            int a = 0,b = 0,c = 0,d = 0;
            int currentRPS = 0;
            Stopwatch timeMeasure = new Stopwatch();
            Stopwatch totalTime = new Stopwatch();
            totalTime.Start();
            timeMeasure.Start();
            for ( int i = 0; i<config.TargetRPS ; i++) 
            {
                if (totalTime.ElapsedMilliseconds > config.ExecTime*60000)
                {
                    i = (int) config.TargetRPS;
                    Console.WriteLine("Total test time exceded.");
                }
                
                if (timeMeasure.ElapsedMilliseconds > 1000)
                {
                    Console.WriteLine(" |||| Current RPS: " + currentRPS + " Desired RPS: " + config.TargetRPS + " |||||| OK: " + a + " | Forbidden: " + b + " | TooManyRequests " + c + " | Others: "+ d);
                    timeMeasure.Reset();
                    timeMeasure.Start();
                    currentRPS = 0 ;
                }
                else
                {
                    await Task.Run(async () =>
                    {
                        var response = await doRequest(config);
                        currentRPS++;
                        if( response == "OK"){a++;}
                        if( response == "Forbidden"){b++;                        }
                        if( response == "TooManyRequests"){c++;}
                        if( response != "OK" && response != "Forbidden" && response != "TooManyRequests"){d++;}
                        //Console.Clear();
                        //Console.WriteLine(response + " |||| Current RPS: " + currentRPS + " Desired RPS: " + config.TargetRPS );
                    });

                }
            }
            
                Console.WriteLine("OK: " + a + " | Forbidden: " + b + " | TooManyRequests " + c + " | Others: "+ d);
        }
        private static async Task<String> doRequest(Config config)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime UtcNow = DateTime.UtcNow;
            string saveUtcNow = DateTime.UtcNow.ToString("g", enUS);

            Content content = new Content
                {
                    name = config.UserName,
                    date = saveUtcNow,
                };
            try
            {
                await semaphore.WaitAsync();
                HttpResponseMessage response = await HttpClient.PostAsJsonAsync( "Live/", content);
                return  response.StatusCode.ToString();
            }
            catch(Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Console.WriteLine("Timed out");
                return  "Error";
            }
            finally
            {
                semaphore.Release();
            } 

        }
        private static void SetMaxConcurrency(string url, int maxConcurrentRequests)
            {
                ServicePointManager.FindServicePoint(new Uri(url)).ConnectionLimit = maxConcurrentRequests;
            }
        private static void SetHttpRequest(Config config)
            {
                HttpClient.BaseAddress = new Uri(config.ServerURL);
                HttpClient.DefaultRequestHeaders.Add("X-Api-Key", config.AuthKey); 
                HttpClient.DefaultRequestHeaders.Accept.Clear();
                HttpClient.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                semaphore = new SemaphoreSlim((int) config.MaxThreads);
                SetMaxConcurrency(config.ServerURL, (int) config.MaxThreads);
            }
    }
}
