// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

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
                drawableRuleset.Overlays.Add(new MetronomeBeatContainer(drawableRuleset.Beatmap.HitObjects.First().StartTime));
        }

        public class MetronomeBeatContainer : BeatSyncedContainer
        {
            private readonly double firstHitTime;

            private PausableSkinnableSound sample;

            public MetronomeBeatContainer(double firstHitTime)
            {
                this.firstHitTime = firstHitTime;
                AllowMistimedEventFiring = false;
                Divisor = 1;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    sample = new PausableSkinnableSound(new SampleInfo("Gameplay/catch-banana"))
                };
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                if (!IsBeatSyncedWithTrack) return;

                int timeSignature = (int)timingPoint.TimeSignature;

                // play metronome from one measure before the first object.
                if (BeatSyncClock.CurrentTime < firstHitTime - timingPoint.BeatLength * timeSignature)
                    return;

                sample.Frequency.Value = beatIndex % timeSignature == 0 ? 1 : 0.5f;
                sample.Play();
            }
        }
    }
}
