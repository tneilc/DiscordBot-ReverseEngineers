using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    [Group("Clear"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
    public class Clear : BaseCommandModule {

        [GroupCommand, Description("Removes a amount of messages or recreates the Channel")]
        public async Task ClearCommand(CommandContext ctx, [Description("Amount of messages to delete")] int amount) {
            try {
                var Messages = await ctx.Channel.GetMessagesAsync(amount + 1);
                await ctx.Channel.DeleteMessagesAsync(Messages);

                var msg = await ctx.RespondAsync($"{amount} messages deleted");
                await Task.Delay(3000);
                await msg.DeleteAsync();
            } catch (DSharpPlus.Exceptions.BadRequestException) {
                await ctx.RespondAsync("Messages are older then 14 Days\nDiscord API no like do .Clear all instead");
            }
        }

        [Command("All"), Description("Recreates the Channel to delete messages")]
        public async Task AllCommand(CommandContext ctx) {
            try {
                var TempPos = ctx.Channel.Position;
                var NewCh = await ctx.Channel.CloneAsync();
                await ctx.Channel.DeleteAsync();
                await NewCh.ModifyPositionAsync(TempPos);
            } catch (Exception) {
                await ctx.RespondAsync("Something horrible happend, Command faulted");
            }
        }
    }
}