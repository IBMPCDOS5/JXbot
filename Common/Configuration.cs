/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
        /// <summary> The location and name of JXbot's configuration file. </summary>
        public static string FileName { get; private set; } = "configuration.json";
        /// <summary> Ids of users who will have owner access to JXbot. </summary>
        public ulong[] Owners { get; set; }
        /// <summary> JXbots command prefix. </summary>
        public string Prefix { get; set; } = "!";
        /// <summary> JXbot's login token. </summary>
        public string Token { get; set; } = "";
        /// <summary> JXbot's blacklist </summary>
        public ulong[] Blacklist { get; set; }
        /// <summary> JXbot's list of users in the TimeModule </summary>
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
