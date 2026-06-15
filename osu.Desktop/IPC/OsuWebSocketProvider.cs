// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using osu.Desktop.IPC.Messages;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IPC;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;
using osu.Game.Utils;
using Beatmap = osu.Desktop.IPC.Messages.Beatmap;
using BeatmapDifficulty = osu.Desktop.IPC.Messages.BeatmapDifficulty;
using BeatmapMetadata = osu.Desktop.IPC.Messages.BeatmapMetadata;

namespace osu.Desktop.IPC
{
    public partial class OsuWebSocketProvider : Component
    {
        private WebSocketServer? server;
        private readonly Bindable<UserActivity?> userActivity = new Bindable<UserActivity?>();

        [Resolved]
        private Bindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> rulesetInfo { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;
        private ScheduledDelegate? debouncedModSettingsChange;

        private readonly object modSettingsLock = new object();

        [BackgroundDependencyLoader]
        private void load(SessionStatics sessionStatics)
        {
            server = new WebSocketServer(49727);
            server.StartAsync().FireAndForget(onError: ex => Logger.Error(ex, "Failed to start websocket"));

            sessionStatics.BindWith(Static.UserOnlineActivity, userActivity);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userActivity.BindValueChanged(val =>
            {
                if (val.NewValue == null)
                    return;

                if (server?.IsRunning != true)
                    return;

                var msg = new UserActivityMessage
                {
                    Status = val.NewValue.GetType().Name,
                };

                broadcast(msg).FireAndForget();
            }, true);

            workingBeatmap.BindValueChanged(val =>
            {
                if (val.NewValue.BeatmapInfo.OnlineID == val.OldValue.BeatmapInfo.OnlineID)
                    return;

                updatePlayerState().FireAndForget();
            });

            rulesetInfo.BindValueChanged(_ => updatePlayerState().FireAndForget());

            mods.BindValueChanged(val =>
            {
                if (val.OldValue.SequenceEqual(val.NewValue, ReferenceEqualityComparer.Instance))
                    return;

                updatePlayerState().FireAndForget();

                modSettingChangeTracker?.Dispose();

                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ =>
                {
                    lock (modSettingsLock)
                    {
                        debouncedModSettingsChange?.Cancel();
                        debouncedModSettingsChange = Scheduler.AddDelayed(() => updatePlayerState().FireAndForget(), 100);
                    }
                };
            });

            updatePlayerState().FireAndForget();
        }

        private async Task updatePlayerState()
        {
            if (workingBeatmap.Value is DummyWorkingBeatmap)
                return;

            if (server?.IsRunning != true)
                return;

            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            var ruleset = rulesetInfo.Value.CreateInstance();
            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(workingBeatmap.Value.BeatmapInfo, mods.Value);

            var starDifficulty = await difficultyCache.GetDifficultyAsync(workingBeatmap.Value.BeatmapInfo, rulesetInfo.Value, mods.Value).ConfigureAwait(false);

            var msg = new PlayerStateMessage
            {
                Beatmap = new Beatmap
                {
                    BeatmapId = workingBeatmap.Value.BeatmapInfo.OnlineID,
                    BeatmapSetId = workingBeatmap.Value.BeatmapSetInfo.OnlineID,
                    BeatmapHash = workingBeatmap.Value.BeatmapInfo.OnlineMD5Hash,
                    Metadata = new BeatmapMetadata
                    {
                        Artist = workingBeatmap.Value.BeatmapInfo.Metadata.Artist,
                        ArtistUnicode = workingBeatmap.Value.BeatmapInfo.Metadata.ArtistUnicode,
                        Title = workingBeatmap.Value.BeatmapInfo.Metadata.Title,
                        TitleUnicode = workingBeatmap.Value.BeatmapInfo.Metadata.TitleUnicode,
                        Author = workingBeatmap.Value.BeatmapInfo.Metadata.Author.Username,
                        Source = workingBeatmap.Value.BeatmapInfo.Metadata.Source,
                        Tags = workingBeatmap.Value.BeatmapInfo.Metadata.Tags,
                        UserTags = workingBeatmap.Value.BeatmapInfo.Metadata.UserTags.ToArray(),
                    },
                    Difficulty = new BeatmapDifficulty
                    {
                        ApproachRate = Math.Round(adjustedDifficulty.ApproachRate, 2),
                        CircleSize = Math.Round(adjustedDifficulty.CircleSize, 2),
                        DrainRate = Math.Round(adjustedDifficulty.DrainRate, 2),
                        OverallDifficulty = Math.Round(adjustedDifficulty.OverallDifficulty, 2),
                    },
                    DifficultyName = workingBeatmap.Value.BeatmapInfo.DifficultyName,
                    RulesetId = workingBeatmap.Value.BeatmapInfo.Ruleset.OnlineID,
                    BPM = FormatUtils.RoundBPM(workingBeatmap.Value.BeatmapInfo.BPM, rate),
                    StarRating = starDifficulty?.Stars.FloorToDecimalDigits(2) ?? workingBeatmap.Value.BeatmapInfo.StarRating.FloorToDecimalDigits(2),
                    MaxCombo = starDifficulty?.MaxCombo ?? 0,
                    Status = workingBeatmap.Value.BeatmapInfo.Status,
                },
                RulesetId = rulesetInfo.Value.OnlineID,
                Mods = mods.Value.Select(m => new APIMod(m)).ToArray(),
            };

            await broadcast(msg).ConfigureAwait(false);
        }

        private Task broadcast(OsuWebSocketMessage message) => Task.Run(async () =>
        {
            if (server?.IsRunning != true)
                return;

            string messageString = JsonSerializer.Serialize(message, message.GetType());
            await server.BroadcastAsync(messageString).ConfigureAwait(false);
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (server?.IsRunning == true)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));
                server.StopAsync(cts.Token).WaitSafely();
                server = null;
            }

            modSettingChangeTracker?.Dispose();

            debouncedModSettingsChange?.Cancel();
            debouncedModSettingsChange = null;
        }
    }
}
