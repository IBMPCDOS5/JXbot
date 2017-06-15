/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Discord;
using Discord.Audio;

namespace JXbot.Services
{
    public class AudioService
    {
        public static IAudioClient client;
        private static ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                //await Logger(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
                Console.WriteLine("Connected to voice channel.");
            }

        }

        /*public async Task JoinAudio(IVoiceChannel channel, ulong guildID)
        {

            var audioClient = await channel.ConnectAsync();
        }*/

        public async Task LeaveAudio(IGuild guild)
        {
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
                Console.WriteLine("Left voice channel.");
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");

                var output = CreateStream(path).StandardOutput.BaseStream;
                var stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024);
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public async Task SendLinkAsync(IGuild guild, IMessageChannel channel, string path)
        {
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                var output = CreateLinkStream(path).StandardOutput.BaseStream;
                var stream = client.CreatePCMStream(AudioApplication.Music, 128 * 1024); //, 128 * 1024
                await output.CopyToAsync(stream);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        public async Task StopAudio(IGuild guild)
        {
            await client.StopAsync();
            return;
        }

        private Process CreateStream(string path)
        {
            foreach(var x in Process.GetProcessesByName("ffmpeg.exe"))
            {
                x.Kill();
            }
            
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48100 -b:a 192k pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        private Process CreateLinkStream(string url)
        {
            Process currentsong = new Process();
            foreach (var x in Process.GetProcessesByName("ffmpeg.exe"))
            {
                x.Kill();
            }
            currentsong.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe -o - {url} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48100 -b:a 192k pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            currentsong.Start();
            return currentsong;
        }
    }
}