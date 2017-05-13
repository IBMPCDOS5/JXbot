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

namespace JXbot.Modules.Mod
{

    [Name("Moderator")]
    public class ModModule : ModuleBase<SocketCommandContext>
    {
        static SocketGuildUser userToJail;
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

        [Command("clearwarns")]
        [Summary("Clears warnings for a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task clearwarnings(IGuildUser user)
        {
            var oldLines = System.IO.File.ReadAllLines($"warnings-{Context.Guild.Name}.txt");
            var newLines = oldLines.Where(line => !line.Contains(user.Username));

            File.WriteAllLines($"warnings-{Context.Guild.Name}.txt", newLines);
            await Context.Channel.SendMessageAsync($"Warnings for {user.Username} have been deleted.");

        }

        [Command("listwarns")]
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
                        Color = new Color(177, 27, 179)
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
                Color = new Color(177, 27, 179)
            };
            embed.AddField((e1) =>
            {
                e1.Name = "User information";
                e1.Value = $"Username: {user.Username}\nNickname: {user.Nickname}\nCurrently playing: {user.Game}";

            });
            embed.AddField((e2) =>
            {
                e2.Name = "Time information";
                e2.Value = $"Created at: {user.CreatedAt}\nJoined at: {user.JoinedAt}";
            });

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("confjail")]
        [Summary("Confirms a jail")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task confjail()
        {
            if (userToJail != null)
            {
                var jailRole = Context.Guild.GetRole(302584782024605700);
                var DM = await userToJail.CreateDMChannelAsync();
                await userToJail.RemoveRolesAsync(userToJail.Roles);
                await userToJail.AddRoleAsync(jailRole);
                await Context.Channel.SendMessageAsync($"<:fireemblem:301087475508707328> {userToJail.Username} has been **jailed** by {Context.User.Username}.");
                await DM.SendMessageAsync("You have been jailed in " + Context.Guild.Name);
                userToJail = null;
            }
            else
            {
                await Context.Channel.SendMessageAsync("<:fireemblem:301087475508707328> There is nobody to jail.");
            }
        }

        [Command("jail")]
        [Summary("Jails a user")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task jail(SocketGuildUser user)
        {
            userToJail = user;
            if (user.GuildPermissions.BanMembers)
            {
                await Context.Channel.SendMessageAsync("Why would you be able to jail another staff member?");
                userToJail = null;
            }
            else
            {
                await Context.Channel.SendMessageAsync("<:fireemblem:301087475508707328> Are you sure you want to jail " + userToJail.Username + "? Type jx:confjail to confirm or cancel to jx:cancel.");
            }

        }

        [Command("mod on")]
        [Summary("Turns the bots moderation on")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task modon()
        {
            if (Program.doModeration)
            {
                await Context.Channel.SendMessageAsync("Moderation is already turned on.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Moderation is now turned on.");
                Program.doModeration = true;
            }
        }

        [Command("mod off")]
        [Summary("Turns the bots moderation off")]
        [MinPermissions(AccessLevel.ServerMod)]
        public async Task modoff()
        {
            if (!Program.doModeration)
            {
                await Context.Channel.SendMessageAsync("Moderation is already turned off.");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Moderation is now turned off.");
                Program.doModeration = false;
            }
        }
    }
}
