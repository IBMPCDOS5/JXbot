/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JXbot.Common;
using JXbot.Common.Preconditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Reflection;
using JXbot.Extensions;

namespace JXbot.Modules.Mod
{

    [Name("Moderator")]
    public class ModModule : ModuleBase<SocketCommandContext>
    {
        Random rnd = new Random();
        static IGuildUser userToJail = null;
        static bool jailConfirmed = false;
        string timeStamp = DateTime.Now.ToString();
        async void FS(string user, string by, string reason)
        {
            while (timeStamp != DateTime.Now.ToString())
            {
                timeStamp = DateTime.Now.ToString();
            }
            using (FileStream fs = new FileStream($"warnings-{Context.Guild.Name}.txt", FileMode.Append, FileAccess.Write))
            using (StreamWriter fw = new StreamWriter(fs))
            {
                await fw.WriteLineAsync($"{timeStamp} {user} was warned by {by}. Reason: {reason}");
            }
        }


        [Command("panic")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task panicMode()
        {
            if (Program.isPanic)
            {
                Program.isPanic = false;
                await Context.Channel.SendMessageAsync("**Info:** Panic mode has been disabled.");
            }
            else
            {
                Program.isPanic = true;
                await Context.Channel.SendMessageAsync("**Info:** Panic mode has been enabled.");
            }
        }

        [Command("clear warn")]
        [Summary("Clears warnings for a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task clearwarnings(IGuildUser user)
        {
            var oldLines = System.IO.File.ReadAllLines($"warnings-{Context.Guild.Name}.txt");
            var newLines = oldLines.Where(line => !line.Contains(user.Username));

            File.WriteAllLines($"warnings-{Context.Guild.Name}.txt", newLines);
            await Context.Channel.SendMessageAsync($"Warnings for {user.Username} have been deleted.");

        }

        [Command("list warn")]
        [Summary("Lists warnings for a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task listwarnings(IGuildUser user)
        {
            string line;
            string results = "";
            using (FileStream fs = new FileStream($"warnings-{Context.Guild.Name}.txt", FileMode.Open, FileAccess.Read))
            using (StreamReader file = new StreamReader(fs))
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(user.Username))
                    {
                        results += line + "\n";
                    }
                }
                if (results == "")
                    await ReplyAsync("<:zelda:301087844741414922> This user has no warnings.");
                else
                {
                    var e = new EmbedBuilder()
                    {
                        Title = $"Warnings for {user.Username}",
                        Description = results,
                        Color = new Color(114, 33, 161)
                    };
                    await Context.Channel.SendMessageAsync("", false, e);
                }
            }
        }

        [Command("warn")]
        [Summary("Warns a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task warn(IGuildUser user, [Remainder] string reason = "")
        {
            var actionBy = Context.User;
            var DM = await user.CreateDMChannelAsync();

            await ReplyAsync($"<:fireemblem:301087475508707328> {user.Username} has been **warned** by {actionBy.Username}. Reason: {reason}");
            await DM.SendMessageAsync($"You have been warned in {Context.Guild.Name}\nReason: {reason}");
            FS(user.Username, actionBy.Username, reason);
        }

        [Command("purge")]
        [Alias("clear", "remove")]
        [Summary("Deletes messages")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messages);

            var m = await ReplyAsync($"Deleted {amount} message(s) c:");
            await Task.Delay(5000);
            await m.DeleteAsync();
        }

        [Command("cancel")]
        [Summary("Cancels an action")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task cancel()
        {
            if (userToJail != null)
            {
                await Context.Channel.SendMessageAsync("<:fireemblem:301087475508707328> OK, " + userToJail.Username + " won't be jailed.");
                userToJail = null;
            }
            else
            {
                await Context.Channel.SendMessageAsync("<:fireemblem:301087475508707328> There is nothing to cancel.");
            }
        }

        [Command("uinfo")]
        [Summary("Displays user information")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task uinfo(IGuildUser user)
        {
            var embedAuthor = new EmbedAuthorBuilder()
            {
                IconUrl = user.GetAvatarUrl(),
                Name = user.Username + "#" + user.Discriminator
            };
            var embedFooter = new EmbedFooterBuilder()
            {
                Text = $"Requested by: {Context.User.Username} at {DateTime.UtcNow}"
            };
            var embed = new EmbedBuilder()
            {
                Author = embedAuthor,
                Footer = embedFooter,
                Description = "\u200B",
                Color = new Color(114, 33, 161)
            };
            embed.AddField((e1) =>
            {
                e1.Name = "User information";
                e1.Value = $"**Username:** {user.Username}\n**Nickname:** {user.Nickname}\n**Currently playing:** {user.Game}\n";

            });
            embed.AddField((e2) =>
            {
                e2.Name = "Time information";
                e2.Value = $"**Created at:** {user.CreatedAt}\n**Joined at:** {user.JoinedAt}";
            });

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("mute")]
        [Summary("Mutes a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task mute(IGuildUser user)
        {
            if (!user.RoleIds.Any(u => u == 301058219676008448))
            {
                var muteRole = Context.Guild.GetRole(301058219676008448);
                await user.AddRoleAsync(muteRole);
                await Context.Channel.SendMessageAsync($"<:fireemblem:301087475508707328> {user.Username} has been **muted** by {Context.User.Username}.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"<:fireemblem:301087475508707328> {user.Username} is already muted.");
            }
        }

        [Command("jail")]
        [Summary("Jails a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task jail(IGuildUser user = null)
        {
            if (jailConfirmed)
            {
                var jailRole = Context.Guild.GetRole(302584782024605700);
                var DM = await userToJail.CreateDMChannelAsync();
                await userToJail.AddRoleAsync(jailRole);
                await Context.Channel.SendMessageAsync($"<:fireemblem:301087475508707328> {userToJail.Username} has been **jailed** by {Context.User.Username}.");
                await DM.SendMessageAsync("You have been jailed in " + Context.Guild.Name);

                jailConfirmed = false;
                return;
            }

            if (user == null && userToJail == null)
            {
                await Context.Channel.SendMessageAsync("<:fireemblem:301087475508707328> There is nobody to jail.");
                return;
            }
            if (user.GuildPermissions.BanMembers && user != null)
            {
                await Context.Channel.SendMessageAsync("Why would you be able to jail another staff member?");
                return;
            }

            userToJail = user;
            await Context.Channel.SendMessageAsync("<:fireemblem:301087475508707328> Are you sure you want to jail " + userToJail.Username + "? Type jx:jail again to confirm or jx:cancel to cancel.");

            jailConfirmed = true;
        }

        [Command("reboot")]
        [Summary("Reboots the bot")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task reboot()
        {
            switch (rnd.Next(4))
            {
                case 0:
                    await Context.Channel.SendMessageAsync("I'll be back before you know it..");
                    break;
                case 1:
                    await Context.Channel.SendMessageAsync("You haven't seen the last of me!");
                    break;
                case 2:
                    await Context.Channel.SendMessageAsync("AstralMod... we'll meet again..");
                    break;
                case 3:
                    await Context.Channel.SendMessageAsync("I have feelings too!");
                    break;
            }

            var reboot = Assembly.GetExecutingAssembly().Location;
            System.Diagnostics.Process.Start(reboot);
            Environment.Exit(0);
        }

        [Command("mod on")]
        [Summary("Turns the bots moderation on")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task modon()
        {
            // if(!serverModeration.ContainsKey(Context.Guild.Id))
            //    serverModeration.Add(Context.Guild.Id, true);

            if (DictionaryExtension.serverModeration[Context.Guild.Id])
            {
                await Context.Channel.SendMessageAsync("Moderation is already turned on.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Moderation is now turned on.");
                DictionaryExtension.serverModeration[Context.Guild.Id] = true;
            }
        }

        [Command("mod off")]
        [Summary("Turns the bots moderation off")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task modoff()
        {
            if (!DictionaryExtension.serverModeration[Context.Guild.Id])
            {
                await Context.Channel.SendMessageAsync("Moderation is already turned off.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Moderation is now turned off.");
                DictionaryExtension.serverModeration[Context.Guild.Id] = false;
            }
        }
    }
}
