// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.IPC.Messages;
using osu.Game.IPC.Models;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.IPC.DataSources
{
    public partial class BeatmapStateWebSocketDataSource : WebSocketDataSource
    {
        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;
        private ScheduledDelegate? debouncedModSettingsChange;

        public BeatmapStateWebSocketDataSource(IWebSocketProvider provider)
            : base(provider) { }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            working.BindValueChanged(val =>
            {
                if (val.NewValue.BeatmapInfo.OnlineID == val.OldValue.BeatmapInfo.OnlineID)
                    return;

                broadcastBeatmapState().FireAndForget();
            });

            ruleset.BindValueChanged(val =>
            {
                if (val.NewValue.Equals(val.OldValue))
                    return;

                broadcastBeatmapState().FireAndForget();
            });

            mods.BindValueChanged(val =>
            {
                if (val.OldValue.SequenceEqual(val.NewValue, ReferenceEqualityComparer.Instance))
                    return;

                modSettingChangeTracker?.Dispose();

                broadcastBeatmapState().FireAndForget();

                modSettingChangeTracker = new ModSettingChangeTracker(mods.Value);
                modSettingChangeTracker.SettingChanged += _ =>
                {
                    debouncedModSettingsChange?.Cancel();
                    debouncedModSettingsChange = Scheduler.AddDelayed(() => broadcastBeatmapState().FireAndForget(), 100);
                };
            });
        }

        public override async Task OnClientConnected(int clientId)
        {
            BeatmapStateWebSocketMessage? message = await buildMessage().ConfigureAwait(false);

            if (message is null)
                return;

            SendMessage(clientId, message);
        }

        private async Task broadcastBeatmapState()
        {
            BeatmapStateWebSocketMessage? message = await buildMessage().ConfigureAwait(false);

            if (message is null)
                return;

            BroadcastMessage(message);
        }

        private async Task<BeatmapStateWebSocketMessage?> buildMessage()
        {
            if (working.Value is DummyWorkingBeatmap)
                return null;

            double rate = ModUtils.CalculateRateWithMods(mods.Value);

            var beatmap = working.Value.BeatmapInfo;
            var metadata = beatmap.Metadata;

            var adjustedDifficulty = ruleset.Value.CreateInstance().GetAdjustedDisplayDifficulty(beatmap, mods.Value);
            var starDifficulty = await difficultyCache.GetDifficultyAsync(beatmap, ruleset.Value, mods.Value).ConfigureAwait(false);

            return new BeatmapStateWebSocketMessage
            {
                Beatmap = new WebSocketBeatmap
                {
                    BeatmapId = beatmap.OnlineID,
                    BeatmapSetId = working.Value.BeatmapSetInfo.OnlineID,
                    BeatmapHash = beatmap.OnlineMD5Hash,
                    Metadata = new WebSocketBeatmapMetadata
                    {
                        Artist = metadata.Artist,
                        ArtistUnicode = metadata.ArtistUnicode,
                        Title = metadata.Title,
                        TitleUnicode = metadata.TitleUnicode,
                        Author = metadata.Author.Username,
                        Source = metadata.Source,
                        Tags = metadata.Tags,
                        UserTags = metadata.UserTags.ToArray(),
                    },
                    Difficulty = new WebSocketBeatmapDifficulty
                    {
                        ApproachRate = Math.Round(adjustedDifficulty.ApproachRate, 2),
                        CircleSize = Math.Round(adjustedDifficulty.CircleSize, 2),
                        DrainRate = Math.Round(adjustedDifficulty.DrainRate, 2),
                        OverallDifficulty = Math.Round(adjustedDifficulty.OverallDifficulty, 2),
                    },
                    DifficultyName = beatmap.DifficultyName,
                    RulesetId = beatmap.Ruleset.OnlineID,
                    BPM = FormatUtils.RoundBPM(beatmap.BPM, rate),
                    StarRating = starDifficulty?.Stars.FloorToDecimalDigits(2) ?? beatmap.StarRating.FloorToDecimalDigits(2),
                    MaximumPP = Math.Round(starDifficulty?.PerformanceAttributes?.Total ?? 0, 2),
                    MaxCombo = starDifficulty?.MaxCombo ?? 0,
                    Status = beatmap.Status,
                    TotalLength = (int)Math.Round(beatmap.Length / rate),
                    DrainLength = (int)Math.Round(working.Value.Beatmap.CalculateDrainLength() / rate),
                    ObjectCount = beatmap.TotalObjectCount,
                },
                RulesetId = ruleset.Value.OnlineID,
                Mods = mods.Value.Select(modToWebSocketMod).ToArray(),
            };
        }

        private static WebSocketMod modToWebSocketMod(Mod mod)
        {
            var settings = new Dictionary<string, object>();

            foreach (var (_, property) in mod.GetSettingsSourceProperties())
            {
                var bindable = (IBindable)property.GetValue(mod)!;

                if (!bindable.IsDefault)
                    settings.Add(property.Name.ToSnakeCase(), bindable.GetUnderlyingSettingValue());
            }

            return new WebSocketMod { Acronym = mod.Acronym, Settings = settings };
        }
    }
}
