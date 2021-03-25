using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    [Group("Warns"), Aliases("Warn", "Warnings")]
    public class Warns : BaseCommandModule {

        [GroupCommand, Description("Shows all warns for said Member")]
        public async Task WarnsShowCommand(CommandContext ctx, [Description("The Member as Mention or ID/Username")] DiscordMember member) {
            var PS = await Program.SQLC.GetPlayer(member.Id);

            DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                Title = $"Warns | {member.Username}",
                Description = PS.Warns.Count == 0 ? $"{member.Mention} **has no warnings**" : $"{member.Mention} **has following Warns:**\n" + string.Join("\n", PS.Warns.ToArray()),
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Warns);
        }

        [Command("clear"), Description("Clears all warns for said Member"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task ClearCommand(CommandContext ctx, [Description("The Member as Mention or ID/Username")] DiscordMember member) {
            var PS = await Program.SQLC.GetPlayer(member.Id);
            PS.Warns.Clear();
            await PS.Save();

            DiscordEmbedBuilder Warns = new DiscordEmbedBuilder {
                Title = $"Warns | {member.Username}",
                Description = $"**Warnings have been cleared for:**\n{member.Mention}",
                Color = Program.EmbedColor,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                Timestamp = DateTimeOffset.Now
            };
            await ctx.RespondAsync(embed: Warns);
        }

        [Command("add"), Description("Adds a warn for said Member with a reason"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task AddCommand(CommandContext ctx, [Description("The Member as Mention or ID/Username")] DiscordMember member, [RemainingText, Description("Reason for the warn")] string reason = "No reason specified") {
            var PS = await Program.SQLC.GetPlayer(member.Id);
            await PS.Warn(ctx.Channel, reason);
            await PS.Save();
        }
    }
}