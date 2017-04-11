﻿using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JXKbot
{
    public class JXbot
    {
        DiscordClient client;
        CommandService cmds;
        String timeStamp = DateTime.Now.ToString();

        public string serverName = "";

        public void FS(string userToKick, string reason) {
            while (timeStamp != DateTime.Now.ToString())
            {
                timeStamp = DateTime.Now.ToString();
            }
            StreamWriter write = new StreamWriter("warnings-" + serverName + ".txt", true);
            write.WriteLine(timeStamp + " " + userToKick + " was warned. Reason: " + reason);
            write.Close();
        }

        public JXbot() {

            client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

            client.UsingCommands(input =>
            {
                input.PrefixChar = '?';
                input.AllowMentionPrefix = true;
            }
            );

            cmds = client.GetService<CommandService>();

            serverName = client.SessionId;

            cmds.CreateCommand("reboot").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
            .Do(async(e) =>
            {
                var reboot = Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start(reboot);
                await e.Channel.SendMessage("Bot is restarting..");
                await Task.Delay(500);
                Environment.Exit(0);
            });

            cmds.CreateCommand("help")
            .Do(async (e) =>
            {
                StreamReader write = new StreamReader("commands.txt");
                String line = write.ReadToEnd();
                await e.Channel.SendMessage(line);
                write.Close();
            });

            cmds.CreateCommand("kick").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
           .Parameter("user", Discord.Commands.ParameterType.Required)
           .Do(async (e) =>
           {
               var user = e.Args[0].ToUpper();
               var userToKick = e.Channel.Users.Where(input => input.Name.ToUpper() == user).FirstOrDefault();
               var ID = userToKick.Id;
               await userToKick.Kick();
               await e.Channel.SendMessage("<@" + ID + "> has been ***KICKED***!");
           });

            cmds.CreateCommand("ban").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
           .Parameter("user", Discord.Commands.ParameterType.Required)
           .Do(async (e) =>
           {
               var user = e.Args[0].ToUpper();
               var userToKick = e.Channel.Users.Where(input => input.Name.ToUpper() == user).FirstOrDefault();
               var ID = userToKick.Id;
               await userToKick.Server.Ban(userToKick);
               await e.Channel.SendMessage("<@" + ID + "> has been ***BANNED***!");
           });

            cmds.CreateCommand("warn").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
           .Parameter("user", ParameterType.Multiple)
           .Do(async (e) =>
           {
               var user = e.Args[0].ToUpper();
               var reason = "";
               for (int i = 1; i < e.Args.Length; i++)
               {
                   reason += e.Args[i].ToString() + " ";
               }
               var userToKick = e.Channel.Users.Where(input => input.Name.ToUpper() == user).FirstOrDefault();
               var ID = userToKick.Id;
               await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has been ***WARNED*** Reason: " + reason);
               await userToKick.SendMessage("You have been warned in JXKGS. Reason: " + reason);
;              FS(userToKick.ToString(), reason);
           });

            cmds.CreateCommand("clearwarn").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
           .Parameter("user", ParameterType.Multiple)
           .Do(async (e) =>
           {
               var user = e.Args[0].ToUpper();
               var userToKick = e.Channel.Users.Where(input => input.Name.ToUpper() == user).FirstOrDefault();
               var ID = userToKick.Id;
               var oldLines = System.IO.File.ReadAllLines("warnings-" + serverName + ".txt");
               var newLines = oldLines.Where(line => !line.Contains(userToKick.ToString()));
               File.WriteAllLines("warnings-" + serverName + ".txt", newLines);
               await e.Channel.SendMessage("Warnings for <@" + ID + "> have been deleted.");
           });
            cmds.CreateCommand("listwarn").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
            .Parameter("user", ParameterType.Multiple)
            .Do(async (e) =>
            {
                var user = e.Args[0].ToUpper();
                var userToKick = e.Channel.Users.Where(input => input.Name.ToUpper() == user).FirstOrDefault();
                var ID = userToKick.Id;
                var result = File.ReadAllLines("warnings-" + serverName + ".txt").Where(c=>c.Contains(userToKick.ToString())).FirstOrDefault();
                if(result == null)
                {
                    await e.Channel.SendMessage("<:zelda:301087844741414922> This user has no warnings.");
                } else {
                    await e.Channel.SendMessage("```" + result + "```");
                }
            });

            cmds.CreateCommand("mute").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
            .Parameter("user", Discord.Commands.ParameterType.Required)
            .Do(async (e) =>
            {
                var user = e.Args[0].ToUpper();
                var userToKick = e.Channel.Users.Where(input => input.Name.ToUpper() == user).FirstOrDefault();
                var ID = userToKick.Id;
                Role[] roles = (Role[])e.Server.Roles;
                await e.User.AddRoles(roles[0]);
                await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has been ***MUTED*** Reason: ");
                await userToKick.SendMessage("You have been muted in JXKGS. Reason: ");
                StreamWriter write = new StreamWriter("warnings-" + serverName + ".txt", true);
                write.WriteLine(timeStamp + userToKick + " was muted. Reason: ");
                write.Close();
            });

            cmds.CreateCommand("ping").Do(async (e) =>
            {
                await e.Channel.SendMessage("pong!");
            });

            cmds.CreateCommand("status").Do(async (e) =>
            {
                await e.Channel.SendMessage("<:fireemblem:301087475508707328> Bot is " + e.User.Status);
            });

            cmds.CreateCommand("kill").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
           .Do(async (e) =>
           {
               await e.Channel.SendMessage("Bot was killed");
               await Task.Delay(500);
               Environment.Exit(0);
            });

            cmds.CreateCommand("announce").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
            .Parameter("channel", ParameterType.Multiple).Do(async (e) =>
            {
                await DoAnnouncement(e);
            });

            RegisterPurgeCommand();

            client.ExecuteAndWait(async () =>
            {
                await client.Connect("", TokenType.Bot);
            });
        }

        private void RegisterPurgeCommand()
        {
            cmds.CreateCommand("purge")
           .Alias(new string[] { "clear", "remove" })
           .AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
           .Parameter("amount", Discord.Commands.ParameterType.Optional)
           .Do(async (e) =>
           {
               int amountToDelete = 100;
               int.TryParse(e.GetArg("amount"), out amountToDelete);
               Message[] messagesToDelete;
               messagesToDelete = await e.Channel.DownloadMessages(amountToDelete);

               await e.Channel.DeleteMessages(messagesToDelete);
               await e.Channel.SendMessage($"Deleted {amountToDelete} messages :)");
           });
        }

        private async Task DoAnnouncement(CommandEventArgs e)
        {
            var channel = e.Server.FindChannels(e.Args[0], ChannelType.Text).FirstOrDefault();

            var message = ConstructMessage(e, channel != null);

            if(channel != null){
                await channel.SendMessage(message);
            } else {
                await e.Channel.SendMessage("Failed! Try again.");
            }
        }

        private string ConstructMessage(CommandEventArgs e, bool firstArg)
        {
            string message = "";

            var name = e.User.Nickname != null ? e.User.Nickname : e.User.Name;

            var indx = firstArg ? 1 : 0;

            for (int i = indx; i < e.Args.Length; i++){
                message += e.Args[i].ToString() + " ";
            }
            return message;
        }
        private void Log(object sender, LogMessageEventArgs e){
            
            while(timeStamp != DateTime.Now.ToString()){
                timeStamp = DateTime.Now.ToString();
            }
            Console.WriteLine(timeStamp + " " + e.Message);
        }
    }
}
