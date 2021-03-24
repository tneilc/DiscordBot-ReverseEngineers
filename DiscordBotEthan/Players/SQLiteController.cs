using Dapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using static DiscordBotEthan.Program;


namespace DiscordBotEthan.Players {

    public class SQLiteController {

        public struct Player {
            public ulong ID { get; set; }
            public List<string> Warns { get; set; }
            public bool Muted { get; set; }

            internal async Task Save() {
                await new SQLiteController().Save(this);
            }

            internal async Task Warn(DiscordChannel channel, string reason = "No reason specified") {
                DiscordGuild Guild = await discord.GetGuildAsync(GuildID);
                DiscordMember Member = await Guild.GetMemberAsync(ID);


                DiscordEmbedBuilder WarnMessage = new DiscordEmbedBuilder {
                    Title = $"Warns | {Member.Username}",
                    Description = $"**{Member.Mention} has been warned for the following Reason:**\n{reason}\n**Muted: False**",
                    Color = EmbedColor,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = "Made by JokinAce 😎" },
                    Timestamp = DateTimeOffset.Now
                };

                if ((Warns.Count + 1) >= 3) {
                    if (!Muted) {
                        var SQLC = new SQLiteController();

                        DateTime MuteTime = DateTime.Now.AddHours(1);
                        DiscordRole MutedRole = Guild.GetRole(Program.MutedRole);

                        await Member.GrantRoleAsync(MutedRole);
                        await SQLC.AddTempmute((long)Member.Id, MuteTime.ToBinary());

                        Muted = true;

                        WarnMessage.WithDescription($"**{Member.Mention} has been warned for the following Reason:**\n{reason}\n**Muted: True\nUnmuted on {MuteTime:dd.MM.yyyy HH:mm}**");

                        _ = Task.Run(async () => {
                            try {
                                await Task.Delay(86400000);

                                var SQLC = new SQLiteController();
                                DiscordGuild Guild = await discord.GetGuildAsync(GuildID);
                                DiscordRole MutedRole = Guild.GetRole(Program.MutedRole);

                                var PS = await SQLC.GetPlayer(Member.Id);
                                PS.Muted = false;
                                await PS.Save();

                                await SQLC.DeleteTempmutesWithID((long)Member.Id);
                                await Member.RevokeRoleAsync(MutedRole);
                            } catch (Exception) {
                                discord.Logger.LogInformation($"Failed the Warn Tempmute process for {Member.Username + "#" + Member.Discriminator}");
                            }

                        });
                    } else {
                        WarnMessage.WithDescription($"**{Member.Mention} has been warned for the following Reason:**\n{reason}\n**Muted: Already muted**");
                    }
 
                }

                var msg = await channel.SendMessageAsync(WarnMessage);

                Warns.Add($"{reason} | [Event]({msg.JumpLink})");
            }
        }

        public async Task<Player> GetPlayer(ulong ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            var output = await cnn.QuerySingleOrDefaultAsync("SELECT * FROM Players WHERE ID=@id", new { id = ID }).ConfigureAwait(false);

            if (output == null) {
                await cnn.ExecuteAsync($"INSERT INTO Players (ID) VALUES (@id)", new { id = ID }).ConfigureAwait(false);
                output = await cnn.QuerySingleOrDefaultAsync("SELECT * FROM Players WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
            }

            long IDc = output.ID;
            string Warns = output.Warns;
            long Muted = output.Muted;

            Player player = new Player {
                ID = (ulong)IDc,
                Warns = string.IsNullOrEmpty(Warns) ? new List<string>() : Warns.Split(",").ToList(),
                Muted = Convert.ToBoolean(Muted)
            };

            return player;

        }

        public async Task Save(Player player) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            var args = new Dictionary<string, object>{
                {"@id", player.ID},
                {"@warns", string.Join(",", player.Warns)},
                {"@muted", player.Muted}
            };
            await cnn.ExecuteAsync($"UPDATE Players SET Warns=@warns, Muted=@muted WHERE ID=@id", args).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns all Reminders from DB
        /// </summary>
        /// <returns>Dynamic List</returns>

        public async Task<IEnumerable<dynamic>> GetReminders() {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryAsync("SELECT * FROM Reminders").ConfigureAwait(false);
        }

        public async Task<IEnumerable<dynamic>> GetRemindersWithID(long ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryAsync("SELECT * FROM Reminders WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
        }

        public async Task<int> AddReminder(long ID, long ChannelID, long Date, string Reminder) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("INSERT INTO Reminders (ID, ChannelID, Date, Reminder) VALUES (@id, @channelid, @date, @reminder)", new { id = ID, channelid = ChannelID, date = Date, reminder = Reminder }).ConfigureAwait(false);
        }

        public async Task<int> DeleteRemindersWithDate(long Date) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("DELETE FROM Reminders WHERE Date=@date", new { date = Date }).ConfigureAwait(false);
        }

        public async Task<int> DeleteRemindersWithID(long ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("DELETE FROM Reminders WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns all Tempmutes from DB
        /// </summary>
        /// <returns>Dynamic List</returns>

        public async Task<IEnumerable<dynamic>> GetTempmutes() {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryAsync("SELECT * FROM Tempmutes").ConfigureAwait(false);
        }

        public async Task<dynamic> GetTempmuteWithID(long ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.QueryFirstOrDefaultAsync("SELECT * FROM Tempmutes WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
        }

        public async Task<int> AddTempmute(long ID, long Date) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("INSERT INTO Tempmutes (ID, Date) VALUES (@id, @date)", new { id = ID, date = Date }).ConfigureAwait(false);
        }

        public async Task<int> DeleteTempmutesWithID(long ID) {
            using IDbConnection cnn = new SQLiteConnection(ConnString);
            return await cnn.ExecuteAsync("DELETE FROM Tempmutes WHERE ID=@id", new { id = ID }).ConfigureAwait(false);
        }
    }
}