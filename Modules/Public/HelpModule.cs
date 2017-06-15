/*
MIT License
Copyright (c) JayXKanz666 2017
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using Discord;
using Discord.Commands;
using JXbot.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JXbot.Modules.Public
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;

        public HelpModule(CommandService service)           // Create a constructor for the commandservice dependency
        {
            _service = service;
        }

        [Command("help")]
        [Alias("halp")]
        public async Task HelpAsync(string command = "")
        {
            if(command == "")
            {
                string prefix = Configuration.Load().Prefix;

                var footerbuilder = new EmbedFooterBuilder()
                {
                    Text = "For more information, please visit: https://github.com/jayxkanz666/JXbot"

                };
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 33, 161),
                    Footer = footerbuilder,
                    Description = "These are the commands you can use:"
                };

                foreach (var module in _service.Modules)
                {
                    string description = null;
                    foreach (var cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                            // if(description.Contains(cm))
                            description += $"{prefix}{cmd.Aliases.First()}\n";
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        builder.AddField(x =>
                        {
                            x.Name = module.Name;
                            x.Value = description;
                            x.IsInline = true;
                        });
                    }
                }

                await ReplyAsync("", false, builder.Build());
            } else
            {
                var result = _service.Search(Context, command);

                if (!result.IsSuccess)
                {
                    await ReplyAsync($"**Error:** The command **{command}** does not exist.");
                    return;
                }

                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 33, 161),
                    Description = $"Here are some commands like **{command}**"
                };

                foreach (var match in result.Commands)
                {
                    var cmd = match.Command;

                    builder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = $"Summary: {cmd.Summary}\n" +
                                  $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n";
                                  //+
                                //  $"Remarks: {cmd.Remarks}";
                        x.IsInline = false;
                    });
                }

                await ReplyAsync("", false, builder.Build());
            }
            
        }
    }
}