/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
using System.Globalization;
using System.Text.RegularExpressions;
using JXbot.Extensions;
using JXbot.Common.Preconditions;
using JXbot.Services;
using Discord.Audio;

namespace JXbot.Modules.Public
{
    [Name("Public")]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public DictionaryExtension _dict = new DictionaryExtension();
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
        private static Timer _timer;
        string timeZoneInfo(string timezone)
        {
            var toRemove = "";
            var localtime = DateTime.Now.ToUniversalTime();
            var estTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var estDateTime = TimeZoneInfo.ConvertTime(localtime, estTimeZone);
            toRemove = estDateTime.ToString().Remove(0, 10);
            return toRemove;
        }
        public static int incr = 0;
        static int minToWait = 0;
        bool nickExpletive(string s)
        {
            foreach (string x in File.ReadAllLines("badwords.txt"))
            {
                if (s.Contains(x.ToLower())) return true;
            }
            return false;
        }
        static bool timerPossible = true;
        static bool pollPossible = true;
        static IGuildUser creatorOfPoll = null;

        [Command("guilds")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task guilds()
        {
            string guilds = "";
            foreach (SocketGuild guild in Context.Client.Guilds)
            {
                guilds += guild.Name + "\n" + "Created by: **" + guild.Owner.Username + "**\n" + "\n";
            }

            var embed = new EmbedBuilder()
            {
                Title = "Number of guilds: " + Context.Client.Guilds.Count,
                Description = guilds
            };

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("declnick")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task declinenick(ulong UID)
        {
            // Program.nickTimer[UID].Change(Timeout.Infinite, Timeout.Infinite);
            DictionaryExtension.nickTimer[UID].Dispose();
            DictionaryExtension.nickTimer.Remove(UID);
            await Context.Channel.SendMessageAsync($"Nickname for {UID} has been declined.");
        }

        [Command("nick")]
        public async Task nick([Remainder] string nickname)
        {
            if (!DictionaryExtension.nickTimeout.ContainsKey(Context.User.Id))
            {
                if (nickExpletive(nickname.ToLower()))
                {
                    await Context.Channel.SendMessageAsync("No... not happening.");
                }
                else if (nickname.Length >= 30)
                {
                    await Context.Channel.SendMessageAsync("That nickname would clutter up the entire screen!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Alright, your nickname will be changed in 5 minutes if it hasn't been declined.");
                    await Context.Guild.GetTextChannel(301094488955420673).SendMessageAsync($"{Context.User.Username}#{Context.User.Discriminator} wants to change their nickname to {nickname}. Type jx:declnick {Context.User.Id} to decline.");
                    DictionaryExtension.nickTimer[Context.User.Id] = new Timer(async _ =>
                    {
                        var user = Context.User as IGuildUser;

                        await user.ModifyAsync(u =>
                        {
                            u.Nickname = nickname;
                        });
                        DictionaryExtension.nickTimer.Remove(Context.User.Id);
                    },
                    null,
                    TimeSpan.FromMinutes(5),// Time that message should send after bot has started
                    TimeSpan.Zero); //time after which message should repeat (TimeSpan.Zero for no repeat)
                }
                DictionaryExtension.nickTimeout[Context.User.Id] = new Timer(async x =>
                {
                    DictionaryExtension.nickTimeout.Remove(Context.User.Id);
                    Console.WriteLine($"{Context.User.Id} has to wait 1 hour until their next nickname change.");
                },
                null, TimeSpan.FromHours(1), TimeSpan.Zero);

            }
            else
            {
                await Context.Channel.SendMessageAsync("Can you be patient?! Wait 1 hour!");
            }
        }

        [Command("eval")]
        public async Task neweval([Remainder] string equation)
        {
            var evaluation = EvalExtension.eval(equation);

            if (evaluation != -1)
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Success",
                    Description = $"The answer is {evaluation}, happy now?",
                    Color = new Color(30, 197, 43)
                };
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var embed = new EmbedBuilder()
                {
                    Title = "Nice try",
                    Description = "You failed pretty bad. Try again you retard.",
                    Color = new Color(216, 33, 33)
                };
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("stoppoll")]
        [Summary("Stops a poll")]
        public async Task stoppoll()
        {
            var user = Context.User as IGuildUser;

            if (Context.User == creatorOfPoll || user.GuildPermissions.BanMembers)
            {
                if (Program.pollEnabled)
                {
                    _timer.Dispose();
                    pollPossible = true;
                    timerPossible = true;
                    Program.pollEnabled = false;
                    DictionaryExtension.votesPerOpt.Clear();
                    DictionaryExtension.userVoted.Clear();
                    if (DictionaryExtension.votesPerOpt.Keys != null)
                    {
                        await Context.Channel.SendMessageAsync($"Poll stopped!\nThe results are in! {DictionaryExtension.votesPerOpt.Keys.Max()} got the most votes!");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("Poll stopped!");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync("There's no active poll to stop.");
                }

            }
        }

        [Command("poll")]
        [Summary("Creates a poll")]
        public async Task poll([Remainder]string input)
        {
            string options = "";
            string[] argParsed = Regex.Split(input, ", ");
            incr = 0;
            int timer = 0;

            if (int.TryParse(argParsed.First(), out timer)) ;
            else
            {
                timer = 5;
            }

            if (argParsed.Length < 2)
            {
                await Context.Channel.SendMessageAsync("Well.. kinda difficult to make a poll without at least 2 options!");
                pollPossible = false;
                timerPossible = false;
            }

            if (argParsed.Length > 11)
            {
                await Context.Channel.SendMessageAsync("You don't really need that many options.");
                pollPossible = false;
                timerPossible = false;
            }

            if (timer > 20)
            {
                await Context.Channel.SendMessageAsync("It'd be nice if you didn't exceed the 20 minute limit...");
                timerPossible = false;
                pollPossible = false;
            }
            else if (timer <= 0)
            {
                await Context.Channel.SendMessageAsync("Do you really think the timer can be that low?");
                timerPossible = false;
                pollPossible = false;
            }

            if (!Program.pollEnabled && pollPossible)
            {
                minToWait = timer;
                creatorOfPoll = (IGuildUser)Context.User;

                var embed = new EmbedBuilder();

                if (int.TryParse(argParsed.First(), out int x))
                {
                    embed.Title = $"Poll: {argParsed[1]}";
                    argParsed = argParsed.Skip(2).ToArray();
                }
                else
                {
                    embed.Title = $"Poll: {argParsed[0]}";
                    argParsed = argParsed.Skip(1).ToArray();
                }

                foreach (string line in argParsed)
                {
                    incr++;
                    options += $"{incr}. {line}\n";
                }
                embed.Description = options;
                embed.Color = new Color(114, 33, 161);

                await Context.Channel.SendMessageAsync("", false, embed);
                Program.pollEnabled = true;

                if (timerPossible)
                {
                    _timer = new Timer(async _ =>
                    {
                        if (DictionaryExtension.votesPerOpt.Keys != null)
                        {
                            await Context.Channel.SendMessageAsync($"The results are in! {DictionaryExtension.votesPerOpt.Values.Max()} got the most votes!");
                        }
                        Program.pollEnabled = false;
                        DictionaryExtension.votesPerOpt.Clear();
                        DictionaryExtension.userVoted.Clear();
                        creatorOfPoll = null;
                        _timer.Dispose();
                    },
                    null,
                    TimeSpan.FromMinutes(timer),// Time that message should send after bot has started
                    TimeSpan.Zero); //time after which message should repeat (TimeSpan.Zero for no repeat) 
                }
            }
            else if (Program.pollEnabled)
            {
                await Context.Channel.SendMessageAsync($"Hold on there! Wait {minToWait} minute(s) for your turn.");
            }
        }

        [Command("getavatar")]
        [Summary("Returns the avatar of a user")]
        public async Task GetAvatar(SocketUser user = null)
        {
            var parsedURI = user.GetAvatarUrl().Replace("size=128", "size=1024");
            await ReplyAsync(parsedURI);
        }

        [Command("nsfw_list")]
        [RequireNsfw]
        [Summary("Lists all your favorite NSFW pictures")]
        public async Task nsfwlist()
        {
            string path = @"F:\Users\force\Pictures\JXGS-nsfw\";
            string[] allNSFW = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            EmbedBuilder eb = new EmbedBuilder();
            eb.Color = new Color(114, 33, 161);
            eb.Title = $"Found '{allNSFW.Length}' results";
            foreach (string s in allNSFW)
            {
                eb.Description += s.Remove(0, 24) + "\n";
            }
            await Context.Channel.SendMessageAsync("", false, eb);
        }

        [Command("nsfw_search")]
        [RequireNsfw]
        [Summary("Searches for your favorite NSFW categories")]
        public async Task nsfwsearch([Remainder] string nsfw)
        {
            string path = @"F:\Users\force\Pictures\JXGS-nsfw\";
            string[] allNSFW = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            if (allNSFW.Contains(path + nsfw))
            {
                string[] picturesofNSFW = Directory.GetFiles(path + nsfw);

                EmbedBuilder eb = new EmbedBuilder();
                eb.Color = new Color(114, 33, 161);
                eb.Title = $"Found '{picturesofNSFW.Length}' results matching '{nsfw}':";
                foreach (string s in picturesofNSFW)
                {
                    eb.Description += s.Remove(0, 46) + "\n";
                }
                int toRemove = 0;
                while (eb.Description.Length > 1750)
                {
                    var ebx = new EmbedBuilder()
                    {
                        Title = eb.Title,
                        Description = eb.Description.Remove(1750)
                    };
                    eb.Description = eb.Description.Remove(toRemove, 1750);
                    toRemove = toRemove + 1750;
                    await Context.Channel.SendMessageAsync("", false, ebx);
                }

                await Context.Channel.SendMessageAsync("", false, eb);
            }
            else
            {
                await ReplyAsync("We don't seem to have that yet... try something else!");
            }

        }

        [Command("nsfw")]
        [RequireNsfw]
        [Summary("NSFW pictures for all! Wait...")]
        public async Task nsfw([Remainder] string nsfw)
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

        [Command("timer")]
        [Summary("Reminds you to do something.")]
        public async Task reminder(params string[] task)
        {
            var reason = string.Join(" ", task.Skip(1));
            var inAt = task[0];
            ReminderFormat rf = new ReminderFormat();
            bool timerDisabled = false;

            if (inAt != "at")
            {
                var time = rf.formattedInt(task[0]);

                switch (rf.returnedArg(task[0]))
                {
                    case 1:
                        if (time > 86400) { await ReplyAsync($"<@{Context.User.Id}> Seriously? Your timer doesn't have to be more than a day.."); timerDisabled = true; }
                        else if (reason == "") await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} seconds.");
                        else await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} seconds: {reason}");
                        break;
                    case 60:
                        if (time > 86400) { await ReplyAsync($"<@{Context.User.Id}> Seriously? Your timer doesn't have to be more than a day.."); timerDisabled = true; }
                        else if (reason == "") await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} minutes.");
                        else await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} minutes: {reason}");
                        break;
                    case 3600:
                        if (time > 86400) { await ReplyAsync($"<@{Context.User.Id}> Seriously? Your timer doesn't have to be more than a day.."); timerDisabled = true; }
                        else if (reason == "") await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} hours.");
                        else await ReplyAsync($"<@{Context.User.Id}> Reminding in {time} hours: {reason}");
                        break;
                }
                if (!timerDisabled)
                {
                    _timer = new Timer(async _ =>
                    {
                        if (reason == "") await ReplyAsync($"<@{Context.User.Id}> **Reminder:** No reminder was given.");
                        else await ReplyAsync($"<@{Context.User.Id}> **Reminder:** {reason}");
                    },
                    null,
                    TimeSpan.FromSeconds(time * rf.returnedArg(task[0])),// Time that message should send after bot has started
                    TimeSpan.Zero); //time after which message should repeat (TimeSpan.Zero for no repeat)
                }
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
            if (!Context.Guild.Channels.Any(c => c.Name == "suggestions"))
            {
                await ReplyAsync("This command cannot be used here.");
            }
            else
            {
                DictionaryExtension.userSuggested.Add(Context.User.Id, true);
                DictionaryExtension.currentState.Add(Context.User.Id, 1);
                var currentDM = await Context.User.CreateDMChannelAsync();

                await Context.Message.DeleteAsync();
                await ReplyAsync($"<@{Context.User.Id}> Continue in DMs");
                var e = new EmbedBuilder()
                {
                    Title = $"**Make a suggestion for {Context.Guild.Name}**\n",
                    Color = new Color(114, 33, 161),
                    Description = "Please be aware of the following:\n" +
                                            "- Your Discord Username will be recorded.\n" +
                                            "- Your suggestion will be visible for anyone to see.\n" +
                                            "- Please do NOT misuse this command. Appropriate action will be taken by staff should you ignore this.\n\n" +
                                            "Is this OK?\nRespond with `y` if you understood the above or press `q` if you don't."

                };
                await currentDM.SendMessageAsync("", false, e);
                var suggestionChannel = Context.Guild.Channels.Where(c => c.Name == "suggestions").FirstOrDefault();
                Program.channel = Context.Guild.GetTextChannel(suggestionChannel.Id);
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
                double num = 0;

                if (double.TryParse(tz, out num))
                {
                    DateTime time;
                    if (num != 0)
                    {
                        time = DateTime.UtcNow.AddHours(num);
                    }
                    else
                    {
                        time = DateTime.UtcNow;
                    }
                    await ReplyAsync($"The time now at UTC {tz} is {time}");
                }
                else
                {
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
                        case "invoxiplaygames":
                        case "uk":
                        case "ipg":
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
                        await Context.Channel.SendMessageAsync($"The time now at {tz} is {timeZoneInfo(timeNow)}");
                    }
                }
            }
        }

        [Command("embed")]
        [Summary("Send an embed. Seperate title/description with ,")]
        public async Task embed([Remainder] string args)
        {
            string[] argParsed = Regex.Split(args, ", ");

            var eb = new EmbedBuilder()
            {
                Title = argParsed[0],
                Description = argParsed[1],
            };

            if (argParsed.Length == 3)
            {
                string colorcode = argParsed[2];
                uint argb = uint.Parse(colorcode.Replace("#", ""), NumberStyles.HexNumber);
                Color clr = new Color(argb);
                eb.Color = clr;
            }
            await Context.Channel.SendMessageAsync("", false, eb);
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

        [Command("ping")]
        [Summary("Self-explanatory")]
        public async Task ping()
        {
            switch (rnd.Next(5))
            {
                case 0:
                    await ReplyAsync("Pong! I'm more active than that Astralfucker!");
                    break;
                case 1:
                    await ReplyAsync("Pong! I'm here!");
                    break;
                case 2:
                    await ReplyAsync("Pong... what do you need this time?");
                    break;
                case 3:
                    await ReplyAsync("Pong. I'm tired.");
                    break;
            }
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
