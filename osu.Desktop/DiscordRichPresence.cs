// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text;
using DiscordRPC;
using DiscordRPC.Message;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Users;
using LogLevel = osu.Framework.Logging.LogLevel;

namespace osu.Desktop
{
    internal partial class DiscordRichPresence : Component
    {
        private const string client_id = "367827983903490050";

        private DiscordRpcClient client = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        private IBindable<APIUser> user = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuGame game { get; set; } = null!;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

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

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            client = new DiscordRpcClient(client_id)
            {
                SkipIdenticalPresence = false // handles better on discord IPC loss, see updateStatus call in onReady.
            };

            client.OnReady += onReady;
            client.OnError += (_, e) => Logger.Log($"An error occurred with Discord RPC Client: {e.Code} {e.Message}", LoggingTarget.Network, LogLevel.Error);

            // A URI scheme is required to support game invitations, as well as informing Discord of the game executable path to support launching the game when a user clicks on join/spectate.
            client.RegisterUriScheme();
            client.Subscribe(EventType.Join);
            client.OnJoin += onJoin;

            config.BindWith(OsuSetting.DiscordRichPresence, privacyMode);

            user = api.LocalUser.GetBoundCopy();
            user.BindValueChanged(u =>
            {
                status.UnbindBindings();
                status.BindTo(u.NewValue.Status);

                activity.UnbindBindings();
                activity.BindTo(u.NewValue.Activity);
            }, true);

            ruleset.BindValueChanged(_ => updateStatus());
            status.BindValueChanged(_ => updateStatus());
            activity.BindValueChanged(_ => updateStatus());
            privacyMode.BindValueChanged(_ => updateStatus());

            client.Initialize();
        }

        private void onReady(object _, ReadyMessage __)
        {
            Logger.Log("Discord RPC Client ready.", LoggingTarget.Network, LogLevel.Debug);
            updateStatus();
        }

        private void updateStatus()
        {
            if (!client.IsInitialized)
                return;

            if (status.Value == UserStatus.Offline || privacyMode.Value == DiscordRichPresenceMode.Off)
            {
                client.ClearPresence();
                return;
            }

            if (activity.Value != null)
            {
                bool hideIdentifiableInformation = privacyMode.Value == DiscordRichPresenceMode.Limited || status.Value == UserStatus.DoNotDisturb;

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

                if (!hideIdentifiableInformation && multiplayerClient.Room != null)
                {
                    MultiplayerRoom room = multiplayerClient.Room;
                    presence.Party = new Party
                    {
                        Privacy = string.IsNullOrEmpty(room.Settings.Password) ? Party.PrivacySetting.Public : Party.PrivacySetting.Private,
                        ID = room.RoomID.ToString(),
                        // technically lobbies can have infinite users, but Discord needs this to be set to something.
                        // 1024 just happens to look nice.
                        // https://discord.com/channels/188630481301012481/188630652340404224/1212967974793642034
                        Max = 1024,
                        Size = room.Users.Count,
                    };

                    RoomSecret roomSecret = new RoomSecret
                    {
                        RoomID = room.RoomID,
                        Password = room.Settings.Password,
                    };

                    presence.Secrets.JoinSecret = JsonConvert.SerializeObject(roomSecret);
                }
                else
                {
                    presence.Party = null;
                    presence.Secrets.JoinSecret = null;
                }
            }
            else
            {
                presence.State = "Idle";
                presence.Details = string.Empty;
            }

            // update user information
            if (privacyMode.Value == DiscordRichPresenceMode.Limited)
                presence.Assets.LargeImageText = string.Empty;
            else
            {
                if (user.Value.RulesetsStatistics != null && user.Value.RulesetsStatistics.TryGetValue(ruleset.Value.ShortName, out UserStatistics? statistics))
                    presence.Assets.LargeImageText = $"{user.Value.Username}" + (statistics.GlobalRank > 0 ? $" (rank #{statistics.GlobalRank:N0})" : string.Empty);
                else
                    presence.Assets.LargeImageText = $"{user.Value.Username}" + (user.Value.Statistics?.GlobalRank > 0 ? $" (rank #{user.Value.Statistics.GlobalRank:N0})" : string.Empty);
            }

            // update ruleset
            presence.Assets.SmallImageKey = ruleset.Value.IsLegacyRuleset() ? $"mode_{ruleset.Value.OnlineID}" : "mode_custom";
            presence.Assets.SmallImageText = ruleset.Value.Name;

            client.SetPresence(presence);
        }

        private void onJoin(object sender, JoinMessage args)
        {
            game.Window?.Raise();
            Logger.Log($"Received room secret from Discord RPC Client: {args.Secret}", LoggingTarget.Network, LogLevel.Debug);

            if (!tryParseRoomSecret(args.Secret, out long roomId, out string? password))
            {
                Logger.Log("Could not parse room from Discord RPC Client", LoggingTarget.Network, LogLevel.Important);
                return;
            }

            var request = new GetRoomRequest(roomId);
            request.Success += room => Schedule(() =>
            {
                game.PresentMultiplayerMatch(room, password);
            });
            request.Failure += _ => Logger.Log($"Could not find room {roomId} from Discord RPC Client", LoggingTarget.Network, LogLevel.Important);
            api.Queue(request);
        }

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
