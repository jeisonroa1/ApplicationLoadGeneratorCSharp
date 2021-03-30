using System;
using System.IO;

namespace mParticle.LoadGenerator
{
    /// <summary>
    /// This is a basic configuration file handler. Please feel free to add values to the config as needed.
    /// </summary>
    public class Config
    {
        public string ServerURL { get; set; }
        public uint TargetRPS { get; set; }
        public string AuthKey { get; set; }
        public string UserName { get; set; }

        /// <summary>
        /// Read the arguments file and, if successful, return the specified arguments values.
        /// </summary>
        /// <param name="argumentsFilePath">Path to the file containing JSON specifying the argument values.</param>
        /// <returns></returns>
        public static Config GetArguments(string argumentsFilePath)
        {
            try
            {
                using (FileStream argumentsFileStream = new FileStream(argumentsFilePath, FileMode.Open))
                {
                    using (StreamReader argumentsReader = new StreamReader(argumentsFileStream))
                    {
                        return ParseArguments(argumentsReader.ReadToEnd());
                    }
                }
            }
            catch (Exception argumentsException)
            {
                Console.WriteLine($"Input arguments could not be processed: {argumentsException.Message}");
                return null;
            }
        }
        
        internal static Config ParseArguments(string argumentsText)
        {
            bool success = true;
            Config arguments;

            try
            {
                arguments = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(argumentsText);
            }
            catch (Exception jsonException)
            {
                Console.WriteLine($"Input arguments could not be interpreted as JSON: {jsonException.Message}");
                return null;
            }

            ValidateArgument(arguments.ServerURL, "serverURL", ref success);
            ValidateArgument(arguments.TargetRPS, "targetRPS", ref success);
            ValidateArgument(arguments.AuthKey, "authKey", ref success);
            ValidateArgument(arguments.UserName, "userName", ref success);

            return success ? arguments : null;
        }

        private static void ValidateArgument(string argument, string argumentName, ref bool success)
        {
            if (argument == null || argument == "")
            {
                Console.WriteLine($"Must specify a nonempty value for {argumentName}.");
                success = false;
            }
        }

        private static void ValidateArgument(uint argument, string argumentName, ref bool success)
        {
            if (argument == 0)
            {
                Console.WriteLine($"Must specify a nonzero value for {argumentName}.");
                success = false;
            }
        }
    }


}
