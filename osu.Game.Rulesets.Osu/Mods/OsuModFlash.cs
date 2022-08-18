// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModFlash : ModWithVisibilityAdjustment, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Var AR Test";
        public override string Acronym => "TP";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Regular.Sun;
        public override string Description => @"how far will your reading stretch";
        public override double ScoreMultiplier => 1.03;

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            base.ApplyToBeatmap(beatmap);
            double lastObjectEnd = beatmap.HitObjects.LastOrDefault()?.GetEndTime() ?? 0;

            foreach (var obj in beatmap.HitObjects.OfType<OsuHitObject>())
                applyVariableAr(obj);

            void applyVariableAr(OsuHitObject osuObject)
            {
                double percentageofmap = osuObject.StartTime / lastObjectEnd;
                osuObject.TimePreempt = (percentageofmap * osuObject.TimePreempt) * ARadded.Value + osuObject.TimePreempt;
                foreach (var nested in osuObject.NestedHitObjects.OfType<OsuHitObject>())
                    applyVariableAr(nested);
            }
        }

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        [SettingSource("Additional AR", "how much AR change to add")]
        public BindableDouble ARadded { get; } = new BindableDouble(1)
        {
            Precision = 0.1f,
            MinValue = 0,
            MaxValue = 10,
        };

        /*[SettingSource("Additional Ar", "how much ar  change to add")]
        public BindableBool scaledown { get; } = new BindableBool();*/
    }
}
