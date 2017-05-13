using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using JXbot.Common;
using System.Threading;
using System.Reflection;
using System.Text;
using Discord.Audio;
using System.Net;
using Imgur;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using System.Net.Http;

namespace JXbot.Modules.Public
{
    [Name("Public")]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public static Random rnd = new Random();
        public static int firstVal, secondVal, nmb, sin, comb;
        public static bool askedQuestion = false;
        public static int sqrt()
        {
            var exclude = new HashSet<int>() { 0, 2, 3, 5, 6, 7, 8, 10, 11, 12, 13, 14, 15 };
            var range = Enumerable.Range(1, 16).Where(i => !exclude.Contains(i));

            var rand = new System.Random();
            int index = rnd.Next(0, 16 - exclude.Count);
            return range.ElementAt(index);
        }
        private const string _lmgtfyUrl = "http://lmgtfy.com/?q=";
        private const string _lmfgtfyUrl = "http://lmfgtfy.com/?q=";
        private Timer _timer;
        string timeZoneInfo(string timezone)
        {
            var toRemove = "";
            var localtime = DateTime.Now.ToUniversalTime();
            var estTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var estDateTime = TimeZoneInfo.ConvertTime(localtime, estTimeZone);
            toRemove = estDateTime.ToString().Remove(0, 9);
            return toRemove;
        }

        [Command("getavatar")]
        [Summary("Returns the avatar of a user")]
        public async Task GetAvatar(SocketUser user = null)
        {
            await ReplyAsync(user.GetAvatarUrl());
        }

        [Command("nsfw_list")]
        [Summary("Lists all your favorite NSFW pictures")]
        public async Task nsfwlist()
        {
            if (Context.Message.Channel.Id != 301109145975914499 || Context.Guild.Id != 301032919256793089)
            {
                await ReplyAsync("This command cannot be used here.");
            }
            else
            {
                string path = @"F:\Users\force\Pictures\JXGS-nsfw\";
                string[] allNSFW = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                EmbedBuilder eb = new EmbedBuilder();
                eb.Color = new Color(177, 27, 179);
                eb.Title = $"Found '{allNSFW.Length}' results";
                foreach (string s in allNSFW)
                {
                    eb.Description += s.Remove(0, 24) + "\n";
                }
                await Context.Channel.SendMessageAsync("", false, eb);
            }
        }

        [Command("nsfw_search")]
        [Summary("Searches for your favorite NSFW categories")]
        public async Task nsfwsearch([Remainder] string nsfw)
        {
            if (Context.Message.Channel.Id != 301109145975914499 || Context.Guild.Id != 301032919256793089)
            {
                await ReplyAsync("This command cannot be used here.");
            }
            else
            {
                string path = @"F:\Users\force\Pictures\JXGS-nsfw\";
                string[] allNSFW = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                if (allNSFW.Contains(path + nsfw))
                {
                    string[] picturesofNSFW = Directory.GetFiles(path + nsfw);

                    EmbedBuilder eb = new EmbedBuilder();
                    eb.Color = new Color(177, 27, 179);
                    eb.Title = $"Found '{picturesofNSFW.Length}' results matching '{nsfw}':";
                    foreach (string s in picturesofNSFW)
                    {
                        eb.Description += s.Remove(0, 24) + "\n";
                    }
                    if (picturesofNSFW.Length > 50)
                    {
                        var ebx = new EmbedBuilder()
                        {
                            Title = eb.Title,
                            Description = eb.Description.Substring(picturesofNSFW.Length / 2)
                        };
                        eb.Description.Remove(picturesofNSFW.Length / 2);
                        await Context.Channel.SendMessageAsync("", false, ebx);
                    }

                    await Context.Channel.SendMessageAsync("", false, eb);
                }
                else
                {
                    await ReplyAsync("We don't seem to have that yet... try something else!");
                }
            }
        }

        [Command("nsfw")]
        [Summary("NSFW pictures for all! Wait...")]
        public async Task nsfw([Remainder] string nsfw)
        {
            if (Context.Message.Channel.Id != 301109145975914499 || Context.Guild.Id != 301032919256793089)
            {
                await ReplyAsync("This command cannot be used here.");
            }
            else
            {
                string path = @"F:\Users\force\Pictures\JXGS-nsfw\";
                string[] allNSFW = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                string[] NSFWquote =
                {
                    "Here's some of that good stuff for ya!",
                    "Is this something you like?",
                    "Here, just for you:",
                    "I know this is a NSFW channel.. but come on!",
                    "You must be having fun..."
                };

                if (allNSFW.Contains(path + nsfw))
                {
                    string[] picturesofNSFW = Directory.GetFiles(path + nsfw);
                    int random = rnd.Next(picturesofNSFW.Count());
                    int quoterandom = rnd.Next(NSFWquote.Length);
                    string quote = NSFWquote[quoterandom];
                    string toPost = picturesofNSFW[random];
                    await Context.Channel.SendFileAsync(toPost, quote);
                }
                else
                {
                    await ReplyAsync("We don't seem to have that yet... try something else!");
                }
            }
        }

        [Command("timer")]
        [Summary("Reminds you to do something.")]
        public async Task reminder(params string[] task)
        {
            var reason = string.Join(" ", task.Skip(1));
            var inAt = task[0];
            ReminderFormat rf = new ReminderFormat();

            if (inAt != "at")
            {
                var time = rf.formattedInt(task[0]);

                switch (rf.returnedArg(task[0]))
                {
                    case 0:
                        if (reason == "") await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} seconds");
                        else await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} seconds: {reason}");
                        break;
                    case 60:
                        if (reason == "") await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} minutes");
                        else await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} minutes: {reason}");
                        break;
                    case 3600:
                        if (reason == "") await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} hours");
                        else await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} hours: {reason}");
                        break;
                }
                _timer = new Timer(async _ =>
                {
                    if (reason == "") await ReplyAsync($"<@{Context.User.Id}> **Reminder:** No reminder was given");
                    else await ReplyAsync($"<@{Context.User.Id}> **Reminder:** {reason}");
                },
                null,
                TimeSpan.FromSeconds(time * rf.returnedArg(task[0])),// Time that message should send after bot has started
                TimeSpan.Zero); //time after which message should repeat (TimeSpan.Zero for no repeat)
            }
            else if (inAt == "at")
            {
                reason = string.Join(" ", task.Skip(2));
                TimeSpan time = TimeSpan.Parse(task[1]);
                var timeNow = DateTime.UtcNow.TimeOfDay;
                var durationUntilTrigger = time - timeNow;

                if (reason == "") await ReplyAsync($"<@{Context.User.Id}> Reminding at UTC {time}");
                else await ReplyAsync($"<@{Context.User.Id}> Reminding at UTC {time}: {reason}");

                var t = new System.Threading.Timer(async o =>
                {
                    if (reason == "") await ReplyAsync($"<@{Context.User.Id}> **Reminder:** No reminder was given");
                    else await ReplyAsync($"<@{Context.User.Id}> **Reminder:** {reason}");
                }, null, durationUntilTrigger, TimeSpan.Zero);
            }
        }

        [Command("suggest")]
        [Summary("Suggest changes for the server")]
        public async Task suggest()
        {
            if (Context.Guild.Id != 301032919256793089)
            {
                await ReplyAsync("This command cannot be used here.");
            }
            else
            {
                Program.userSuggested.Add(Context.User.Id, true);
                Program.currentState.Add(Context.User.Id, 1);
                var currentDM = await Context.User.CreateDMChannelAsync();

                await ReplyAsync("Continue in DMs");
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
                /*  await currentDM.SendMessageAsync("**Make a suggestion**\n" +
                                          "Please be aware of the following:\n" +
                                          "- Your Discord Username will be recorded.\n" +
                                          "- Your suggestion will be visible for anyone to see.\n" +
                                          "- Please do NOT misuse this command. Appropriate action will be taken by staff should you ignore this.\n\n" +
                                          "Is this OK?\nRespond with `y` if you understood the above or press `q` if you don't."); */
                Program.channel = Context.Guild.GetTextChannel(312593713203380225);
            }
        }

        [Command("maths")]
        [Summary("The bot will ask a random math question")]
        public async Task maths()
        {
            firstVal = rnd.Next(3000);
            secondVal = rnd.Next(1500);
            nmb = rnd.Next(5);
            sin = rnd.Next(6);

            switch (nmb)
            {
                case 0:
                    await Context.Channel.SendMessageAsync($"What's the square root of {sqrt()}?");
                    break;
                case 1:
                    await Context.Channel.SendMessageAsync($"What's {firstVal} + {secondVal}?");
                    comb = firstVal + secondVal;
                    break;
                case 2:
                    await Context.Channel.SendMessageAsync($"What's the sin of {sin}?");
                    break;
                case 3:
                    if (secondVal > firstVal)
                    {
                        firstVal = secondVal++;
                    }
                    comb = firstVal / secondVal;
                    await Context.Channel.SendMessageAsync($"What's {firstVal} / {secondVal}?");
                    break;
                case 4:
                    comb = firstVal * secondVal;
                    await Context.Channel.SendMessageAsync($"What's {firstVal} * {secondVal}?");
                    break;
            }
            askedQuestion = true;
        }

        [Command("attack")]
        [Summary("Attacks a user")]
        public async Task attack(IGuildUser user)
        {
            await Context.Channel.SendMessageAsync("<@" + Context.User.Id + "> :right_facing_fist: <@" + user.Id + ">");
        }

        [Command("time")]
        [Summary("Displays the current time of a user")]
        public async Task time([Remainder] string tz)
        {
            if (Context.Guild.Id != 301032919256793089)
            {
                await ReplyAsync("This command cannot be used here.");
            }
            else
            {
                var invalidTimeZone = false;
                var timeNow = "";
                var temp = tz;

                switch (tz.ToLower())
                {
                    case "jason":
                    case "jelle":
                    case "aren":
                    case "amsterdam":
                        timeNow = "W. Europe Standard Time";
                        break;
                    case "neppy":
                    case "neptune":
                    case "cameron":
                    case "cammy":
                    case "pst":
                        timeNow = "Pacific Standard Time";
                        break;
                    case "vic":
                    case "victor":
                    case "victor tran":
                    case "east":
                        timeNow = "E. Australia Standard Time";
                        break;
                    case "alex":
                    case "akidfromtheuk":
                    case "meerkat":
                    case "uk":
                        timeNow = "GMT Standard Time";
                        break;
                    case "vrabby":
                    case "vrabbers":
                    case "turtle":
                        timeNow = "E. South America Standard Time";
                        break;
                    case "lolrepeat":
                    case "lolmemes":
                    case "lempamo":
                    case "nebble":
                    case "nebbly":
                    case "bird":
                        timeNow = "US Eastern Standard Time";
                        break;
                    case "friendsnone":
                        timeNow = "North Asia East Standard Time";
                        break;
                    case "ashifter":
                    case "shifty":
                    case "ashifty":
                        timeNow = "Mountain Standard Time";
                        break;
                    case "melon":
                    case "trm":
                    case "therandommelon":
                        timeNow = "Central Standard Time";
                        break;
                    default:
                        invalidTimeZone = true;
                        await Context.Channel.SendMessageAsync("Fuck off. That's not a valid timezone!");
                        break;
                }
                if (!invalidTimeZone)
                {
                    await Context.Channel.SendMessageAsync("The time now at " + tz + " is" + timeZoneInfo(timeNow));
                }
            }
        }

        [Command("embed")]
        [Summary("Send an embed. Seperate title/description with ,")]
        public async Task embed([Remainder] string args)
        {
            string[] argParsed = args.Split(',');

            var eb = new EmbedBuilder()
            {
                Title = argParsed[0],
                Description = argParsed[1],
            };
            await Context.Channel.SendMessageAsync("", false, eb);
        }

        [Command("say")]
        [Alias("echo")]
        [Summary("Echos the provided input")]
        public async Task Say([Remainder] string input)
        {
            //ulong[] uList = Configuration.Load().Blacklist.Select(ulong.Parse).ToArray();

            if (Configuration.Load().Blacklist.Contains(Context.User.Id))
                await ReplyAsync("You're on the blacklist! Nice try c:");
            else
                await ReplyAsync(input);
        }

        [Command("info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }

        [Command("lmgtfy")]
        [Summary("Use this when someone does not know how to use google")]
        public Task LmgtfyAsync([Remainder]string query)
        {
            string cleanQuery = Uri.EscapeDataString(query);
            string url = _lmgtfyUrl + cleanQuery;
            return ReplyAsync(url);
        }

        [Command("lmfgtfy")]
        [Summary("Use this when someone does not fucking know how to use google")]
        public Task LmfgtfyAsync([Remainder]string query)
        {
            string cleanQuery = Uri.EscapeDataString(query);
            string url = _lmfgtfyUrl + cleanQuery;
            return ReplyAsync(url);
        }

        private static string GetUptime()
            => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
    }
}
