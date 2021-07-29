// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMuted : Mod
    {
        public override string Name => "Muted";
        public override string Acronym => "MU";
        public override IconUsage? Icon => FontAwesome.Solid.VolumeMute;
        public override string Description => "Can you still feel the rhythm without music?";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
    }

    public abstract class ModMuted<TObject> : ModMuted, IApplicableToDrawableRuleset<TObject>, IApplicableToTrack
        where TObject : HitObject
    {
        private readonly BindableNumber<double> volumeAdjust = new BindableDouble();

        [SettingSource("Enable metronome", "Add a metronome to help you keep track of the rhythm.")]
        public BindableBool EnableMetronome { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public void ApplyToTrack(ITrack track)
        {
            track.AddAdjustment(AdjustableProperty.Volume, volumeAdjust);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            if (EnableMetronome.Value)
                drawableRuleset.Overlays.Add(new Metronome(drawableRuleset.Beatmap.HitObjects.First().StartTime));
        }
    }
}
