// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNightcore : ModDoubleTime
    {
        public override string Name => "Nightcore";
        public override string Acronym => "NC";
        public override IconUsage? Icon => OsuIcon.ModNightcore;
        public override string Description => "Uguuuuuuuu...";
    }

    public abstract class ModNightcore<TObject> : ModNightcore, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        protected ModNightcore()
        {
            SpeedChange.BindValueChanged(val =>
            {
                freqAdjust.Value = SpeedChange.Default;
                tempoAdjust.Value = val.NewValue / SpeedChange.Default;
            }, true);
        }

        public override void ApplyToTrack(ITrack track)
        {
            // base.ApplyToTrack() intentionally not called (different tempo adjustment is applied)
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new NightcoreBeatContainer());
        }

        public class NightcoreBeatContainer : BeatSyncedContainer
        {
            private PausableSkinnableSound hatSample;
            private PausableSkinnableSound clapSample;
            private PausableSkinnableSound kickSample;
            private PausableSkinnableSound finishSample;

            private int? firstBeat;

            public NightcoreBeatContainer()
            {
                Divisor = 2;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    hatSample = new PausableSkinnableSound(new SampleInfo("Gameplay/nightcore-hat")),
                    clapSample = new PausableSkinnableSound(new SampleInfo("Gameplay/nightcore-clap")),
                    kickSample = new PausableSkinnableSound(new SampleInfo("Gameplay/nightcore-kick")),
                    finishSample = new PausableSkinnableSound(new SampleInfo("Gameplay/nightcore-finish")),
                };
            }

            private const int bars_per_segment = 4;

            protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes)
            {
                base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

                int beatsPerBar = timingPoint.TimeSignature.Numerator;
                int segmentLength = beatsPerBar * Divisor * bars_per_segment;

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

            private void playBeatFor(int beatIndex, TimeSignature signature)
            {
                if (beatIndex == 0)
                    finishSample?.Play();

                switch (signature.Numerator)
                {
                    case 3:
                        switch (beatIndex % 6)
                        {
                            case 0:
                                kickSample?.Play();
                                break;

                            case 3:
                                clapSample?.Play();
                                break;

                            default:
                                hatSample?.Play();
                                break;
                        }

                        break;

                    case 4:
                        switch (beatIndex % 4)
                        {
                            case 0:
                                kickSample?.Play();
                                break;

                            case 2:
                                clapSample?.Play();
                                break;

                            default:
                                hatSample?.Play();
                                break;
                        }

                        break;
                }
            }
        }
    }
}
