using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace DiscordBotEthan.Commands {

    public class Tag : BaseCommandModule {

        [Command("Tag"), Description("Requests a Tag")]
        public async Task TagCommand(CommandContext ctx, [Description("`highlighting`,`slashcommands`,`lpbaseofdll`,`learnpython`,`intellisense`")] string tagtoshow) {
            switch (tagtoshow.ToLower()) {
                case "highlighting": {
                        DiscordEmbedBuilder Tags = new DiscordEmbedBuilder {
                            Title = "Tag | Highlighting",
                            Description = @"You can denote a specific language for syntax highlighting, by typing the name of the language you want the code block to expect right after the first three backticks (\`\`\`) beginning your code block. An example...

\`\`\`python
def CallFunction():
`    `print('allah')
CallFunction()
\`\`\`
would be
```python
def CallFunction():
    print('allah')
CallFunction()
```",
                            Color = Program.EmbedColor,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                            Timestamp = DateTimeOffset.Now
                        };
                        await ctx.RespondAsync(embed: Tags);
                        break;
                    }

                case "slashcommands": {
                        DiscordEmbedBuilder Tags = new DiscordEmbedBuilder {
                            Title = "Tag | SlashCommands",
                            Description = @"You can do Slash Commands that do ASCII-Art Emojis or quick Shortcuts for something. An example...

/shrug

would be

¯\_(ツ)_/¯

**List of default Slash Commands:**
/shrug
/tenor
/giphy
/tableflip
/unflip
/tts
/me
/spoiler
/nick",

                            Color = Program.EmbedColor,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                            Timestamp = DateTimeOffset.Now
                        };
                        await ctx.RespondAsync(embed: Tags);
                        break;
                    }

                case "lpbaseofdll": {
                        DiscordEmbedBuilder Tags = new DiscordEmbedBuilder {
                            Title = "Tag | lpBaseOfDll",
                            Description = @"There has been trouble coping Code and not listining to Ethan, copy this exact string
`lpBaseOfDll`",
                            Color = Program.EmbedColor,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                            Timestamp = DateTimeOffset.Now
                        };
                        await ctx.RespondAsync(embed: Tags);
                        break;
                    }

                case "learnpython": {
                        DiscordEmbedBuilder Tags = new DiscordEmbedBuilder {
                            Title = "Tag | Learn Python",
                            Description = @"We don't spoonfeed.

You need to atleast learn basics of Python (arrays, for/while loops, etc.)

Starting page to Python and Programming with it itself:
https://www.python.org/about/gettingstarted/

Website for learning Python for free:
https://www.learnpython.org/",
                            Color = Program.EmbedColor,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                            Timestamp = DateTimeOffset.Now
                        };
                        await ctx.RespondAsync(embed: Tags);
                        break;
                    }

                case "intellisense": {
                        DiscordEmbedBuilder Tags = new DiscordEmbedBuilder {
                            Title = "Tag | IntelliSense",
                            Description = @"​Please. Please PLEASE for the love of god use IntelliSense.
Your IDE will show you what parameters a method takes, and/or what properties/methods exists for a given object.
If anything, there's always the documentation.",
                            Color = Program.EmbedColor,
                            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                            Timestamp = DateTimeOffset.Now
                        };
                        await ctx.RespondAsync(embed: Tags);
                        break;
                    }

                default:
                    throw new ArgumentException("Tag not found", nameof(tagtoshow));
            }
        }
    }
}