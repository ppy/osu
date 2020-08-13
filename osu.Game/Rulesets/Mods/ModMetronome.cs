// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMetronome : Mod
    {
        public override string Name => "Metronome";
        public override string Acronym => "MT";
        public override IconUsage? Icon => OsuIcon.ModAuto; // temporary icon
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Add a metronome";
        public override bool Ranked => false;
        public override double ScoreMultiplier => 0.0;

        public enum TickFrequency
        {
            One = 1,
            Two = 2,
            Four = 4
        }

        [SettingSource("Tick frequency", "Number of metronome ticks per beat")]
        public Bindable<TickFrequency> Frequency { get; } = new Bindable<TickFrequency>(TickFrequency.One);
    }

    public abstract class ModMetronome<TObject> : ModMetronome, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new MetronomeBeatContainer(Frequency.GetBoundCopy()));
        }

        public class MetronomeBeatContainer : BeatSyncedContainer
        {
            private SkinnableSound metronomeSample;

            public MetronomeBeatContainer(Bindable<TickFrequency> tickFrequency)
            {
                tickFrequency.BindValueChanged(val =>
                {
                    Divisor = (int)tickFrequency.Value;
                }, true);
            }

            private int? firstBeat;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = metronomeSample = new SkinnableSound(new SampleInfo("nightcore-hat")); // temporary sample
            }

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);
                int beatsPerBar = (int)timingPoint.TimeSignature;
                int segmentLength = beatsPerBar * Divisor * 4;

                if (!IsBeatSyncedWithTrack)
                {
                    firstBeat = null;
                    return;
                }

                if (!firstBeat.HasValue || beatIndex < firstBeat)
                    // decide on a good starting beat index if once has not yet been decided.
                    firstBeat = beatIndex < 0 ? 0 : (beatIndex / segmentLength + 1) * segmentLength;

                if (beatIndex >= firstBeat)
                    metronomeSample.Play();
            }
        }
    }
}
