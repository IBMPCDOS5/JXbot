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

namespace JXbot
{
    public class Program
    {
        public static void Main(string[] args) =>
            new Program().Start().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandHandler handler;
        public static bool doModeration = false;
        public Random rnd = new Random();
        PublicModule module = new PublicModule();
        DateTime startTime = DateTime.Now;
        public static Dictionary<ulong, bool> userSuggested = new Dictionary<ulong, bool>();
        public Dictionary<ulong, string> titleSug = new Dictionary<ulong, string>();
        public Dictionary<ulong, string> descSug = new Dictionary<ulong, string>();
        Dictionary<ulong, string> messageHistory = new Dictionary<ulong, string>();
        Dictionary<ulong, int> messageCount = new Dictionary<ulong, int>();
        public static Dictionary<ulong, int> currentState = new Dictionary<ulong, int>();
        string title = "";
        string description = "";
        public static int state = 0;
        public static SocketTextChannel channel = null;

        public async Task Start()
        {
            Configuration.EnsureExists();

            client = new DiscordSocketClient();

            client.Log += Log;

            client.MessageReceived += async (msg) =>
            {
                var message = msg as SocketUserMessage;

                var badword = File.ReadAllLines("badwords.txt");

                var context = new SocketCommandContext(client, message);

                SocketTextChannel log = null;

                int argPos = 0;

                if (context.Guild != null)
                    log = context.Guild.GetTextChannel(301094488955420673);

                if (doModeration)
                {
                    if (Array.IndexOf(badword, context.Message.Content.ToLower()) != -1)
                    {
                        await log.SendMessageAsync("<@" + context.User.Id + "> wrote " + context.Message.Content + " in " + context.Channel + ".");
                        switch(rnd.Next(0,4))
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

                if(userSuggested.ContainsKey(context.User.Id))
                {
                    var currentDM = await context.User.CreateDMChannelAsync();
                    if (context.Message.Content.ToLower() == "q" && context.Guild == null)
                    {
                        await currentDM.SendMessageAsync("Process cancelled.");
                        userSuggested.Remove(context.User.Id);
                        currentState.Remove(context.User.Id);
                    }
                    else if (context.Guild == null)
                    {
                        switch (currentState[context.User.Id])
                        {
                            case 1:
                                if (context.Message.Content.ToLower() == "y")
                                {
                                    currentState[context.User.Id] = 2;
                                    var e = new EmbedBuilder()
                                    {
                                        Title = "**Title**",
                                        Color = new Color(177, 27, 179),
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
                                    titleSug.Add(context.User.Id, context.Message.Content);
                                    if (description == "")
                                    {
                                        currentState[context.User.Id] = 3;
                                        var e = new EmbedBuilder()
                                        {
                                            Title = "**Suggestion**",
                                            Color = new Color(177, 27, 179),
                                            Description = "Write something that's relevant to the server.\nA good example is: `There should be a role for people who are gamers`\nA bad example is: `Make a role for gamers now, or else..`\nPlease write your suggestion now (500 characters or less)"

                                        };
                                        await currentDM.SendMessageAsync("", false, e);
                                    }
                                    else
                                    {
                                        currentState[context.User.Id] = 4;
                                        EmbedBuilder eb = new EmbedBuilder();
                                        EmbedFooterBuilder efb = new EmbedFooterBuilder();
                                        EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
                                        efb.Text = "Submitted at: " + DateTime.UtcNow;
                                        eab.IconUrl = context.User.GetAvatarUrl();
                                        eab.Name = context.User.Username;
                                        eb.WithAuthor(eab);
                                        eb.Color = new Color(177, 27, 179);
                                        eb.WithFooter(efb);
                                        eb.AddField((ebf) =>
                                        {
                                            ebf.Name = title;
                                            ebf.Value = description;
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
                                    descSug.Add(context.User.Id, context.Message.Content);
                                    currentState[context.User.Id] = 4;
                                    EmbedBuilder eb = new EmbedBuilder();
                                    EmbedFooterBuilder efb = new EmbedFooterBuilder();
                                    EmbedAuthorBuilder eab = new EmbedAuthorBuilder();
                                    efb.Text = "Submitted at: " + DateTime.UtcNow;
                                    eab.IconUrl = context.User.GetAvatarUrl();
                                    eab.Name = context.User.Username;
                                    eb.WithAuthor(eab);
                                    eb.Color = new Color(177, 27, 179);
                                    eb.WithFooter(efb);
                                    eb.AddField((ebf) =>
                                    {
                                        ebf.Name = titleSug[context.User.Id];
                                        ebf.Value = descSug[context.User.Id];
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
                                    eb.Color = new Color(177, 27, 179);
                                    eb.WithFooter(efb);
                                    eb.AddField((ebf) =>
                                    {
                                        ebf.Name = titleSug[context.User.Id];
                                        ebf.Value = descSug[context.User.Id];
                                    });
                                    await channel.SendMessageAsync("", false, eb.Build());
                                    await currentDM.SendMessageAsync("Thanks for your suggestion! c:");
                                    description = "";
                                    title = "";
                                    userSuggested.Remove(context.User.Id);
                                    titleSug.Remove(context.User.Id);
                                    descSug.Remove(context.User.Id);
                                    currentState.Remove(context.User.Id);
                                }
                                else if (context.Message.Content.ToLower() == "r" && context.Guild == null)
                                {
                                    currentState[context.User.Id] = 1;
                                    description = "";
                                    title = "";
                                    titleSug.Remove(context.User.Id);
                                    descSug.Remove(context.User.Id);
                                    var e = new EmbedBuilder()
                                    {
                                        Title = "**Make a suggestion**\n",
                                        Color = new Color(177, 27, 179),
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
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("yes") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("yeah") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("right?"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> I guess.");
                }
                else if (message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("no") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("nope") || message.HasMentionPrefix(client.CurrentUser, ref argPos) && message.Content.Contains("nah"))
                {
                    await context.Channel.SendMessageAsync("<@" + context.User.Id + "> I guess not.");
                }

            };
            client.MessageDeleted += async (e,msg) =>
            {
                var message = msg as SocketUserMessage;

                var context = new SocketCommandContext(client, message);

                if(!context.User.IsBot)
                {
                    await context.Guild.GetTextChannel(301094488955420673).SendMessageAsync($"[{context.Guild}] {context.Message.Timestamp}: {context.User} {"deleted message in <#" + context.Channel.Id + ">"}: {"```" + context.Message.Content + "```"}");
                }
            };
            client.MessageUpdated += async (e,msg,s) =>
            {
                var message = msg as SocketUserMessage;

                var context = new SocketCommandContext(client, message);

                if (!context.User.IsBot)
                {
                    //await context.Guild.GetTextChannel(301094488955420673).SendMessageAsync($"[{context.Guild}] {startTime}: {context.User} {"changed message in <#" + context.Channel.Id + ">"}: {"```" + message. + "```"} {"```" + e.Value + "```"}");
                }
            };

            var timer = new System.Threading.Timer(e => currentGame(),
            null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            await client.LoginAsync(TokenType.Bot, Configuration.Load().Token);
            await client.StartAsync();

            var map = new DependencyMap();
            map.Add(client);
             
            handler = new CommandHandler();
            await handler.Install(map);
            
            await Task.Delay(-1);
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
