using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace mParticle.LoadGenerator
{
    public sealed class Program
    {
        public static void Main(string[] args)
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
            for (int i = 0 ; i < config.TargetRPS ; i++) // check if the time is less than the total time of the test in the configuration file (in minutes)
            {
                // Run all the RPS required every second.
                // Count for HTTP Status code type 5XX 4XX 2XX
                // Print every iteration the Current RPS and the Target RPS. Also the Status Code counts.
                var response = doRequest(config);
                Console.WriteLine(response);
            }
            // TODO Do some work!
        }

        
        public static async Task<JObject> doRequest(Config config)
        {
            DateTime saveUtcNow = DateTime.UtcNow;
            string content = "{\"name\":"+ config.UserName +
                             ",\"date\":" + saveUtcNow + "}";
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
                WebResponse response = await request.GetResponseAsync();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();
                    JObject adResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseContent);
                    return adResponse;
                }
            }
            catch (WebException webException)
            {
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
