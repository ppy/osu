// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text;
using DiscordRPC;
using DiscordRPC.Message;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Users;
using LogLevel = osu.Framework.Logging.LogLevel;

namespace osu.Desktop
{
    internal partial class DiscordRichPresence : Component
    {
        private const string client_id = "1216669957799018608";

        private DiscordRpcClient client = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuGame game { get; set; } = null!;

        [Resolved]
        private LoginOverlay? login { get; set; }

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private readonly IBindable<UserStatus?> status = new Bindable<UserStatus?>();
        private readonly IBindable<UserActivity> activity = new Bindable<UserActivity>();
        private readonly Bindable<DiscordRichPresenceMode> privacyMode = new Bindable<DiscordRichPresenceMode>();

        private readonly RichPresence presence = new RichPresence
        {
            Assets = new Assets { LargeImageKey = "osu_logo_lazer" },
            Secrets = new Secrets
            {
                JoinSecret = null,
                SpectateSecret = null,
            },
        };

        private IBindable<APIUser>? user;

        [BackgroundDependencyLoader]
        private void load()
        {
            client = new DiscordRpcClient(client_id)
            {
                // SkipIdenticalPresence allows us to fire SetPresence at any point and leave it to the underlying implementation
                // to check whether a difference has actually occurred before sending a command to Discord (with a minor caveat that's handled in onReady).
                SkipIdenticalPresence = true
            };

            client.OnReady += onReady;
            client.OnError += (_, e) => Logger.Log($"An error occurred with Discord RPC Client: {e.Message} ({e.Code})", LoggingTarget.Network, LogLevel.Error);

            try
            {
                client.RegisterUriScheme();
                client.Subscribe(EventType.Join);
                client.OnJoin += onJoin;
            }
            catch (Exception ex)
            {
                // This is known to fail in at least the following sandboxed environments:
                // - macOS (when packaged as an app bundle)
                // - flatpak (see: https://github.com/flathub/sh.ppy.osu/issues/170)
                // There is currently no better way to do this offered by Discord, so the best we can do is simply ignore it for now.
                Logger.Log($"Failed to register Discord URI scheme: {ex}");
            }

            client.Initialize();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            config.BindWith(OsuSetting.DiscordRichPresence, privacyMode);

            user = api.LocalUser.GetBoundCopy();
            user.BindValueChanged(u =>
            {
                status.UnbindBindings();
                status.BindTo(u.NewValue.Status);

                activity.UnbindBindings();
                activity.BindTo(u.NewValue.Activity);
            }, true);

            ruleset.BindValueChanged(_ => schedulePresenceUpdate());
            status.BindValueChanged(_ => schedulePresenceUpdate());
            activity.BindValueChanged(_ => schedulePresenceUpdate());
            privacyMode.BindValueChanged(_ => schedulePresenceUpdate());
            multiplayerClient.RoomUpdated += onRoomUpdated;
        }

        private void onReady(object _, ReadyMessage __)
        {
            Logger.Log("Discord RPC Client ready.", LoggingTarget.Network, LogLevel.Debug);

            // when RPC is lost and reconnected, we have to clear presence state for updatePresence to work (see DiscordRpcClient.SkipIdenticalPresence).
            if (client.CurrentPresence != null)
                client.SetPresence(null);

            schedulePresenceUpdate();
        }

        private void onRoomUpdated() => schedulePresenceUpdate();

        private ScheduledDelegate? presenceUpdateDelegate;

        private void schedulePresenceUpdate()
        {
            presenceUpdateDelegate?.Cancel();
            presenceUpdateDelegate = Scheduler.AddDelayed(() =>
            {
                if (!client.IsInitialized)
                    return;

                if (status.Value == UserStatus.Offline || privacyMode.Value == DiscordRichPresenceMode.Off)
                {
                    client.ClearPresence();
                    return;
                }

                bool hideIdentifiableInformation = privacyMode.Value == DiscordRichPresenceMode.Limited || status.Value == UserStatus.DoNotDisturb;

                updatePresence(hideIdentifiableInformation);
                client.SetPresence(presence);
            }, 200);
        }

        private void updatePresence(bool hideIdentifiableInformation)
        {
            if (user == null)
                return;

            // user activity
            if (activity.Value != null)
            {
                presence.State = truncate(activity.Value.GetStatus(hideIdentifiableInformation));
                presence.Details = truncate(activity.Value.GetDetails(hideIdentifiableInformation) ?? string.Empty);

                if (getBeatmapID(activity.Value) is int beatmapId && beatmapId > 0)
                {
                    presence.Buttons = new[]
                    {
                        new Button
                        {
                            Label = "View beatmap",
                            Url = $@"{api.WebsiteRootUrl}/beatmaps/{beatmapId}?mode={ruleset.Value.ShortName}"
                        }
                    };
                }
                else
                {
                    presence.Buttons = null;
                }
            }
            else
            {
                presence.State = "Idle";
                presence.Details = string.Empty;
            }

            // user party
            if (!hideIdentifiableInformation && multiplayerClient.Room != null)
            {
                MultiplayerRoom room = multiplayerClient.Room;

                presence.Party = new Party
                {
                    Privacy = string.IsNullOrEmpty(room.Settings.Password) ? Party.PrivacySetting.Public : Party.PrivacySetting.Private,
                    ID = room.RoomID.ToString(),
                    // technically lobbies can have infinite users, but Discord needs this to be set to something.
                    // to make party display sensible, assign a powers of two above participants count (8 at minimum).
                    Max = (int)Math.Max(8, Math.Pow(2, Math.Ceiling(Math.Log2(room.Users.Count)))),
                    Size = room.Users.Count,
                };

                RoomSecret roomSecret = new RoomSecret
                {
                    RoomID = room.RoomID,
                    Password = room.Settings.Password,
                };

                if (client.HasRegisteredUriScheme)
                    presence.Secrets.JoinSecret = JsonConvert.SerializeObject(roomSecret);

                // discord cannot handle both secrets and buttons at the same time, so we need to choose something.
                // the multiplayer room seems more important.
                presence.Buttons = null;
            }
            else
            {
                presence.Party = null;
                presence.Secrets.JoinSecret = null;
            }

            // game images:
            // large image tooltip
            if (privacyMode.Value == DiscordRichPresenceMode.Limited)
                presence.Assets.LargeImageText = string.Empty;
            else
            {
                if (user.Value.RulesetsStatistics != null && user.Value.RulesetsStatistics.TryGetValue(ruleset.Value.ShortName, out UserStatistics? statistics))
                    presence.Assets.LargeImageText = $"{user.Value.Username}" + (statistics.GlobalRank > 0 ? $" (rank #{statistics.GlobalRank:N0})" : string.Empty);
                else
                    presence.Assets.LargeImageText = $"{user.Value.Username}" + (user.Value.Statistics?.GlobalRank > 0 ? $" (rank #{user.Value.Statistics.GlobalRank:N0})" : string.Empty);
            }

            // small image
            presence.Assets.SmallImageKey = ruleset.Value.IsLegacyRuleset() ? $"mode_{ruleset.Value.OnlineID}" : "mode_custom";
            presence.Assets.SmallImageText = ruleset.Value.Name;
        }

        private void onJoin(object sender, JoinMessage args) => Scheduler.AddOnce(() =>
        {
            game.Window?.Raise();

            if (!api.IsLoggedIn)
            {
                login?.Show();
                return;
            }

            Logger.Log($"Received room secret from Discord RPC Client: \"{args.Secret}\"", LoggingTarget.Network, LogLevel.Debug);

            // Stable and lazer share the same Discord client ID, meaning they can accept join requests from each other.
            // Since they aren't compatible in multi, see if stable's format is being used and log to avoid confusion.
            if (args.Secret[0] != '{' || !tryParseRoomSecret(args.Secret, out long roomId, out string? password))
            {
                Logger.Log("Could not join multiplayer room, invitation is invalid or incompatible.", LoggingTarget.Network, LogLevel.Important);
                return;
            }

            var request = new GetRoomRequest(roomId);
            request.Success += room => Schedule(() =>
            {
                game.PresentMultiplayerMatch(room, password);
            });
            request.Failure += _ => Logger.Log($"Could not join multiplayer room, room could not be found (room ID: {roomId}).", LoggingTarget.Network, LogLevel.Important);
            api.Queue(request);
        });

        private static readonly int ellipsis_length = Encoding.UTF8.GetByteCount(new[] { '…' });

        private static string truncate(string str)
        {
            if (Encoding.UTF8.GetByteCount(str) <= 128)
                return str;

            ReadOnlyMemory<char> strMem = str.AsMemory();

            do
            {
                strMem = strMem[..^1];
            } while (Encoding.UTF8.GetByteCount(strMem.Span) + ellipsis_length > 128);

            return string.Create(strMem.Length + 1, strMem, (span, mem) =>
            {
                mem.Span.CopyTo(span);
                span[^1] = '…';
            });
        }

        private static bool tryParseRoomSecret(string secretJson, out long roomId, out string? password)
        {
            roomId = 0;
            password = null;

            RoomSecret? roomSecret;

            try
            {
                roomSecret = JsonConvert.DeserializeObject<RoomSecret>(secretJson);
            }
            catch
            {
                return false;
            }

            if (roomSecret == null) return false;

            roomId = roomSecret.RoomID;
            password = roomSecret.Password;

            return true;
        }

        private static int? getBeatmapID(UserActivity activity)
        {
            switch (activity)
            {
                case UserActivity.InGame game:
                    return game.BeatmapID;

                case UserActivity.EditingBeatmap edit:
                    return edit.BeatmapID;
            }

            return null;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (multiplayerClient.IsNotNull())
                multiplayerClient.RoomUpdated -= onRoomUpdated;

            client.Dispose();
            base.Dispose(isDisposing);
        }

        private class RoomSecret
        {
            [JsonProperty(@"roomId", Required = Required.Always)]
            public long RoomID { get; set; }

            [JsonProperty(@"password", Required = Required.AllowNull)]
            public string? Password { get; set; }
        }
    }
}
