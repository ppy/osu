// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio.Effects;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.Audio
{
    public class TestSceneFilter : OsuTestScene
    {
        private WorkingBeatmap testBeatmap;
        private OsuSpriteText lowpassText;
        private OsuSpriteText highpassText;
        private Filter lowpassFilter;
        private Filter highpassFilter;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            testBeatmap = new WaveformTestBeatmap(audio);
            Add(new FillFlowContainer
            {
                Children = new Drawable[]
                {
                    lowpassFilter = new Filter(audio.TrackMixer),
                    highpassFilter = new Filter(audio.TrackMixer, BQFType.HighPass),
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

        [Test]
        public void TestLowPass() => testFilter(lowpassFilter, lowpassFilter.MaxCutoff, 0);

        [Test]
        public void TestHighPass() => testFilter(highpassFilter, 0, highpassFilter.MaxCutoff);

        private void testFilter(Filter filter, int cutoffFrom, int cutoffTo)
        {
            AddStep("Load Track", () => testBeatmap.LoadTrack());
            AddStep("Play Track", () => testBeatmap.Track.Start());
            AddWaitStep("Let track play", 10);
            AddStep("Filter Sweep", () =>
            {
                filter.CutoffTo(cutoffFrom).Then()
                      .CutoffTo(cutoffTo, 2000, cutoffFrom > cutoffTo ? Easing.OutCubic : Easing.InCubic);
            });
            AddWaitStep("Let track play", 10);
            AddStep("Filter Sweep (reverse)", () =>
            {
                filter.CutoffTo(cutoffTo).Then()
                      .CutoffTo(cutoffFrom, 2000, cutoffTo > cutoffFrom ? Easing.OutCubic : Easing.InCubic);
            });
            AddWaitStep("Let track play", 10);
            AddStep("Stop track", () => testBeatmap.Track.Stop());
        }
    }
}
