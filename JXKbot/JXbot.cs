using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JXKbot
{
    public class JXbot
    {
        DiscordClient client;
        CommandService cmds;
        fileStream FS;
        String timeStamp = GetTimestamp(DateTime.Now);

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy/MM/dd/HH/mm/ss/ffff ");
        }

        public string serverName;

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
           .Parameter("user", Discord.Commands.ParameterType.Required)
           .Do(async (e) =>
           {
               var user = e.Args[0].ToUpper();
               var userToKick = e.Channel.Users.Where(input => input.Name.ToUpper() == user).FirstOrDefault();
               var ID = userToKick.Id;
               await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has been ***WARNED*** Reason: ");
               StreamWriter write = new StreamWriter("warnings-" + serverName + ".txt");
               write.WriteLine(timeStamp + userToKick + " was warned. Reason: ");
               write.Close();
           });

            cmds.CreateCommand("ping").Do(async (e) =>
            {
                await e.Channel.SendMessage("pong!");
            });

            cmds.CreateCommand("status").Do(async (e) =>
            {
                await e.Channel.SendMessage(":fireemblem: Bot is functioning properly!");
            });

            cmds.CreateCommand("kill").AddCheck((cm, u, ch) => u.ServerPermissions.ManageRoles)
           .Do(async (e) =>
           {
               await e.Channel.SendMessage("Bot was killed");
               await Task.Delay(500);
               Environment.Exit(0);
            });

            cmds.CreateCommand("announce").Parameter("channel", ParameterType.Multiple).Do(async (e) =>
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
            
            if(e.Message.Contains("bot:")){
                Console.WriteLine("works!");
            }
            Console.WriteLine(e.Message);
        }
    }
}
