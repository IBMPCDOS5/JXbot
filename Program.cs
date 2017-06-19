/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using JXbot.Modules.Public;
using System.IO;
using System.Text.RegularExpressions;
using JXbot.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using JXbot.Services;
using System.Reflection;
using System.Runtime.InteropServices;
using JXbot.Extensions;

namespace JXbot
{
    public class Program
    {
        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        public CommandService _service = new CommandService();
        private IServiceProvider _provider;
        private DiscordSocketClient client;
        public static bool doModeration = false;
        public static bool pollEnabled = false;
        public static bool isPanic = false;
        public Random rnd = new Random();
        public static int state = 0;
        public static SocketTextChannel channel = null;
        public DictionaryExtension _dict = new DictionaryExtension();
        PublicModule module = new PublicModule();
        DateTime startTime = DateTime.Now;
        public async Task Install()
        {
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            var serviceProvider = ConfigureServices();
            _provider = serviceProvider;
            client.MessageReceived += HandleCommand;
        }

        public async Task Start()
        {
            Console.WriteLine($"JXbot v{Assembly.GetExecutingAssembly().GetName().Version}");
            Configuration.EnsureExists();

            client = new DiscordSocketClient();

            client.Log += Log;

            client.UserJoined += UserJoined;
            client.UserLeft += UserLeft;
            client.MessageReceived += async (msg) =>
            {
                var message = msg as SocketUserMessage;

                var badword = File.ReadAllLines("badwords.txt");

                var context = new SocketCommandContext(client, message);

                SocketTextChannel log = null;

                int argPos = 0;

                if (context.Guild != null)
                {
                    log = context.Guild.GetTextChannel(301094488955420673);
                    if (!DictionaryExtension.serverModeration.ContainsKey(context.Guild.Id))
                        DictionaryExtension.serverModeration.Add(context.Guild.Id, false);
                }

                if (isPanic)
                {
                    var u = context.User as IGuildUser;
                    if (!u.IsBot && !u.GuildPermissions.BanMembers)
                    {
                        await context.Message.DeleteAsync();
                    }
                }

                if (context.Guild != null && DictionaryExtension.serverModeration[context.Guild.Id])
                {
                    if (Array.IndexOf(badword, context.Message.Content.ToLower()) != -1 && !context.Message.Content.Contains("penistone"))
                    {
                        await log.SendMessageAsync("<@" + context.User.Id + "> wrote " + context.Message.Content + " in " + context.Channel + ".");
                        switch (rnd.Next(0, 4))
                        {
                            case 0:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Don't be rude!");
                                break;
                            case 1:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> I can't believe you just said that..");
                                break;
                            case 2:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Let's not.");
                                break;
                            case 3:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Start being more polite!");
                                break;
                            default:
                                break;
                        }
                        await context.Message.DeleteAsync();
                        return;
                    }

                    var text = Regex.Replace(context.Message.Content, @"[^0-9a-zA-Z]+", " ");

                    //caps prevention
                    if (text == text.ToUpper() && !context.User.IsBot)
                    {
                        switch (rnd.Next(3))
                        {
                            case 0:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> There's no need to shout.");
                                break;
                            case 1:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> We like it quiet here.");
                                break;
                            case 2:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> SHHHH!");
                                break;
                            default:
                                break;
                        }
                        await context.Message.DeleteAsync();
                        return;
                    }
                }

                if (PublicModule.askedQuestion && !context.User.IsBot)
                {
                    if (PublicModule.nmb == 0)
                    {
                        if (Math.Ceiling(Math.Sqrt(PublicModule.sqrt())).ToString() == context.Message.Content)
                        {
                            await context.Channel.SendMessageAsync("<@" + context.User.Id + "> answered correctly");
                            PublicModule.askedQuestion = false;
                        }
                    }
                    else if (PublicModule.nmb == 1)
                    {
                        if (PublicModule.comb.ToString() == context.Message.Content)
                        {
                            await context.Channel.SendMessageAsync("<@" + context.User.Id + "> answered correctly");
                            PublicModule.askedQuestion = false;
                        }
                    }
                    else if (PublicModule.nmb == 2)
                    {
                        if (Math.Round(Math.Sin(PublicModule.sin), 2).ToString() == context.Message.Content)
                        {
                            await context.Channel.SendMessageAsync("<@" + context.User.Id + "> answered correctly");
                            PublicModule.askedQuestion = false;
                        }
                    }
                    else if (PublicModule.nmb == 3)
                    {
                        if (PublicModule.comb.ToString() == context.Message.Content)
                        {
                            await context.Channel.SendMessageAsync("<@" + context.User.Id + "> answered correctly");
                            PublicModule.askedQuestion = false;
                        }
                    }
                    else if (PublicModule.nmb == 4)
                    {
                        if (PublicModule.comb.ToString() == context.Message.Content)
                        {
                            await context.Channel.SendMessageAsync("<@" + context.User.Id + "> answered correctly");
                            PublicModule.askedQuestion = false;
                        }
                    }
                }

                if (DictionaryExtension.userSuggested.ContainsKey(context.User.Id))
                {
                    var currentDM = await context.User.CreateDMChannelAsync();
                    if (context.Message.Content.ToLower() == "q" && context.Guild == null)
                    {
                        await currentDM.SendMessageAsync("Process cancelled.");
                        DictionaryExtension.userSuggested.Remove(context.User.Id);
                        DictionaryExtension.currentState.Remove(context.User.Id);
                    }
                    else if (context.Guild == null)
                    {
                        switch (DictionaryExtension.currentState[context.User.Id])
                        {
                            case 1:
                                if (context.Message.Content.ToLower() == "y")
                                {
                                    DictionaryExtension.currentState[context.User.Id] = 2;
                                    var e = new EmbedBuilder()
                                    {
                                        Title = "**Title**",
                                        Color = new Color(114, 33, 161),
                                        Description = "Enter the title for your suggestion (20 characters or less)"

                                    };
                                    await currentDM.SendMessageAsync("", false, e);
                                    //   await currentDM.SendMessageAsync("**Title**\nEnter the title for your suggestion (20 characters or less)");
                                }
                                break;
                            case 2:
                                if (context.Message.Content.Length > 20)
                                {
                                    await currentDM.SendMessageAsync("Please do not exceed the 20 character limit.");
                                }
                                else if (Array.IndexOf(badword, context.Message.Content.ToLower()) != -1)
                                {
                                    await currentDM.SendMessageAsync("There's no need to be rude in a suggestion. BE POLITE.");
                                }
                                else
                                {
                                    //title = context.Message.Content;
                                    _dict.titleSug.Add(context.User.Id, context.Message.Content);
                                    if (!_dict.descSug.ContainsKey(context.User.Id))
                                    {
                                        DictionaryExtension.currentState[context.User.Id] = 3;
                                        var e = new EmbedBuilder()
                                        {
                                            Title = "**Suggestion**",
                                            Color = new Color(114, 33, 161),
                                            Description = "Write something that's relevant to the server.\nA good example is: `There should be a role for people who are gamers`\nA bad example is: `Make a role for gamers now, or else..`\nPlease write your suggestion now (500 characters or less)"

                                        };
                                        await currentDM.SendMessageAsync("", false, e);
                                    }
                                    else
                                    {
                                        DictionaryExtension.currentState[context.User.Id] = 4;
                                        EmbedBuilder eb = new EmbedBuilder();
                                        EmbedFooterBuilder efb = new EmbedFooterBuilder();
                                        EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
                                        efb.Text = "Submitted at: " + DateTime.UtcNow;
                                        eab.IconUrl = context.User.GetAvatarUrl();
                                        eab.Name = context.User.Username;
                                        eb.WithAuthor(eab);
                                        eb.Color = new Color(114, 33, 161);
                                        eb.WithFooter(efb);
                                        eb.AddField((ebf) =>
                                        {
                                            ebf.Name = _dict.titleSug[context.User.Id];
                                            ebf.Value = _dict.descSug[context.User.Id];
                                        });
                                        await currentDM.SendMessageAsync("**Confirmation**\nThis is what the staff will see:\n", false, eb);
                                        await currentDM.SendMessageAsync("\nReady to submit this suggestion?\n[y] Submit\n[r] Start over");
                                    }
                                }
                                break;
                            case 3:
                                if (context.Message.Content.Length > 500)
                                {
                                    await currentDM.SendMessageAsync("Please do not exceed the 500 character limit.");
                                }
                                else if (Array.IndexOf(badword, context.Message.Content.ToLower()) != -1)
                                {
                                    await currentDM.SendMessageAsync("There's no need to be rude in a suggestion. BE POLITE.");
                                }
                                else
                                {
                                    //description = context.Message.Content;
                                    _dict.descSug.Add(context.User.Id, context.Message.Content);
                                    DictionaryExtension.currentState[context.User.Id] = 4;
                                    EmbedBuilder eb = new EmbedBuilder();
                                    EmbedFooterBuilder efb = new EmbedFooterBuilder();
                                    EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
                                    efb.Text = "Submitted at: " + DateTime.UtcNow;
                                    eab.IconUrl = context.User.GetAvatarUrl();
                                    eab.Name = context.User.Username;
                                    eb.WithAuthor(eab);
                                    eb.Color = new Color(114, 33, 161);
                                    eb.WithFooter(efb);
                                    eb.AddField((ebf) =>
                                    {
                                        ebf.Name = _dict.titleSug[context.User.Id];
                                        ebf.Value = _dict.descSug[context.User.Id];
                                    });
                                    await currentDM.SendMessageAsync("**Confirmation**\nThis is what the staff will see:\n", false, eb);
                                    await currentDM.SendMessageAsync("\nReady to submit this suggestion?\n[y] Submit\n[r] Start over");
                                }
                                break;
                            case 4:
                                if (context.Message.Content.ToLower() == "y" && context.Guild == null)
                                {
                                    EmbedBuilder eb = new EmbedBuilder();
                                    EmbedFooterBuilder efb = new EmbedFooterBuilder();
                                    EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
                                    efb.Text = "Submitted at: " + DateTime.UtcNow;
                                    eab.IconUrl = context.User.GetAvatarUrl();
                                    eab.Name = context.User.Username;
                                    eb.WithAuthor(eab);
                                    eb.Color = new Color(114, 33, 161);
                                    eb.WithFooter(efb);
                                    eb.AddField((ebf) =>
                                    {
                                        ebf.Name = _dict.titleSug[context.User.Id];
                                        ebf.Value = _dict.descSug[context.User.Id];
                                    });
                                    await channel.SendMessageAsync("", false, eb.Build());
                                    await currentDM.SendMessageAsync("Thanks for your suggestion! c:");
                                    DictionaryExtension.userSuggested.Remove(context.User.Id);
                                    _dict.titleSug.Remove(context.User.Id);
                                    _dict.descSug.Remove(context.User.Id);
                                    DictionaryExtension.currentState.Remove(context.User.Id);
                                }
                                else if (context.Message.Content.ToLower() == "r" && context.Guild == null)
                                {
                                    DictionaryExtension.currentState[context.User.Id] = 1;
                                    _dict.titleSug.Remove(context.User.Id);
                                    _dict.descSug.Remove(context.User.Id);
                                    var e = new EmbedBuilder()
                                    {
                                        Title = $"**Make a suggestion for {context.Guild.Name}**\n",
                                        Color = new Color(114, 33, 161),
                                        Description = "Please be aware of the following:\n" +
                                        "- Your Discord Username will be recorded.\n" +
                                        "- Your suggestion will be visible for anyone to see.\n" +
                                        "- Please do NOT misuse this command. Appropriate action will be taken by staff should you ignore this.\n\n" +
                                        "Is this OK?\nRespond with `y` if you understood the above or press `q` if you don't."

                                    };
                                    await currentDM.SendMessageAsync("", false, e);
                                    /* await currentDM.SendMessageAsync("**Make a suggestion**\n" +
                                        "Please be aware of the following:\n" +
                                        "- Your Discord Username will be recorded.\n" +
                                        "- Your suggestion will be visible for anyone to see.\n" +
                                        "- Please do NOT misuse this command. Appropriate action will be taken by staff should you ignore this.\n\n" +
                                        "Is this OK?\nRespond with `y` if you understood the above or press `q` if you don't."); */
                                }
                                else
                                {
                                    await currentDM.SendMessageAsync("Respond with `y` or `q`.");
                                }
                                break;
                        }
                    }
                }

                if (pollEnabled && !context.User.IsBot)
                {
                    if (!DictionaryExtension.userVoted.ContainsKey(context.User.Id))
                    {
                        DictionaryExtension.userVoted.Add(context.User.Id, false);
                    }

                    int test = 0;

                    int[] options = new int[PublicModule.incr];

                    for (int i = 0; i < PublicModule.incr; i++)
                    {
                        options[i] = i;
                    }

                    if (int.TryParse(message.Content, out test) && options.Contains(Convert.ToInt32(message.Content) - 1) && !DictionaryExtension.userVoted[context.User.Id])
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Username} voted for {test}");
                        DictionaryExtension.votesPerOpt.Add(test, 0);
                        DictionaryExtension.votesPerOpt[test] += 1;
                        DictionaryExtension.userVoted[context.User.Id] = true;
                    }
                }

                if (!context.User.IsBot)
                {
                    if (!_dict.messageHistory.Keys.Contains(context.User.Id))
                    {
                        _dict.messageHistory.Add(context.User.Id, context.Message.Content);
                    }

                    if (!_dict.messageCount.Keys.Contains(context.User.Id))
                    {
                        _dict.messageCount.Add(context.User.Id, 0);
                    }
                    else
                    {
                        _dict.messageCount[context.User.Id] += 1;
                    }
                    if (_dict.messageHistory[context.User.Id] != context.Message.Content)
                    {
                        _dict.messageCount[context.User.Id] = 0;
                    }

                    
                    if (_dict.messageHistory[context.User.Id] == context.Message.Content && _dict.messageCount[context.User.Id] == 5)
                    {
                        var logChannel = context.Guild.GetTextChannel(301094488955420673);
                        await context.Channel.SendMessageAsync("<@" + context.User.Id + "> You're really starting to piss me off. A notification has been sent to the staff.");
                        await logChannel.SendMessageAsync($"{context.User.Username}#{context.User.Discriminator} was spamming in {context.Guild}, #{context.Channel}");
                        await context.Message.DeleteAsync();
                    }
                    else if (_dict.messageHistory[context.User.Id] == context.Message.Content && _dict.messageCount[context.User.Id] > 5)
                    {
                        await context.Message.DeleteAsync();
                    }
                    else if (_dict.messageHistory[context.User.Id] == context.Message.Content && _dict.messageCount[context.User.Id] > 3)
                    {
                        switch (rnd.Next(0, 4))
                        {
                            case 0:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Don't spam.");
                                break;
                            case 1:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> You're just annoying everyone.");
                                break;
                            case 2:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> I don't need to see your message more than once.");
                                break;
                            case 3:
                                await context.Channel.SendMessageAsync("<@" + context.User.Id + "> It'd be nicer if you could just.. STOP");
                                break;
                        }
                        await context.Message.DeleteAsync();
                    }
                    _dict.messageHistory[context.User.Id] = context.Message.Content;
                }

                if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("<3"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> WHAT?!!! I want hearts! <3");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("?"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Maybe? I don't know.");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("hello") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("hi"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Hey there!");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("jason"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Jason? What about him? He programmed me.");
                }

                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("astralmod") && message.Content.Contains("hate"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> AstralMod? I don't like him.");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("fuck you") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("fuck off"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Wanna get banned?");
                }
                else if (Configuration.Load().Blacklist.Contains(context.User.Id) && message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("yes") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("yeah") || Configuration.Load().Blacklist.Contains(context.User.Id) && message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("right?"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> I guess.");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("no") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("nope") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("nah"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> I guess not.");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("like astralmod"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> WHAT DID YOU SAY? YOU MENTIONED THE WORST BOT!");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("> astralmod")
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> Anything is better than AstralMod");
                }
            };
            client.MessageDeleted += async (e, msg) =>
            {
                var message = msg as SocketUserMessage;

                var context = new SocketCommandContext(client, message);

                if (!context.User.IsBot)
                {
                    await context.Guild.GetTextChannel(301094488955420673).SendMessageAsync($"[{context.Guild}] {context.Message.Timestamp}: {context.User} {"deleted message in <#" + context.Channel.Id + ">"}: {"```" + context.Message.Content + "```"}");
                }
            };
            client.MessageUpdated += async (e, msg, s) =>
            {
                var message = msg as SocketUserMessage;
                var test = s as SocketUserMessage;

                var context = new SocketCommandContext(client, message);

                if (!context.User.IsBot)
                {
                //    await context.Guild.GetTextChannel(301094488955420673).SendMessageAsync($"[{context.Guild}] {startTime}: {context.User} {"changed message in <#" + context.Channel.Id + ">"}: {"```" + msg - 1 + "```"} to {"```" + msg.Content + "```"}");
                }
            };

            var timer = new System.Threading.Timer(e => currentGame(),
            null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            await Install();

            await client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton<AudioService>()
                .AddSingleton(new CommandService(new CommandServiceConfig { CaseSensitiveCommands = false }));

            var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
            provider.GetService<AudioService>();

            return provider;
        }

        private async Task UserLeft(SocketGuildUser user)
        {
            var guild = client.GetGuild(301032919256793089);
            var channel = guild.GetTextChannel(301094488955420673);
            var test = user as IGuildUser;

            await channel.SendMessageAsync($":arrow_backward: <@{user.Id}>");
        }

        public async Task HandleCommand(SocketMessage parameterMessage)
        {
            var message = parameterMessage as SocketUserMessage;

            if (message == null) return;

            int argPos = 0;

            if (!(message.HasMentionPrefix(client.CurrentUser, ref argPos) || message.HasStringPrefix(Configuration.Load().Prefix, ref argPos))) return;

            var context = new SocketCommandContext(client, message);

            var result = await _service.ExecuteAsync(context, argPos);
            /* if(!message.Author.IsBot && !message.HasMentionPrefix(client.CurrentUser, ref argPos) && result.IsSuccess)
                 await message.DeleteAsync();
                */

            if (!result.IsSuccess && message.HasStringPrefix(Configuration.Load().Prefix, ref argPos))
                await message.Channel.SendMessageAsync($"**Error:** {result.ErrorReason}"); 

        }

        private async Task UserJoined(SocketGuildUser user)
        {
            var guild = client.GetGuild(301032919256793089);
            var channel = guild.GetTextChannel(301094488955420673);
            var test = user as IGuildUser;

            var author = new EmbedAuthorBuilder();
            author.IconUrl = user.GetAvatarUrl();
            author.Name = user.Username;

            var embed = new EmbedBuilder()
            {
                Author = author,
                Description = $"Discriminator: {user.Discriminator}\nCreated at: {user.CreatedAt}\nJoined at: {user.JoinedAt}"
            };
            await channel.SendMessageAsync($":arrow_forward: <@{user.Id}>", false, embed);

            if(test.CreatedAt.Day == test.JoinedAt.Value.Day && test.CreatedAt.Year == test.JoinedAt.Value.Year)
            {
                await channel.SendMessageAsync("<@&301076183921983490> This user was created today.");
            }
        }   

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task currentGame()
        {
            switch (rnd.Next(10))
            {
                case 0:
                    await client.SetGameAsync("testing bugs");
                    break;
                case 1:
                    await client.SetGameAsync("fixing stuff");
                    break;
                case 2:
                    await client.SetGameAsync("existing");
                    break;
                case 3:
                    await client.SetGameAsync("with life");
                    break;
                case 4:
                    await client.SetGameAsync("secretly hating Jason");
                    break;
                case 5:
                    await client.SetGameAsync("stalking Caljos13");
                    break;
                case 6:
                    await client.SetGameAsync("with your IP");
                    break;
                case 7:
                    await client.SetGameAsync("with tokens");
                    break;
                case 8:
                    await client.SetGameAsync(".NET or NodeJS");
                    break;
                case 9:
                    await client.SetGameAsync("attacking AstralMod");
                    break;
            }
        }
    }
}
