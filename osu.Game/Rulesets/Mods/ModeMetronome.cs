// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMetronome : Mod
    {
        public override string Name => "Metronome";
        public override string Acronym => "MN";
        public override IconUsage? Icon => OsuIcon.Metronome;
        public override LocalisableString Description => "Need a little help staying on track? Let the metronome lead the way.";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
        public override bool Ranked => false;
        public override bool ValidForFreestyleAsRequiredMod => false;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {  
            typeof(ModNightcore), 
            typeof(ModRelax),
            typeof(ModCinema),
            typeof(ModMuted),
        }).ToArray();
    }

    public abstract class ModMetronome<TObject> : ModMetronome, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        [SettingSource("Metronome volume", "Adjust the volume of the metronome clicks.")]
        public BindableDouble MetronomeVolume { get; } = new BindableDouble(0.5)
        {
            MinValue = 0,
            MaxValue = 1,
            Precision = 0.01,
        };

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            var firstObject = drawableRuleset.Beatmap.HitObjects.FirstOrDefault();
            if (firstObject == null)
                return;

            MetronomeBeat metronomeBeat;
            drawableRuleset.FrameStableComponents.Add(metronomeBeat = new MetronomeBeat(firstObject.StartTime));
            metronomeBeat.AddAdjustment(AdjustableProperty.Volume, MetronomeVolume);
        }
    }
}
