using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiscordBotEthan.Commands {

    public class Piston : BaseCommandModule {

        [Command("Piston"), Cooldown(5, 5, CooldownBucketType.Global)]
        public async Task PistonCommand(CommandContext ctx, string language, [RemainingText] string code) {
            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1) {
                await ctx.RespondAsync("You need to wrap the code into a code block.");
                return;
            }

            code = code[cs1..cs2];

            await ctx.RespondAsync("Beginning execution");

            DiscordEmbedBuilder exec = new DiscordEmbedBuilder {
                Title = "Piston | Returned",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };

            using var client = new WebClient();

            Dictionary<string, dynamic> obj = new Dictionary<string, dynamic> {
                ["language"] = language,
                ["version"] = "*",
                ["files"] = new List<Dictionary<string, string>> { new Dictionary<string, string> { { "content", code } } }
            };

            try {
                var request = client.UploadString("https://emkc.org/api/v2/piston/execute", "POST", JsonConvert.SerializeObject(obj));
                dynamic Response = JsonConvert.DeserializeObject(request);

                if (Response.run.stdout != null) {
                    exec.Description = Response.run.stdout;
                } else if (Response.run.stderr != null) {
                    exec.Description = Response.run.stderr;
                } else if (Response.run.output != null) {
                    exec.Description = Response.run.output;
                } else {
                    exec.Description = "Failed";
                }
            } catch (WebException e) {
                exec.Description = "**WebException**: " + e.Message;
            }
            await ctx.RespondAsync(exec);
        }
    }
}