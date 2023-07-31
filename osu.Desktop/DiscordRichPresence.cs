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
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Users;
using LogLevel = osu.Framework.Logging.LogLevel;

namespace osu.Desktop
{
    internal partial class DiscordRichPresence : Component
    {
        private const string client_id = "367827983903490050";
        private const string default_image_key = "osu_logo_lazer";

        private DiscordRpcClient client = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        private IBindable<APIUser> user = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        private readonly IBindable<UserStatus> status = new Bindable<UserStatus>();
        private readonly IBindable<UserActivity> activity = new Bindable<UserActivity>();
        private readonly Bindable<IBeatmapSetOnlineInfo?> beatmapSetOnline = new Bindable<IBeatmapSetOnlineInfo?>();

        private readonly Bindable<DiscordRichPresenceMode> privacyMode = new Bindable<DiscordRichPresenceMode>();

        private readonly RichPresence presence = new RichPresence
        {
            Assets = new Assets { LargeImageKey = default_image_key, }
        };

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            client = new DiscordRpcClient(client_id)
            {
                SkipIdenticalPresence = false // handles better on discord IPC loss, see updateStatus call in onReady.
            };

            client.OnReady += onReady;

            client.OnError += (_, e) => Logger.Log($"An error occurred with Discord RPC Client: {e.Code} {e.Message}", LoggingTarget.Network);

            config.BindWith(OsuSetting.DiscordRichPresence, privacyMode);

            user = api.LocalUser.GetBoundCopy();
            user.BindValueChanged(u =>
            {
                status.UnbindBindings();
                status.BindTo(u.NewValue.Status);

                activity.UnbindBindings();
                activity.BindTo(u.NewValue.Activity);
            }, true);

            activity.BindValueChanged(_ =>
            {
                updateStatus();
                fetchBeatmapSet();
            });

            ruleset.BindValueChanged(_ => updateStatus());
            status.BindValueChanged(_ => updateStatus());
            privacyMode.BindValueChanged(_ => updateStatus());
            beatmapSetOnline.BindValueChanged(_ => updateStatus());

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

            if (status.Value is UserStatusOffline || privacyMode.Value == DiscordRichPresenceMode.Off)
            {
                client.ClearPresence();
                return;
            }

            if (status.Value is UserStatusOnline && activity.Value != null)
            {
                presence.State = truncate(activity.Value.GetStatus(privacyMode.Value == DiscordRichPresenceMode.Limited));
                presence.Details = truncate(getDetails(activity.Value));

                if (getBeatmap(activity.Value) is IBeatmapInfo beatmap && beatmap.OnlineID > 0)
                {
                    presence.Buttons = new[]
                    {
                        new Button
                        {
                            Label = "View beatmap",
                            Url = $@"{api.WebsiteRootUrl}/beatmapsets/{beatmap.BeatmapSet?.OnlineID}#{ruleset.Value.ShortName}/{beatmap.OnlineID}"
                        }
                    };

                    presence.Assets.LargeImageKey = default_image_key;
                    if (beatmapSetOnline.Value != null && Encoding.UTF8.GetByteCount(beatmapSetOnline.Value.Covers.List) <= 256) // Ensure the URL will fit and not throw.
                        presence.Assets.LargeImageKey = beatmapSetOnline.Value.Covers.List;
                }
                else
                {
                    presence.Buttons = null;
                    presence.Assets.LargeImageKey = default_image_key;
                }
            }
            else
            {
                presence.State = "Idle";
                presence.Details = string.Empty;
                presence.Buttons = null;
                presence.Assets.LargeImageKey = default_image_key;
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

        private IBeatmapInfo? getBeatmap(UserActivity activity)
        {
            switch (activity)
            {
                case UserActivity.InGame game:
                    return game.BeatmapInfo;

                case UserActivity.EditingBeatmap edit:
                    return edit.BeatmapInfo;
            }

            return null;
        }

        private async void fetchBeatmapSet()
        {
            IBeatmapInfo? beatmap = getBeatmap(activity.Value);
            if (beatmap == null)
            {
                beatmapSetOnline.Value = null;
                return;
            }

            if (beatmap.BeatmapSet is IBeatmapSetOnlineInfo online)
            {
                beatmapSetOnline.Value = online;
            }
            else
            {
                try
                {
                    var beatmapOnline = await beatmapLookupCache.GetBeatmapAsync(beatmap.OnlineID);
                    if (beatmapOnline != null && beatmapOnline.BeatmapSet != null)
                    {
                        beatmapSetOnline.Value = beatmapOnline.BeatmapSet;
                    }
                    else
                    {
                        beatmapSetOnline.Value = null;
                    }
                }
                catch(Exception)
                {
                    // ignoring exceptions as this is a "best attempt" feature.
                    beatmapSetOnline.Value = null;
                }
            }
        }

        private string getDetails(UserActivity activity)
        {
            switch (activity)
            {
                case UserActivity.InGame game:
                    return game.BeatmapInfo.ToString() ?? string.Empty;

                case UserActivity.EditingBeatmap edit:
                    return edit.BeatmapInfo.ToString() ?? string.Empty;

                case UserActivity.WatchingReplay watching:
                    return watching.BeatmapInfo?.ToString() ?? string.Empty;

                case UserActivity.InLobby lobby:
                    return privacyMode.Value == DiscordRichPresenceMode.Limited ? string.Empty : lobby.Room.Name.Value;
            }

            return string.Empty;
        }

        protected override void Dispose(bool isDisposing)
        {
            client.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
