using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotEthan {

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BlacklistAttribute : CheckBaseAttribute {
        public ulong[] BlacklistedMembers { get; set; }

        public BlacklistAttribute(params ulong[] blacklist) {
            BlacklistedMembers = blacklist;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) {
            return Task.FromResult(!BlacklistedMembers.Any(x => x == ctx.Member.Id));
        }
    }
}