// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Mods
{
    public abstract partial class ModStartPos : Mod, IApplicableToBeatmap, IApplicableToPlayer
    {
        public override string Name => "Start Position";
        public override string Acronym => "SP";
        public override IconUsage? Icon => OsuIcon.ModStartPos;
        public override ModType Type => ModType.Conversion;
        public override LocalisableString Description => "Start from any point in the beatmap.";

        [SettingSource("Start time", "The time from which to start the beatmap.", SettingControlType = typeof(SettingsSlider<double, StartTimeSlider>))]
        public BindableDouble StartTime { get; } = new BindableDouble(0)
        {
            MinValue = 0,
            MaxValue = 1337,
            Precision = 1,
        };

        protected double FirstObjectTime { get; private set; }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (beatmap.HitObjects.Count == 0)
                return;

            FirstObjectTime = beatmap.HitObjects.First().StartTime;
            double lastObjectEnd = beatmap.GetLastObjectTime();
            StartTime.MaxValue = Math.Ceiling((lastObjectEnd - FirstObjectTime) / 1000);
        }

        public void ApplyToPlayer(Player? player)
        {
            if (player == null || StartTime.Value <= 0)
                return;

            double seekTime = StartTime.Value * 1000 + FirstObjectTime;
            player.OnGameplayStarted += () => player.Seek(seekTime);
        }

        public partial class StartTimeSlider : RoundedSliderBar<double>
        {
            [Resolved]
            private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

            public StartTimeSlider()
            {
                KeyboardStep = 1;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (workingBeatmap.Value?.BeatmapInfo.Length > 0 && Current is BindableNumber<double> num)
                    num.MaxValue = Math.Ceiling(workingBeatmap.Value.BeatmapInfo.Length / 1000);
            }

            public override LocalisableString TooltipText => FormatStartTime(Current.Value);

            public static string FormatStartTime(double value)
            {
                int totalSeconds = (int)Math.Floor(value);
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }
    }
}
