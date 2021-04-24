// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRandomOsu : Mod, IApplicableToBeatmap
    {
        public override string Name => "Random";
        public override string Acronym => "RD";
        public override IconUsage? Icon => OsuIcon.Dice;
        public override ModType Type => ModType.Conversion;
        public override string Description => "Practice your reaction time!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => false;

        [SettingSource("Circles", "Hit circles appear at random positions")]
        public Bindable<bool> RandomiseCirclePositions { get; } = new BindableBool
        {
            Default = true,
            Value = true,
        };

        [SettingSource("Sliders", "Sliders appear at random positions")]
        public Bindable<bool> RandomiseSliderPositions { get; } = new BindableBool
        {
            Default = true,
            Value = true,
        };

        [SettingSource("Spinners", "Spinners appear at random positions")]
        public Bindable<bool> RandomiseSpinnerPositions { get; } = new BindableBool
        {
            Default = true,
            Value = true,
        };

        public void ApplyToBeatmap(IBeatmap beatmap) => RandomiseHitObjectPositions(beatmap);

        protected abstract void RandomiseHitObjectPositions(IBeatmap beatmap);
    }
}
