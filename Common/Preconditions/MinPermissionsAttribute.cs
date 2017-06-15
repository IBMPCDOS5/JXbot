/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JXbot.Common.Preconditions
{
    /// <summary>
    /// Set the minimum permission required to use a module or command
    /// similar to how MinPermissions works in Discord.Net 0.9.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MinPermissionsAttribute : PreconditionAttribute
    {
        private AccessLevel Level;

        public MinPermissionsAttribute(AccessLevel level)
        {
            Level = level;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider provider)
        {
            var access = GetPermission(context);            // Get the acccesslevel for this context

            if (access >= Level)                            // If the user's access level is greater than the required level, return success.
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Insufficient permissions."));
        }

        public AccessLevel GetPermission(ICommandContext c)
        {
            if (c.User.IsBot)                                    // Prevent other bots from executing commands.
                return AccessLevel.Blocked;

            if (Configuration.Load().Owners.Contains(c.User.Id)) // Give configured owners special access.
                return AccessLevel.BotOwner;

            var user = c.User as SocketGuildUser;                // Check if the context is in a guild.
            if (user != null)
            {
                if (c.Guild.OwnerId == user.Id)                  // Check if the user is the guild owner.
                    return AccessLevel.ServerOwner;

                if (user.GuildPermissions.Administrator)         // Check if the user has the administrator permission.
                    return AccessLevel.ServerAdmin;

                if (user.GuildPermissions.ManageMessages ||      // Check if the user can ban, kick, or manage messages.
                    user.GuildPermissions.BanMembers ||
                    user.GuildPermissions.KickMembers)
                    return AccessLevel.ServerMod;

                if (user.Roles.Any(role => role.Name == "musicfag"))
                    return AccessLevel.ServerMusic;
            }

            return AccessLevel.User;                             // If nothing else, return a default permission.
        }
    }
}