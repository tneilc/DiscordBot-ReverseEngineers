using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Repython : BaseCommandModule {
        private readonly string[] BlacklistedCode = { "os", "socket", "importlib", "sys", "subprocess", "asyncore", "hostname", "ping", "shutdown", "system", "__import__", "eval" };

        [Command("Repython"), Blacklist(353243266579431424, 566653752451399700), Cooldown(2, 10, CooldownBucketType.User), RequireRoles(RoleCheckMode.Any, "coder", "C# Global Elite"), Hidden]
        public async Task RepythonCommand(CommandContext ctx, [RemainingText] string code) {
            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1) {
                await ctx.RespondAsync("You need to wrap the code into a code block.");
                return;
            }

            code = code[cs1..cs2].Replace("\"", "'");

            if (BlacklistedCode.Any(x => code.ToLower().Contains(x))) {
                await ctx.RespondAsync($"Something doesn't seem right.. <@{Program.BotOwner}> verify this");

                var msg = await ctx.Channel.GetNextMessageAsync(x => x.Author.Id == Program.BotOwner);
                if (!string.Equals(msg.Result.Content, "verified", StringComparison.OrdinalIgnoreCase) || msg.TimedOut) {
                    await ctx.RespondAsync("Bot Owner didn't verify of execution");
                    return;
                }
            }

            var BeginMsg = await ctx.RespondAsync("Beginning execution");

            DiscordEmbedBuilder exec = new DiscordEmbedBuilder {
                Title = $"Execution | Result",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };

            try {
                var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "python3",
                        Arguments = $"-c \"{code}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                if (!proc.Start() || !proc.WaitForExit(5000)) {
                    proc.Kill();

                    exec.Description = "Timeout";
                    var tempmsg = await ctx.RespondAsync(exec);

                    var Jokin = await ctx.Guild.GetMemberAsync(Program.BotOwner);
                    await Jokin.SendMessageAsync("Timeout on repython command. Check\n" + tempmsg.JumpLink);
                    return;
                }

                var result = await proc.StandardOutput.ReadToEndAsync().ConfigureAwait(false);

                if (result != null && !string.IsNullOrWhiteSpace(result)) {
                    exec.Description = result;
                } else {
                    result = await proc.StandardError.ReadToEndAsync().ConfigureAwait(false);
                    exec.Description = result ?? "No error but no return either";
                }
            } catch (Exception ex) {
                exec.Description = "You fucked up\n" + string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message);
            }
            await BeginMsg.ModifyAsync(exec.Build());
        }
    }
}