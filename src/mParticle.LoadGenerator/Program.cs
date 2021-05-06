using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace mParticle.LoadGenerator
{
    public sealed class Program
    {
        //private static SemaphoreSlim semaphore;
        //private static HttpClient HttpClient = new HttpClient();
        
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
             
            Stopwatch totalTime = new Stopwatch();
            Request request = new Request(config);
            Tester tester = new Tester(request, (int)config.ExecTime);
            totalTime.Start();
            //var results = await tester.GetResultsSynchrnously((int)config.TargetRPS);
            //var results = await tester.GetResultsInParallel((int)config.TargetRPS);
            var results = await tester.GetResultsInParallelMaxThreads((int)config.TargetRPS, (int)config.MaxThreads);
            totalTime.Stop();
            Console.WriteLine($"Total Time: {totalTime.ElapsedMilliseconds.ToString()} " );
            Console.WriteLine($"Total Request sent: {results.Count().ToString()} " );
            PrintReport(results, totalTime.ElapsedMilliseconds);
        }
        public static void PrintReport (IEnumerable<HttpRequestResult> results, long totalTime)
        {
            int a = results.Count( x => x.statusCode == "OK");
            int b = results.Count( x => x.statusCode == "InternalServerError");
            int c = results.Count( x => x.statusCode == "TooManyRequests");
            int d = (results.Count())-a-b-c;
            var avg = results.Select( x => x.time).ToList().Average();
            var max = results.Select( x => x.time).ToList().Max();
            Console.WriteLine("");
            Console.WriteLine("--------------------------------REPORT--------------------------------");
            Console.WriteLine("OK: " + a + " | InternalServerError: " + b + " | TooManyRequests " + c + " | Others: " + d );
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine( $"Average Response Time: {avg.ToString()} ms. Maximum Response Time: {max.ToString()} ms.");
            Console.WriteLine( $"Observed RPS : {(results.Count()/(totalTime/1000)).ToString()} Request/Second. ");

        }

        private static float Average( params int[] values)
        {
            int sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i];
            }
            return (float)sum/values.Length;
        }
    }
    public class Content
    {
        public string name { get; set; }
        public string date { get; set; }
    }

    public class HttpRequestResult
    {
        public string statusCode { get; set; }
        public int time { get; set; }
    }

    public class Request
    {
        private HttpClient client;
        private Content content;
        private Config config;
 
        public Request(Config _config)
        {
            config = _config;
            client = new HttpClient();
            SetHttpRequest();
            content = new Content
                {
                    name = config.UserName
                };
        }
 
        public async Task<String> doRequest()
        {
            updateContent();
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync( "Live/", content);
                return  response.StatusCode.ToString();
            }
            catch(Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Console.WriteLine("Timed out");
                return  "Error";
            }
        }

        public async Task<HttpRequestResult> DoRequestReturningInfo()
        {
            updateContent();
            Stopwatch current = new Stopwatch();
            current.Start();
            string responseText = string.Empty;
            
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync( "Live/", content);
                responseText = response.StatusCode.ToString();
            }
            catch(Exception ex) when (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                Console.WriteLine("Timed out");
                responseText = "Error";
            }
            current.Stop();
            var result = new HttpRequestResult()
            {
                statusCode = responseText,
                time = (int)current.ElapsedMilliseconds
            };
            return result;
        }
        private void updateContent()
        {
            CultureInfo enUS = new CultureInfo("en-US");
            string saveUtcNow = DateTime.UtcNow.ToString("g", enUS);
            content.date = saveUtcNow;
        }

        private void SetHttpRequest()
            {
                client.BaseAddress = new Uri(config.ServerURL);
                client.DefaultRequestHeaders.Add("X-Api-Key", config.AuthKey); 
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            }
    }
    
    public class Tester
    {
        Request request;
        private int execTime;
        public Tester(Request _request, int _execTime)
        {
            request = _request;
            execTime = _execTime;
        }
        public async Task<IEnumerable<HttpRequestResult>> GetResultsSynchrnously(int RPS)
        {
            var results = new List<HttpRequestResult>();
            Stopwatch runningTime = new Stopwatch();
            runningTime.Start();
            for(int i=0 ; i<RPS ; i++)
            {
                Stopwatch current = new Stopwatch();
                current.Start();
                var response = await request.doRequest();
                current.Stop();
                var result = new HttpRequestResult()
                {
                    statusCode = response,
                    time = (int)current.ElapsedMilliseconds
                };
                current.Reset();
                results.Add(result);
                var totalTime = runningTime.ElapsedMilliseconds/1000;
                if (totalTime > execTime)
                {
                    i=RPS;
                }
            }
            return results;
        }

        public async Task<IEnumerable<HttpRequestResult>> GetResultsInParallel(int RPS)
        {
            int[] requestList = new int[RPS];
            var tasks = requestList.Select(x => request.DoRequestReturningInfo());
            var results= await Task.WhenAll(tasks);
    
            return results;
        }

        public async Task<IEnumerable<HttpRequestResult>> GetResultsInParallelMaxThreads(int RPS, int MaxThreads)
        {
            var semaphore = new SemaphoreSlim(MaxThreads);
            var tasks = new List<Task>();
            var results= new List<HttpRequestResult>();
            int[] requestList = new int[RPS];
            foreach (var element in requestList)
            {
                await semaphore.WaitAsync();
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result =  await request.DoRequestReturningInfo();
                        results.Add(result);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            await Task.WhenAll(tasks);
            return results;
        }

    }
    
}


