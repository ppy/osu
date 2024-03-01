// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text;
using DiscordRPC;
using DiscordRPC.Message;
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
        public const string DISCORD_PROTOCOL = $"discord-{client_id}://";

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

            // set up stuff for spectate/join
            // first, we register a uri scheme for when osu! isn't running and a user clicks join/spectate
            // the rpc library we use also happens to _require_ that we do this
            client.RegisterUriScheme();
            client.Subscribe(EventType.Join); // we have to explicitly tell discord to send us join events.
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

                    presence.Secrets.JoinSecret = $"{room.RoomID}:{room.Settings.Password}";
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
            game.Window?.Raise(); // users will expect to be brought back to osu! when joining a lobby from discord

            if (!tryParseRoomSecret(args.Secret, out long roomId, out string? password))
                Logger.Log("Failed to parse the room secret Discord gave us", LoggingTarget.Network, LogLevel.Error);

            var request = new GetRoomRequest(roomId);
            request.Success += room => Schedule(() =>
            {
                game.PresentMultiplayerMatch(room, password);
            });
            request.Failure += _ => Logger.Log("Couldn't find the room Discord gave us", LoggingTarget.Network, LogLevel.Error);
            api.Queue(request);
        }

        private static readonly int ellipsis_length = Encoding.UTF8.GetByteCount(new[] { '…' });

        private string truncate(string str)
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

        private static bool tryParseRoomSecret(ReadOnlySpan<char> secret, out long roomId, out string? password)
        {
            roomId = 0;
            password = null;

            int roomSecretSplitIndex = secret.IndexOf(':');

            if (roomSecretSplitIndex == -1)
                return false;

            if (!long.TryParse(secret[..roomSecretSplitIndex], out roomId))
                return false;

            // just convert to string here, we're going to have to alloc it later anyways
            password = secret[(roomSecretSplitIndex + 1)..].ToString();
            if (password.Length == 0) password = null;

            return true;
        }

        private int? getBeatmapID(UserActivity activity)
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
    }
}
