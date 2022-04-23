using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModPracticeMode : Mod
    {
        public override string Name => "Practice Mode";
        public override string Acronym => "PM";
        public override string Description => @"Start at a percentage of the beatmap";
        public override IconUsage? Icon => FontAwesome.Solid.Memory;

        public abstract BindableNumber<double> SkipTo { get; }
    }

    public abstract class ModPracticeMode<TObject> : ModPracticeMode, IApplicableAfterBeatmapConversion
        where TObject : HitObject
    {
        public override double ScoreMultiplier => 1.0;
        public override ModType Type => ModType.DifficultyReduction;
        public override Type[] IncompatibleMods => new Type[] { };
        [SettingSource("Skip", "The percentage of the level to skip", SettingControlType = typeof(SettingsSlider<double, PracticeModeSkipSlider>))]
        public override BindableNumber<double> SkipTo { get; } = new BindableDouble
        {
            MinValue = 0,
            MaxValue = 1,
            Default = 0.5,
            Value = 0.5,
            Precision = 0.01,
        };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (beatmap is IBeatmap<TObject> beatmapOfObj)
                ApplyToBeatmap(beatmapOfObj);
        }

        public void ApplyToBeatmap(Beatmap beatmap)
        {
            if (beatmap is Beatmap<TObject> beatmapOfObj)
                ApplyToBeatmap(beatmapOfObj);
        }

        public void ApplyToBeatmap(IBeatmap<TObject> beatmap)
        {
            if (beatmap is Beatmap<TObject> beatmapOfObj)
                ApplyToBeatmap(beatmapOfObj);
        }

        public void ApplyToBeatmap(Beatmap<TObject> beatmap)
        {
            var newObjects = new List<TObject>();
            var objs = beatmap.HitObjects;
            double firstObjAt = objs.FirstOrDefault()?.StartTime ?? 0.0;
            double lastObjAt = objs.LastOrDefault()?.GetEndTime() ?? 0.0;

            double minLength = (lastObjAt - firstObjAt) * SkipTo.Value;
            beatmap.HitObjects = objs.Where((h) => (h.StartTime - firstObjAt) >= minLength).ToList();
        }
    }

    public class PracticeModeSkipSlider : OsuSliderBar<double>
    {
        public PracticeModeSkipSlider()
        {
            DisplayAsPercentage = true;
        }
    }
}
