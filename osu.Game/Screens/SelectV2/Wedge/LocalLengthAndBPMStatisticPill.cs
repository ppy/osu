// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Screens.SelectV2.Wedge
{
    public partial class LocalLengthAndBPMStatisticPill : LengthAndBPMStatisticPill
    {
        private ModSettingChangeTracker? modSettingChangeTracker;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            workingBeatmap.BindValueChanged(_ => updateStatistics());

            mods.BindValueChanged(m =>
            {
                modSettingChangeTracker?.Dispose();

                updateStatistics();

                modSettingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                modSettingChangeTracker.SettingChanged += _ => updateStatistics();
            }, true);
        }

        private void updateStatistics()
        {
            // TODO: consider mods which apply variable rates.
            double rate = 1;
            foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                rate = mod.ApplyToRate(0, rate);

            var beatmap = workingBeatmap.Value.Beatmap;

            int bpmMax = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMaximum, rate);
            int bpmMin = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMinimum, rate);
            int mostCommonBPM = FormatUtils.RoundBPM(60000 / beatmap.GetMostCommonBeatLength(), rate);

            string labelText = bpmMin == bpmMax
                ? $"{bpmMin}"
                : $"{bpmMin}-{bpmMax} ({mostCommonBPM})";

            BPMStatistic.Value = labelText;

            double drainLength = Math.Round(beatmap.CalculateDrainLength() / rate);
            double hitLength = Math.Round(beatmap.BeatmapInfo.Length / rate);

            LengthStatistic.Value = hitLength.ToFormattedDuration();
            LengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

            BPMStatistic.ValueColour = getColourByRate(rate);
            LengthStatistic.ValueColour = getColourByRate(rate);
        }

        private Colour4 getColourByRate(double rate)
        {
            switch (rate)
            {
                case < 1:
                    return colours.ForModType(ModType.DifficultyReduction);

                case > 1:
                    return colours.ForModType(ModType.DifficultyIncrease);

                default:
                    return colourProvider.Content2;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingChangeTracker?.Dispose();
        }
    }
}
