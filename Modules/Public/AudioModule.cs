/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Threading.Tasks;
using Discord.Commands;
using System.Diagnostics;
using JXbot.Services;
using Discord;
using JXbot.Common.Preconditions;
using JXbot.Common;
using System.IO;
using System.Text.RegularExpressions;

namespace JXbot.Modules.Public
{
    [Name("Audio")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        // Scroll down further for the AudioService.
        // Like, way down.
        // Hit 'End' on your keyboard if you still can't find it.
        // private readonly AudioService _service;

        private AudioService _service = new AudioService();

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        [MinPermissions(AccessLevel.ServerMusic)]
        public async Task JoinCmd()
        {
            var user = Context.User as IGuildUser;

            if (user.VoiceChannel != null)
            {
                await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            }
            else
            {
                await Context.Channel.SendMessageAsync("You need to be in a voice channel to use this command.");
            }
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        [MinPermissions(AccessLevel.ServerMusic)]
        public async Task LeaveCmd()
        {
            var channel = Context.Guild as IGuild;
            await _service.LeaveAudio(channel);
            await Context.Channel.SendMessageAsync("Left voice channel.");
        }

        [Command("play", RunMode = RunMode.Async)]
        [MinPermissions(AccessLevel.ServerMusic)]
        [Summary("Plays music. Usage: [local/yt] [filepath/url]")]
        public async Task PlayCmd(string param, [Remainder] string song)
        {
            var user = Context.User as IGuildUser;

            if (user.VoiceChannel != null)
            {
                if (param == "yt")
                {
                    if(song.StartsWith("<") && song.EndsWith(">"))
                    {
                        song = song.Replace("<", "").Replace(">", "");
                    }
                    await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
                    await Context.Channel.SendMessageAsync($"Started playing: {YoutubeURLParser.TitleParser(song)}");
                    await _service.SendLinkAsync(Context.Guild, Context.Channel, song);
                }
                else if (param == "local")
                {
                    await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
                    await Context.Channel.SendMessageAsync($"Started playing: {Path.GetFileNameWithoutExtension(song)}");
                    await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("You need to be in a voice channel to use this command.");
            }
        }
    }
}