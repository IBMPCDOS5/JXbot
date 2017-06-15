using Discord;
using Discord.Commands;
using JXbot.Common;
using JXbot.Common.Preconditions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;  
using System.Text;
using System.Threading.Tasks;

namespace JXbot.Modules.Public
{
    public class QuoteModule : ModuleBase<SocketCommandContext>
    {
        private string quotes = "quotes.json";
        private Random rnd = new Random();

        [Command("random")]
        public async Task randomQuote(IGuildUser user = null)
        {
            string file = Path.Combine(AppContext.BaseDirectory, quotes);
            var obj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(file));

            if(user == null)
            {
                var x = obj;

                int randomUser = rnd.Next(x.Count);
                var pickedUser = x[randomUser];
                int randomQuote = rnd.Next(pickedUser["Quotes"].Values().Count());
                var quotesCurrent = (JArray)pickedUser["Quotes"];

                var embedAuthor = new EmbedAuthorBuilder()
                {
                    IconUrl = (string)pickedUser["IconURL"],
                    Name = user.Username
                };

                var embedFooter = new EmbedFooterBuilder()
                {
                    Text = (string)pickedUser["Footer"]
                };

                var embed = new EmbedBuilder()
                {
                    Author = embedAuthor,
                    Footer = embedFooter,
                    Description = quotesCurrent[randomQuote].ToString(),
                };

                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var test = obj[user.Username];
                var quotesCurrent = (JArray)test["Quotes"];
                int randomQuote = rnd.Next(test["Quotes"].Values().Count());

                if ((string)test["IconURL"] == "URL")
                {
                    test["IconURL"] = user.GetAvatarUrl();
                    File.WriteAllText(file, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }

                if ((string)test["IconURL"] != user.GetAvatarUrl())
                {
                    test["IconURL"] = user.GetAvatarUrl();
                    File.WriteAllText(file, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }

                if ((string)test["Footer"] == "CreatedDate")
                {
                    test["Footer"] = DateTime.UtcNow.ToString();
                    File.WriteAllText(file, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }

                var embedAuthor = new EmbedAuthorBuilder()
                {
                    IconUrl = (string)test["IconURL"],
                    Name = user.Username
                };

                var embedFooter = new EmbedFooterBuilder()
                {
                    Text = (string)test["Footer"]
                };

                var embed = new EmbedBuilder()
                {
                    Author = embedAuthor,
                    Footer = embedFooter,
                    Description = quotesCurrent[randomQuote].ToString(),
                };

                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("addquote")]
        public async Task addQuote(IGuildUser user, [Remainder] string quote)
        {
            string file = Path.Combine(AppContext.BaseDirectory, quotes);
            var obj = (JObject)JsonConvert.DeserializeObject(File.ReadAllText(file));

            if (obj[user.Username] != null)
            {
                var appendQuote = (JArray)obj[user.Username]["Quotes"];
                appendQuote.Add(quote);
                File.WriteAllText(file, JsonConvert.SerializeObject(obj, Formatting.Indented));

                await Context.Channel.SendMessageAsync($"INFO: Added {quote} by {user.Username} to the JSON Object");
            }
            else
            {
                var userToAdd = @"{
                                ""IconURL"": ""URL"",
                                ""Footer"": ""CreatedDate"",
                                ""Quotes"": [ """ + quote + @""" ]" +
                                @"}";

                obj.Add(user.Username, JToken.Parse(userToAdd));
                File.WriteAllText(file, JsonConvert.SerializeObject(obj, Formatting.Indented));

                await Context.Channel.SendMessageAsync($"INFO: Added {user.Username} with quote {quote} to the JSON Object");
            }
        }
    }
}
