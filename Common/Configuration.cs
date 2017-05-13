using Newtonsoft.Json;
using System;
using System.IO;

namespace JXbot.Common
{
    /// <summary> 
    /// A file that contains information you either don't want public
    /// or will want to change without having to compile another bot.
    /// </summary>
    public class Configuration
    {
        [JsonIgnore]
        /// <summary> The location and name of your bot's configuration file. </summary>
        public static string FileName { get; private set; } = "configuration.json";
        /// <summary> Ids of users who will have owner access to the bot. </summary>
        public ulong[] Owners { get; set; }
        /// <summary> Your bot's command prefix. </summary>
        public string Prefix { get; set; } = "!";
        /// <summary> Your bot's login token. </summary>
        public string Token { get; set; } = "";
        /// <summary> Your blacklist </summary>
        public ulong[] Blacklist { get; set; }
        /// <summary> Your list of users in the TimeModule </summary>
        public string[] TimeModuleUsers { get; set; }

        public static void EnsureExists()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            if (!File.Exists(file))                                
            {
                string path = Path.GetDirectoryName(file);      
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var config = new Configuration();                 

                Console.WriteLine("Please enter your token: ");
                string token = Console.ReadLine();                 

                config.Token = token;
                config.SaveJson();                                  
            }
            Console.WriteLine("Configuration Loaded");
        }

        /// <summary> Save the configuration to the path specified in FileName. </summary>
        public void SaveJson()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            File.WriteAllText(file, ToJson());
        }

        /// <summary> Load the configuration from the path specified in FileName. </summary>
        public static Configuration Load()
        {
            string file = Path.Combine(AppContext.BaseDirectory, FileName);
            return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(file));
        }

        /// <summary> Convert the configuration to a JSON string. </summary>
        public string ToJson()
            => JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
