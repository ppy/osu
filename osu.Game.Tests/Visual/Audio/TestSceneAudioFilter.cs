// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Audio.Effects;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.Audio
{
    public class TestSceneAudioFilter : OsuTestScene
    {
        private OsuSpriteText lowpassText;
        private AudioFilter lowpassFilter;

        private OsuSpriteText highpassText;
        private AudioFilter highpassFilter;

        private Track track;

        private WaveformTestBeatmap beatmap;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            beatmap = new WaveformTestBeatmap(audio);
            track = beatmap.LoadTrack();

            Add(new FillFlowContainer
            {
                Children = new Drawable[]
                {
                    lowpassFilter = new AudioFilter(audio.TrackMixer),
                    highpassFilter = new AudioFilter(audio.TrackMixer, BQFType.HighPass),
                    lowpassText = new OsuSpriteText
                    {
                        Padding = new MarginPadding(20),
                        Text = $"Low Pass: {lowpassFilter.Cutoff.Value}hz",
                        Font = new FontUsage(size: 40)
                    },
                    new OsuSliderBar<int>
                    {
                        Width = 500,
                        Height = 50,
                        Padding = new MarginPadding(20),
                        Current = { BindTarget = lowpassFilter.Cutoff }
                    },
                    highpassText = new OsuSpriteText
                    {
                        Padding = new MarginPadding(20),
                        Text = $"High Pass: {highpassFilter.Cutoff.Value}hz",
                        Font = new FontUsage(size: 40)
                    },
                    new OsuSliderBar<int>
                    {
                        Width = 500,
                        Height = 50,
                        Padding = new MarginPadding(20),
                        Current = { BindTarget = highpassFilter.Cutoff }
                    }
                }
            });
            lowpassFilter.Cutoff.ValueChanged += e => lowpassText.Text = $"Low Pass: {e.NewValue}hz";
            highpassFilter.Cutoff.ValueChanged += e => highpassText.Text = $"High Pass: {e.NewValue}hz";
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Play Track", () => track.Start());
            waitTrackPlay();
        }

        [Test]
        public void TestLowPass()
        {
            AddStep("Filter Sweep", () =>
            {
                lowpassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF).Then()
                             .CutoffTo(0, 2000, Easing.OutCubic);
            });

            waitTrackPlay();

            AddStep("Filter Sweep (reverse)", () =>
            {
                lowpassFilter.CutoffTo(0).Then()
                             .CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 2000, Easing.InCubic);
            });

            waitTrackPlay();
            AddStep("Stop track", () => track.Stop());
        }

        [Test]
        public void TestHighPass()
        {
            AddStep("Filter Sweep", () =>
            {
                highpassFilter.CutoffTo(0).Then()
                              .CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF, 2000, Easing.InCubic);
            });

            waitTrackPlay();

            AddStep("Filter Sweep (reverse)", () =>
            {
                highpassFilter.CutoffTo(AudioFilter.MAX_LOWPASS_CUTOFF).Then()
                              .CutoffTo(0, 2000, Easing.OutCubic);
            });

            waitTrackPlay();

            AddStep("Stop track", () => track.Stop());
        }

        private void waitTrackPlay() => AddWaitStep("Let track play", 10);
    }
}
