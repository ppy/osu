// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using ManagedBass.Fx;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Audio.Effects;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.Audio
{
    public partial class TestSceneAudioFilter : OsuTestScene
    {
        private OsuSpriteText lowPassText;
        private AudioFilter lowPassFilter;

        private OsuSpriteText highPassText;
        private AudioFilter highPassFilter;

        private Track track;

        private WaveformTestBeatmap beatmap;

        private RoundedSliderBar<int> lowPassSlider;
        private RoundedSliderBar<int> highPassSlider;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            beatmap = new WaveformTestBeatmap(audio);
            track = beatmap.LoadTrack().GetUnderlyingTrack();

            Add(new FillFlowContainer
            {
                Children = new Drawable[]
                {
                    lowPassFilter = new AudioFilter(audio.TrackMixer),
                    highPassFilter = new AudioFilter(audio.TrackMixer, BQFType.HighPass),
                    lowPassText = new OsuSpriteText
                    {
                        Padding = new MarginPadding(20),
                        Text = $"Low Pass: {lowPassFilter.Cutoff}hz",
                        Font = new FontUsage(size: 40)
                    },
                    lowPassSlider = new RoundedSliderBar<int>
                    {
                        Width = 500,
                        Height = 50,
                        Padding = new MarginPadding(20),
                        Current = new BindableInt
                        {
                            MinValue = 0,
                            MaxValue = AudioFilter.MAX_LOWPASS_CUTOFF,
                        }
                    },
                    highPassText = new OsuSpriteText
                    {
                        Padding = new MarginPadding(20),
                        Text = $"High Pass: {highPassFilter.Cutoff}hz",
                        Font = new FontUsage(size: 40)
                    },
                    highPassSlider = new RoundedSliderBar<int>
                    {
                        Width = 500,
                        Height = 50,
                        Padding = new MarginPadding(20),
                        Current = new BindableInt
                        {
                            MinValue = 0,
                            MaxValue = AudioFilter.MAX_LOWPASS_CUTOFF,
                        }
                    }
                }
            });

            lowPassSlider.Current.ValueChanged += e =>
            {
                lowPassText.Text = $"Low Pass: {e.NewValue}hz";
                lowPassFilter.Cutoff = e.NewValue;
            };

            highPassSlider.Current.ValueChanged += e =>
            {
                highPassText.Text = $"High Pass: {e.NewValue}hz";
                highPassFilter.Cutoff = e.NewValue;
            };
        }

        #region Overrides of Drawable

        protected override void Update()
        {
            base.Update();
            highPassSlider.Current.Value = highPassFilter.Cutoff;
            lowPassSlider.Current.Value = lowPassFilter.Cutoff;
        }

        #endregion

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Play Track", () => track.Start());

            AddStep("Reset filters", () =>
            {
                lowPassFilter.Cutoff = AudioFilter.MAX_LOWPASS_CUTOFF;
                highPassFilter.Cutoff = 0;
            });

            waitTrackPlay();
        }

        [Test]
        public void TestLowPassSweep()
        {
            AddStep("Filter Sweep", () =>
            {
                lowPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF).Then()
                             .CutoffTo(0, 2000, Easing.OutCubic);
            });

            waitTrackPlay();

            AddStep("Filter Sweep (reverse)", () =>
            {
                lowPassFilter.CutoffTo(0).Then()
                             .CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 2000, Easing.InCubic);
            });

            waitTrackPlay();
            AddStep("Stop track", () => track.Stop());
        }

        [Test]
        public void TestHighPassSweep()
        {
            AddStep("Filter Sweep", () =>
            {
                highPassFilter.CutoffTo(0).Then()
                              .CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 2000, Easing.InCubic);
            });

            waitTrackPlay();

            AddStep("Filter Sweep (reverse)", () =>
            {
                highPassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF).Then()
                              .CutoffTo(0, 2000, Easing.OutCubic);
            });

            waitTrackPlay();

            AddStep("Stop track", () => track.Stop());
        }

        private void waitTrackPlay() => AddWaitStep("Let track play", 10);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            track?.Dispose();
        }
    }
}
