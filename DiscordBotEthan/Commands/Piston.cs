using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
                Title = $"Piston | Returned",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };

            using var client = new WebClient();

            var data = new System.Collections.Specialized.NameValueCollection {
                    { "language", language },
                    { "source", code }
            };

            try {
                var request = client.UploadValues("https://emkc.org/api/v1/piston/execute", "POST", data);
                dynamic Response = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(request));

                if (Response.output != null) {
                    exec.Description = Response.output;
                    await ctx.RespondAsync(exec);
                } else if (Response.stderr != null) {
                    exec.Description = Response.stderr;
                    await ctx.RespondAsync(exec);
                } else {
                    exec.Description = "Failed";
                    await ctx.RespondAsync(exec);
                }
            } catch (WebException) {
                exec.Description = "Failed";
                await ctx.RespondAsync(exec);
            }
        }
    }
}