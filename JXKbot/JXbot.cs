using Discord;
using Discord.Commands;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Audio;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;using Discord.Modules;
using System.Threading;
using System.Xml;

namespace JXKbot
{
    public class JXbot
    {
        DiscordClient client;
        CommandService cmds;
        String timeStamp = DateTime.Now.ToString();
        Random rnd = new Random();
        XmlDocument config = new XmlDocument();
        public string serverName = "";
        public bool askedQuestion = false;

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

            int firstVal = 0;
            int secondVal = 0;
            int sqRoot = 0;
            int nmb = 0;
            int comb = 0;
            int sin = 0;

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

            client.MessageDeleted += async (s, e) =>
            {
                if (!e.Message.IsAuthor) {
                    await e.Server.GetChannel(301094488955420673).SendMessage($"```[{e.Server}] {timeStamp}: {e.User} {"deleted"}: {e.Message.RawText} {"```"}");
                }
            };
            client.MessageUpdated += async (s, e) =>
            {
                if (e.Before.Text != e.After.Text)
                {
                    await e.Server.GetChannel(301094488955420673).SendMessage($"```[{e.Server}] {timeStamp}: {e.User} {"changed message from"} {e.Before.Text} {"to"} {e.After.Text} {"```"}");
                }
            };
            client.MessageReceived += async (s, e) =>
            { 
                if(askedQuestion){
                    if (nmb == 0)
                    {
                        if(Math.Round(Math.Sqrt(sqRoot),2).ToString() == e.Message.Text){
                            await e.Channel.SendMessage("<@" + e.User.Id + "> answered correctly");
                            askedQuestion = false;
                        }

                    } else if (nmb == 1){
                        if(comb.ToString() == e.Message.Text){
                            await e.Channel.SendMessage("<@" + e.User.Id + "> answered correctly");
                            askedQuestion = false;
                        }
                    } else if (nmb == 2){
                        if (Math.Round(Math.Sin(sin), 2).ToString() == e.Message.Text)
                        {
                            await e.Channel.SendMessage("<@" + e.User.Id + "> answered correctly");
                            askedQuestion = false;
                        }
                    }
                }
            };

            client.Ready += (s, e) =>
            {
                setGame(null);
                Console.WriteLine(timeStamp + " JXbot is now ready.");
            };

            client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;

            });

            cmds = client.GetService<CommandService>();

            serverName = client.SessionId;

            cmds.CreateCommand("question")
            .Do(async (e) =>
            {
                firstVal = rnd.Next(5, 3000);
                secondVal = rnd.Next(35, 7000);
                sqRoot = rnd.Next(50, 70);
                nmb = rnd.Next(0, 3);
                sin = rnd.Next(5,30);
                switch(nmb){
                    case 0: await e.Channel.SendMessage("What's the square root of " + sqRoot + "?");
                        break;
                    case 1: await e.Channel.SendMessage("What's " + firstVal + "+" + secondVal + "?");
                        comb = firstVal + secondVal;
                        break;
                    case 2: await e.Channel.SendMessage("What's the sin of " + sin + "?");
                        break;
                }
                askedQuestion = true;
            });


            cmds.CreateCommand("reboot")
            .Do(async (e) =>
            {
                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                } else {
                    var reboot = Assembly.GetExecutingAssembly().Location;
                    System.Diagnostics.Process.Start(reboot);
                    await e.Channel.SendMessage("Bot is restarting..");
                    await Task.Delay(500);
                    Environment.Exit(0);
                }
            });

            cmds.CreateCommand("help")
            .Do(async (e) =>
            {
                StreamReader write = new StreamReader("commands.txt");
                String line = write.ReadToEnd();
                await e.Channel.SendMessage(line);
                write.Close();
            });

            cmds.CreateCommand("kick")
           .Parameter("user", Discord.Commands.ParameterType.Required)
           .Do(async (e) =>
           {
               if (!e.User.ServerPermissions.ManageRoles)
               {
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
               }
               else
               {
                   var user = e.Args[0].ToUpper();
                   if (user.Contains("!"))
                   {
                       user = user.Replace("<@!", "");
                       user = user.Replace(">", "");
                   }
                   else
                   {
                       user = user.Replace("<@", "");
                       user = user.Replace(">", "");
                   }
                   var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                   var ID = userToKick.Id;
                   await userToKick.Kick();
                   await e.Channel.SendMessage("<@" + ID + "> has been ***KICKED***!");
               }
           });

            cmds.CreateCommand("ban")
           .Parameter("user", Discord.Commands.ParameterType.Required)
           .Do(async (e) =>
           {
               if (!e.User.ServerPermissions.ManageRoles) {
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
               } else {
                   var user = e.Args[0].ToUpper();
                   if (user.Contains("!"))
                   {
                       user = user.Replace("<@!", "");
                       user = user.Replace(">", "");
                   }
                   else
                   {
                       user = user.Replace("<@", "");
                       user = user.Replace(">", "");
                   }
                   var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                   var ID = userToKick.Id;
                   if (!e.User.Roles.Any(c => c.Name.ToUpper() == "ADMIN" || !e.User.Roles.Any(x => x.Name.ToUpper() == "AREN" && ID == 223987673365217281) || ID == 191290329985581069))
                   {
                       await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> why would YOU be able to type that in?");
                   }
                   else
                   {
                       await userToKick.Server.Ban(userToKick);
                       await e.Channel.SendMessage("<@" + ID + "> has been ***BANNED***!");
                   }
               }
           });
            cmds.CreateCommand("unban")
            .Parameter("user", Discord.Commands.ParameterType.Required)
            .Do(async (e) =>
            {
                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                }
                else
                {
                    var user = e.Args[0].ToUpper();
                    if (user.Contains("!"))
                    {
                        user = user.Replace("<@!", "");
                        user = user.Replace(">", "");
                    }
                    else
                    {
                        user = user.Replace("<@", "");
                        user = user.Replace(">", "");
                    }
                    var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                    var ID = userToKick.Id;
                    await userToKick.Server.Unban(userToKick);
                    await e.Channel.SendMessage("<@" + ID + "> has been ***UNBANNED***!");
                }
            });

            cmds.CreateCommand("warn")
           .Parameter("user", ParameterType.Multiple)
           .Do(async (e) =>
           {
               if (!e.User.ServerPermissions.ManageRoles)
               {
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
               }
               else
               {
                   var user = e.Args[0].ToUpper(); ;
                   if (user.Contains("!"))
                   {
                       user = user.Replace("<@!", "");
                       user = user.Replace(">", "");
                   }
                   else
                   {
                       user = user.Replace("<@", "");
                       user = user.Replace(">", "");
                   }
                   var reason = "";
                   for (int i = 1; i < e.Args.Length; i++)
                   {
                       reason += e.Args[i].ToString() + " ";
                   }
                   var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                   var ID = userToKick.Id;
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has been ***WARNED*** Reason: " + reason);
                   await userToKick.SendMessage("You have been warned in JXKGS. Reason: " + reason);
                   ; FS(userToKick.ToString(), reason);
               }
           });

            cmds.CreateCommand("clearwarn")
           .Parameter("user", ParameterType.Multiple)
           .Do(async (e) =>
           {
               if (!e.User.ServerPermissions.ManageRoles)
               {
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
               }
               else
               {
                   var user = e.Args[0].ToUpper();
                   if (user.Contains("!"))
                   {
                       user = user.Replace("<@!", "");
                       user = user.Replace(">", "");
                   }
                   else
                   {
                       user = user.Replace("<@", "");
                       user = user.Replace(">", "");
                   }
                   var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                   var ID = userToKick.Id;
                   var oldLines = System.IO.File.ReadAllLines("warnings-" + serverName + ".txt");
                   var newLines = oldLines.Where(line => !line.Contains(userToKick.ToString())); ;
                   File.WriteAllLines("warnings-" + serverName + ".txt", newLines);
                   await e.Channel.SendMessage("Warnings for <@" + ID + "> have been deleted.");
               }
           });
            cmds.CreateCommand("listwarn")
            .Parameter("user", ParameterType.Multiple)
            .Do(async (e) =>
            {

                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                }
                else
                {
                    var user = e.Args[0].ToUpper();
                    if (user.Contains("!"))
                    {
                        user = user.Replace("<@!", "");
                        user = user.Replace(">", "");
                    }
                    else
                    {
                        user = user.Replace("<@", "");
                        user = user.Replace(">", "");
                    }
                    var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                    var ID = userToKick.Id;
                    var result = File.ReadAllLines("warnings-" + serverName + ".txt").Where(c => c.Contains(userToKick.ToString())).FirstOrDefault();
                    if (result == null)
                    {
                        await e.Channel.SendMessage("<:zelda:301087844741414922> This user has no warnings.");
                    }
                    else
                    {
                        //await e.Channel.SendMessage("```" + result + "```");
                    }
                }
            });

            cmds.CreateCommand("mute")
           .Parameter("user", Discord.Commands.ParameterType.Multiple)
           .Do(async (e) =>
           {
               if (!e.User.ServerPermissions.ManageRoles)
               {
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
               }
               else
               {
                   var user = e.Args[0].ToUpper();
                   if (user.Contains("!"))
                   {
                       user = user.Replace("<@!", "");
                       user = user.Replace(">", "");
                   }
                   else
                   {
                       user = user.Replace("<@", "");
                       user = user.Replace(">", "");
                   }
                   var reason = "";
                   for (int i = 1; i < e.Args.Length; i++)
                   {
                       reason += e.Args[i].ToString() + " ";
                   }
                   var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                   var ID = userToKick.Id;
                   Discord.Role roles = (Discord.Role)e.Server.GetRole(301058219676008448);
                   await userToKick.AddRoles(roles);
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has been ***MUTED*** Reason: " + reason);
                   await userToKick.SendMessage("You have been muted in JXKGS. Reason: " + reason);
                   StreamWriter write = new StreamWriter("warnings-" + serverName + ".txt", true);
                   write.WriteLine(timeStamp + " " + userToKick + " was muted. Reason: " + reason);
                   write.Close();
               }
           });

            cmds.CreateCommand("unmute")
            .Parameter("user", Discord.Commands.ParameterType.Required)
            .Do(async (e) =>
            {
                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                }
                else
                {
                    var user = e.Args[0].ToUpper();
                    if (user.Contains("!"))
                    {
                        user = user.Replace("<@!", "");
                        user = user.Replace(">", "");
                    }
                    else
                    {
                        user = user.Replace("<@", "");
                        user = user.Replace(">", "");
                    }
                    var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                    var ID = userToKick.Id;
                    Discord.Role roles = (Discord.Role)e.Server.GetRole(301058219676008448);
                    await userToKick.RemoveRoles(roles);
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has been ***UNMUTED***");
                }
            });

            cmds.CreateCommand("addrole")
            .Parameter("user", ParameterType.Multiple)
            .Do(async (e) =>
            {
                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                }
                else
                {
                    var user = e.Args[0].ToUpper();
                    var role = e.Args[1].ToUpper();
                    if (user.Contains("!"))
                    {
                        user = user.Replace("<@!", "");
                        user = user.Replace(">", "");
                    }
                    else
                    {
                        user = user.Replace("<@", "");
                        user = user.Replace(">", "");
                    }
                    var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                    var ID = userToKick.Id;
                    Discord.Role getRole = e.Server.FindRoles(role,false).FirstOrDefault();
                    if(!e.Server.Roles.Contains(getRole)){
                        await e.Channel.SendMessage("Didn't find role " + getRole.Name);
                    }
                    if (e.User.Roles.Any(c => c.Name.ToUpper() == "MOD" && getRole.Id == 301064839835549696) || getRole.Id == 301033615687680000)
                    {
                        await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> why would YOU be able to type that in?");
                    }
                    else
                    {
                        await userToKick.AddRoles(getRole);
                        await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has received the " + getRole.Name + " role");
                    }
                }
            });

            cmds.CreateCommand("delrole")
            .Parameter("user", ParameterType.Multiple)
            .Do(async (e) =>
            {
                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                }
                else
                {
                    var user = e.Args[0].ToUpper();
                    var role = e.Args[1].ToString();
                    if (user.Contains("!"))
                    {
                        user = user.Replace("<@!", "");
                        user = user.Replace(">", "");
                    }
                    else
                    {
                        user = user.Replace("<@", "");
                        user = user.Replace(">", "");
                    }
                    role = role.Replace("<@&", "");
                    role = role.Replace(">", "");
                    var userToKick = e.Channel.Users.Where(input => input.Id.ToString() == user).FirstOrDefault();
                    var ID = userToKick.Id;
                    Discord.Role getRole = e.Server.FindRoles(role, false).FirstOrDefault();
                    if (!e.Server.Roles.Contains(getRole))
                    {
                        await e.Channel.SendMessage("Didn't find role " + getRole.Name);
                    }
                    if (e.User.Roles.Any(c => c.Name.ToUpper() == "ADMIN" && getRole.Id == 301064839835549696) || getRole.Id == 301033615687680000)
                    {
                        await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> why would YOU be able to type that in?");
                    }
                    else
                    {
                        await userToKick.RemoveRoles(getRole);
                        await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + ID + "> has been removed from the " + getRole.Name + " role");
                    }
                }
            });

            cmds.CreateCommand("createrole")
            .Parameter("user", ParameterType.Required)
            .Do(async (e) =>
            {
                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                }
                else
                {
                    var role = e.Args[0].ToString();
                    await e.Server.CreateRole(role, null, null, false, true);
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> The role " + role + " has been added");
                }
            });
            cmds.CreateCommand("ping").Do(async (e) =>
            {
                await e.Channel.SendMessage("pong!");
            });

            cmds.CreateCommand("status").Do(async (e) =>
            {
                await e.Channel.SendMessage("<:fireemblem:301087475508707328> Bot is " + e.User.Status);
            });

            cmds.CreateCommand("kill")
           .Do(async (e) =>
           {
               if (!e.User.ServerPermissions.ManageRoles)
               {
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
               }
               else
               {
                   await e.Channel.SendMessage("You killed me >:(");
                   await Task.Delay(500);
                   Environment.Exit(0);
               }
           });

            cmds.CreateCommand("announce")
            .Parameter("channel", ParameterType.Multiple).Do(async (e) =>
            {
                if (!e.User.ServerPermissions.ManageRoles)
                {
                    await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
                }
                else
                {
                    await DoAnnouncement(e);
                }
            });

            RegisterPurgeCommand();

            client.ExecuteAndWait(async () =>
            {
                await client.Connect(File.ReadAllText("token.cfg"), TokenType.Bot);
                });
        }

        void setGame(object o)
        {
            var game = "";
            Random random = new Random();
            switch ((random.Next(0, 6)))
            {
                case 0:
                    client.SetGame("testing bugs");
                    break;
                case 1:
                    client.SetGame("fixing stuff");
                    break;
                case 2:
                    client.SetGame("existing");
                    break;
                case 3:
                    client.SetGame("with life");
                    break;
                case 4:
                    client.SetGame("secretly hating Jason");
                    break;
                case 5:
                    client.SetGame("being annoyed by Wrappers");
                    break;
            }
        }

        private void RegisterPurgeCommand()
        {
            cmds.CreateCommand("purge")
           .Alias(new string[] { "clear", "remove" })
           .Parameter("amount", Discord.Commands.ParameterType.Optional)
           .Do(async (e) =>
           {
               if (!e.User.ServerPermissions.ManageRoles)
               {
                   await e.Channel.SendMessage("<:fireemblem:301087475508707328> <@" + e.User.Id + "> What? You're not a member of the staff!");
               }
               else
               {
                   int amountToDelete = 100;
                   int.TryParse(e.GetArg("amount"), out amountToDelete);
                   Message[] messagesToDelete;
                   messagesToDelete = await e.Channel.DownloadMessages((int)amountToDelete + 1);
                   await e.Channel.DeleteMessages(messagesToDelete);
                   await e.Channel.SendMessage($"Deleted {amountToDelete} messages :)"); ;
               }
           });
        }

        private async Task DoAnnouncement(CommandEventArgs e)
        {
            var channel = e.Server.FindChannels(e.Args[0], ChannelType.Text).FirstOrDefault();

            var message = ConstructMessage(e, channel != null);

            if(channel != null){
                await channel.SendMessage(message);
            } else {
                await e.Channel.SendMessage(message);
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
