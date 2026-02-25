// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModNightcore : ModRateAdjust
    {
        public override string Name => "Nightcore";
        public override string Acronym => "NC";
        public override IconUsage? Icon => OsuIcon.ModNightcore;
        public override ModType Type => ModType.DifficultyIncrease;
        public override LocalisableString Description => "Uguuuuuuuu...";
        public override bool Ranked => true;

        [SettingSource("Speed increase", "The actual increase to apply", SettingControlType = typeof(MultiplierSettingsSlider))]
        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble(1.5)
        {
            MinValue = 1.01,
            MaxValue = 2,
            Precision = 0.01,
        };

        private readonly BindableNumber<double> tempoAdjust = new BindableDouble(1);
        private readonly BindableNumber<double> freqAdjust = new BindableDouble(1);

        private readonly RateAdjustModHelper rateAdjustHelper;

        protected ModNightcore()
        {
            rateAdjustHelper = new RateAdjustModHelper(SpeedChange);

            // intentionally not deferring the speed change handling to `RateAdjustModHelper`
            // as the expected result of operation is not the same (nightcore should preserve constant pitch).
            SpeedChange.BindValueChanged(val =>
            {
                freqAdjust.Value = SpeedChange.Default;
                tempoAdjust.Value = val.NewValue / SpeedChange.Default;
            }, true);
        }

        public override void ApplyToTrack(IAdjustableAudioComponent track)
        {
            track.AddAdjustment(AdjustableProperty.Frequency, freqAdjust);
            track.AddAdjustment(AdjustableProperty.Tempo, tempoAdjust);
        }

        public override double ScoreMultiplier => rateAdjustHelper.ScoreMultiplier;
    }

    public abstract partial class ModNightcore<TObject> : ModNightcore, IApplicableToDrawableRuleset<TObject>
        where TObject : HitObject
    {
        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            drawableRuleset.Overlays.Add(new NightcoreBeatContainer());
        }

        public partial class NightcoreBeatContainer : BeatSyncedContainer
        {
            private PausableSkinnableSound? hatSample;
            private PausableSkinnableSound? clapSample;
            private PausableSkinnableSound? kickSample;
            private PausableSkinnableSound? finishSample;

            private int? firstBeat;
            private int lastBeat = -1;

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
                    firstBeat = beatIndex < 0 ? 0 : (beatIndex / segmentLength) * segmentLength;

                if (beatIndex >= firstBeat)
                    playBeatFor(beatIndex, segmentLength, timingPoint);
            }

            private void playBeatFor(int beatIndex, int segmentLength, TimingControlPoint timingPoint)
            {
                // https://github.com/peppy/osu-stable-reference/blob/6ab0cf1f9f7b3449f5c0d8defcd458aae72cdb88/osu!/Audio/NightcoreBeat.cs#L41
                if (lastBeat == beatIndex)
                    return;

                lastBeat = beatIndex;

                int beatInSegment = beatIndex % segmentLength;

                if (beatInSegment == 0)
                {
                    // https://github.com/peppy/osu-stable-reference/blob/6ab0cf1f9f7b3449f5c0d8defcd458aae72cdb88/osu!/Audio/NightcoreBeat.cs#L53
                    bool playFinish = beatIndex > 0 || !timingPoint.OmitFirstBarLine;

                    if (playFinish)
                        finishSample?.Play();
                }

                switch (timingPoint.TimeSignature.Numerator)
                {
                    case 3:
                        switch (beatInSegment % 6)
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
                        switch (beatInSegment % 4)
                        {
                            case 0:
                                kickSample?.Play();
                                break;

                            case 2:
                                clapSample?.Play();
                                break;

                            default:
                                // note that in stable hat samples would only play if the beatmap tick rate was even
                                // (https://github.com/peppy/osu-stable-reference/blob/6ab0cf1f9f7b3449f5c0d8defcd458aae72cdb88/osu!/Audio/NightcoreBeat.cs#L30-L32)
                                // that kind of presumes that only music timed in 4/4 exists, and does not really work well
                                // if the beatmap e.g. mixes 4/4 and 3/4 signature timing control points.
                                // therefore this conditional behaviour is not reimplemented.
                                hatSample?.Play();
                                break;
                        }

                        break;
                }
            }
        }
    }
}
