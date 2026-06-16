// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.IPC.Messages;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.IPC.DataSources
{
    public class PlayerStateWebSocketDataSource : WebSocketDataSource
    {
        private readonly Bindable<WorkingBeatmap> workingBeatmap;
        private readonly IBindable<RulesetInfo> rulesetInfo;
        private readonly IBindable<IReadOnlyList<Mod>> mods;
        private readonly BeatmapDifficultyCache difficultyCache;

        private ModSettingChangeTracker? modSettingChangeTracker;
        private ScheduledDelegate? debouncedModSettingsChange;

        public PlayerStateWebSocketDataSource(IWebSocketProvider provider, Bindable<WorkingBeatmap> workingBeatmap,
                                              IBindable<RulesetInfo> rulesetInfo, IBindable<IReadOnlyList<Mod>> mods,
                                              BeatmapDifficultyCache difficultyCache, GameHost gameHost)
            : base(provider)
        {
            this.workingBeatmap = workingBeatmap;
            this.rulesetInfo = rulesetInfo;
            this.mods = mods;
            this.difficultyCache = difficultyCache;

            workingBeatmap.BindValueChanged(val =>
            {
                if (val.NewValue.BeatmapInfo.OnlineID == val.OldValue.BeatmapInfo.OnlineID)
                    return;

                updatePlayerState().FireAndForget();
            });

            rulesetInfo.BindValueChanged(val =>
            {
                if (val.NewValue.Equals(val.OldValue))
                    return;

                updatePlayerState().FireAndForget();
            });

            mods.BindValueChanged(val =>
            {
                if (val.OldValue.SequenceEqual(val.NewValue, ReferenceEqualityComparer.Instance))
                    return;

                modSettingChangeTracker?.Dispose();

                updatePlayerState().FireAndForget();

                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ =>
                {
                    debouncedModSettingsChange?.Cancel();
                    debouncedModSettingsChange = gameHost.UpdateThread.Scheduler.AddDelayed(() => updatePlayerState().FireAndForget(), 100);
                };
            });
        }

        private async Task updatePlayerState()
        {
            if (workingBeatmap.Value is DummyWorkingBeatmap)
                return;

            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            var ruleset = rulesetInfo.Value.CreateInstance();
            var adjustedDifficulty = ruleset.GetAdjustedDisplayDifficulty(workingBeatmap.Value.BeatmapInfo, mods.Value);

            var starDifficulty = await difficultyCache.GetDifficultyAsync(workingBeatmap.Value.BeatmapInfo, rulesetInfo.Value, mods.Value).ConfigureAwait(false);

            var msg = new PlayerStateWebSocketMessage
            {
                Beatmap = new WebSocketBeatmap
                {
                    BeatmapId = workingBeatmap.Value.BeatmapInfo.OnlineID,
                    BeatmapSetId = workingBeatmap.Value.BeatmapSetInfo.OnlineID,
                    BeatmapHash = workingBeatmap.Value.BeatmapInfo.OnlineMD5Hash,
                    Metadata = new WebSocketBeatmapMetadata
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
                    Difficulty = new WebSocketBeatmapDifficulty
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

            BroadcastMessage(msg);
        }
    }
}
