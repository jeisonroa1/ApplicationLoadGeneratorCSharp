using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;

namespace mParticle.LoadGenerator
{
    public sealed class Program
    {
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

            //int a = 0;
            //int b = 0;
            //int c = 0;

            for (int i = 0 ; i < config.TargetRPS ; i++) // check if the time is less than the total time of the test in the configuration file (in minutes)
            {
                //bool Completed = ExecuteWithTimeLimit(TimeSpan.FromMilliseconds(1000), async() =>
                //{
                //   var response = await doRequest(config);
                //});
                // Run all the RPS required every second.
                // Count for HTTP Status code type 5XX 4XX 2XX
                // Print every iteration the Current RPS and the Target RPS. Also the Status Code counts.
                var response = await doRequest(config);
                foreach (var value in response.Properties())
                {
                     //Console.WriteLine(value);
                }

               
            }
            // TODO Do some work!
        }

        public static bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock)
        {
            try
            {
                Task task = Task.Factory.StartNew(() => codeBlock());
                task.Wait(timeSpan);

                return task.IsCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }   
        }


        

        
        public static async Task<JObject> doRequest(Config config)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime UtcNow = DateTime.UtcNow;
            string saveUtcNow = DateTime.UtcNow.ToString("g", enUS);

            string content = "{\"name\": \""+ config.UserName +
                             "\" ,\"date\": \"" + saveUtcNow + "\"}";

            Console.WriteLine(content);
            byte[] data = Encoding.UTF8.GetBytes(content);
            WebRequest request = WebRequest.Create(config.ServerURL);
            request.Method = "POST"; 
            request.ContentType = "application/json";
            request.Headers.Add("X-Api-Key",config.AuthKey);
            request.ContentLength = data.Length;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                HttpWebResponse response =  (HttpWebResponse) await request.GetResponseAsync();
                Console.WriteLine(response.StatusCode);
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();
                    JObject adResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseContent);
                    return adResponse;
                }
            }
            catch (WebException webException)
            {
                HttpWebResponse wRespStatusCode = ((HttpWebResponse) webException.Response);                
                Console.WriteLine(wRespStatusCode.StatusCode); ///////////////////////////////

                if (webException.Response != null)
                {
                    using (StreamReader reader = new StreamReader(webException.Response.GetResponseStream()))
                    {
                        string responseContent = reader.ReadToEnd();
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseContent); ;
                    }
                }
            }

   return null;
}
    }
}
