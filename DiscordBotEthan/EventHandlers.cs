using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using JokinsCommon;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static DiscordBotEthan.Program;

namespace DiscordBotEthan {

    public static class EventHandlers {

        public static Task Discord_Ready(DiscordClient dc, DSharpPlus.EventArgs.ReadyEventArgs args) {
            _ = Task.Run(async () => {
                dc.Logger.LogInformation("Looking for Reminders");
                var output = await SQLC.GetReminders();

                if (output.Any()) {
                    foreach (var item in output) {
                        long ID = item.ID;
                        long ChannelID = item.ChannelID;
                        long Date = item.Date;
                        string Reminder = item.Reminder;

                        try {
                            DiscordGuild Guild = await dc.GetGuildAsync(GuildID);
                            DiscordMember member = await Guild.GetMemberAsync((ulong)ID);
                            DiscordChannel channel = Guild.GetChannel((ulong)ChannelID);
                            DateTime dateTime = DateTime.FromBinary(Date);

                            if (dateTime < DateTime.Now) {
                                await SQLC.DeleteRemindersWithDate(Date);
                                await channel.SendMessageAsync($":alarm_clock:, {member.Mention} you wanted me to remind you the following but I'm Late:\n\n{Reminder}");
                                continue;
                            }

                            _ = Task.Run(async () => {
                                await Task.Delay((int)dateTime.Subtract(DateTime.Now).TotalMilliseconds);

                                DiscordGuild Guild = await dc.GetGuildAsync(GuildID);
                                DiscordMember member = await Guild.GetMemberAsync((ulong)ID);
                                DiscordChannel channel = Guild.GetChannel((ulong)ChannelID);

                                await channel.SendMessageAsync($":alarm_clock:, {member.Mention} you wanted me to remind you the following:\n\n{Reminder}");

                                await SQLC.DeleteRemindersWithDate(Date);
                            });
                        } catch (Exception) {
                            await SQLC.DeleteRemindersWithDate(Date);
                            continue;
                        }
                    }
                    dc.Logger.LogInformation("Found Reminders and started them");
                } else {
                    dc.Logger.LogInformation("No Reminders found");
                }

                dc.Logger.LogInformation("Looking for muted Members");
                output = await SQLC.GetTempmutes();
                if (output.Any()) {
                    foreach (var item in output) {
                        long ID = item.ID;
                        long Date = item.Date;

                        try {
                            DiscordGuild Guild = await dc.GetGuildAsync(GuildID);
                            DiscordRole MutedRole = Guild.GetRole(Program.MutedRole);
                            DiscordMember member = await Guild.GetMemberAsync((ulong)ID);
                            DateTime dateTime = DateTime.FromBinary(Date);

                            if (dateTime < DateTime.Now) {
                                await SQLC.DeleteTempmutesWithID(ID);
                                await member.RevokeRoleAsync(MutedRole);
                                continue;
                            }

                            _ = Task.Run(async () => {
                                try {
                                    await Task.Delay((int)dateTime.Subtract(DateTime.Now).TotalMilliseconds);

                                    DiscordGuild Guild = await dc.GetGuildAsync(GuildID);
                                    DiscordRole MutedRole = Guild.GetRole(Program.MutedRole);
                                    DiscordMember member = await Guild.GetMemberAsync((ulong)ID);

                                    var PS = await SQLC.GetPlayer(member.Id);
                                    PS.Muted = false;
                                    await PS.Save();

                                    await member.RevokeRoleAsync(MutedRole);
                                    await SQLC.DeleteTempmutesWithID(ID);
                                } catch (Exception) {
                                    dc.Logger.LogInformation($"Failed the Tempmute process for {member.Username + member.Discriminator}");
                                }
                            });
                        } catch (Exception) {
                            await SQLC.DeleteTempmutesWithID(ID);
                            continue;
                        }
                    }
                    dc.Logger.LogInformation("Found muted Members and starting them");
                } else {
                    dc.Logger.LogInformation("No muted Members found");
                }

                while (true) {
                    foreach (var Status in Statuses) {
                        DiscordActivity activity = new DiscordActivity {
                            ActivityType = ActivityType.Playing,
                            Name = Status
                        };
                        await dc.UpdateStatusAsync(activity, UserStatus.DoNotDisturb);
                        dc.Logger.LogInformation("Status Update");
                        await Task.Delay(120000);
                    }
                }
            });
            return Task.CompletedTask;
        }

        private static readonly Dictionary<ulong, List<string>> UsersLastMessages = new Dictionary<ulong, List<string>>();

        public static Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs args) {
            _ = Task.Run(async () => {
                if (args.Author.IsBot)
                    return;

                var PS = await SQLC.GetPlayer(args.Author.Id);

                if (!UsersLastMessages.ContainsKey(args.Author.Id)) {
                    UsersLastMessages.Add(args.Author.Id, new List<string>());
                } else if (UsersLastMessages[args.Author.Id].Count > 1 && UsersLastMessages[args.Author.Id].Contains(args.Message.Content.Trim())) {
                    await PS.Warn(args.Channel, "Spamming");
                    UsersLastMessages[args.Author.Id].Clear();
                } else if (UsersLastMessages[args.Author.Id].Contains(args.Message.Content.Trim())) {
                    UsersLastMessages[args.Author.Id].Add(args.Message.Content.Trim());
                } else {
                    UsersLastMessages[args.Author.Id].Clear();
                    UsersLastMessages[args.Author.Id].Add(args.Message.Content.Trim());
                }

                if (new Random().Next(500) == 1) {
                    using WebClient client = new WebClient();
                    await args.Message.RespondAsync(client.DownloadString("https://insult.mattbas.org/api/insult"));
                }

                string stripped = args.Message.Content.RemoveString(" ", ".").ToLower();

                if (args.Message.Attachments.Count > 0) {
                    foreach (var attach in args.Message.Attachments) {
                        if (attach.FileName.EndsWith("exe")) {
                            await args.Message.DeleteAsync("EXE File");
                            await PS.Warn(args.Channel, "Uploading a EXE File");
                        } else if (attach.FileName.EndsWith("dll")) {
                            await args.Message.DeleteAsync("DLL File");
                            await PS.Warn(args.Channel, "Uploading a DLL File");
                        }
                    }
                } else if (stripped.Contains("discordgg")) {
                    await args.Message.DeleteAsync();
                    await PS.Warn(args.Channel, "Invite Link");
                } else if (stripped.Contains("nigger") || stripped.Contains("nigga")) {
                    await PS.Warn(args.Channel, "Saying the N-Word");
                    await args.Message.RespondAsync("Keep up the racism and you will get banned\nUse nig, nibba instead atleast");
                }

                await PS.Save();
            });

            return Task.CompletedTask;
        }

        public static async Task Discord_GuildMemberAdded(DiscordClient dc, DSharpPlus.EventArgs.GuildMemberAddEventArgs args) {
            await args.Member.GrantRoleAsync(args.Guild.GetRole(LearnerRole));

            var PS = await SQLC.GetPlayer(args.Member.Id);
            if (PS.Muted) {
                _ = Task.Run(async () => {
                    try {
                        DiscordRole MutedRole = args.Guild.GetRole(Program.MutedRole);
                        await args.Member.GrantRoleAsync(MutedRole);
                        await Task.Delay(86400000);
                        var PS = await SQLC.GetPlayer(args.Member.Id);
                        PS.Muted = false;
                        await PS.Save();
                        await args.Member.RevokeRoleAsync(MutedRole);
                    } catch (Exception) {
                        dc.Logger.LogInformation($"Failed the Mute Bypass detection process for {args.Member.Mention}");
                    }
                });
            }
        }

        public static async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs args) {
            foreach (var failedCheck in ((ChecksFailedException)args.Exception).FailedChecks) {
                if (failedCheck is BlacklistAttribute) {
                    await args.Context.RespondAsync("You are on a Blacklist for this Command");
                    return;
                }
            }

            switch (args.Exception) {
                case ArgumentException e:
                    await args.Context.RespondAsync($"Idk what the fuck you want to do with that Command (Argument {e.ParamName ?? "unknown"} is faulty)");
                    break;

                case ChecksFailedException _:
                    await args.Context.RespondAsync("The FBI has been contacted (You don't have the rights for that command)");
                    break;
            }
        }
    }
}