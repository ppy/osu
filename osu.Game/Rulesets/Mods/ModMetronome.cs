using System;
using System.Collections.Generic;
using System.Text;
using ManagedBass;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Internal;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osu.Game.Audio;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Framework.Bindables;
using System.Linq;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMetronome : Mod
    {
        public override string Name => "Metronome";
        public override string Acronym => "MT";
        public override IconUsage? Icon => OsuIcon.ModAuto;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Add a metronome";
        public override bool Ranked => false;
        public override double ScoreMultiplier => 0.0;
        public override bool HasImplementation => true;

        private const float min_frequency = 0.25f;

        [SettingSource("Frequency", "Number of beats between each tick of the metronome")]
        public BindableNumber<float> TickFrequency { get; } = new BindableFloat
        {
            MinValue = min_frequency,
            MaxValue = 1/min_frequency,
            Default = 1f,
            Value = 1f,
            Precision = 0.25f,
        };

        protected ModMetronome()
        {
        }

    }

    public abstract class ModMetronome<TObject> : ModMetronome, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new MetronomeBeatContainer(TickFrequency.GetBoundCopy(), 0.25f));
        }

        public class MetronomeBeatContainer : BeatSyncedContainer
        {
            
            private float freq;

            private SkinnableSound metronomeSample;

            private BindableNumber<float> tickFrequency;

            public MetronomeBeatContainer(BindableNumber<float> tickFrequency, float minFrequency)
            {
                this.tickFrequency = tickFrequency;
                this.tickFrequency.BindValueChanged(val =>
                {
                    Divisor = (int)(1 / minFrequency);
                    freq = tickFrequency.Value * Divisor;
                }, true);
                
            }

            private int? firstBeat;
            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = metronomeSample =  new SkinnableSound(new Audio.SampleInfo("nightcore-hat"));
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
                    playBeatFor(beatIndex % segmentLength, timingPoint.TimeSignature);

            }

            private void playBeatFor(int beatIndex, TimeSignatures signature)
            {
                if (beatIndex % freq == 0)
                    metronomeSample.Play();
        
            }


        }
    }

}
