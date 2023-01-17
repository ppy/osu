// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Threading;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneWaveform : OsuTestScene
    {
        private IWorkingBeatmap waveformBeatmap;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            waveformBeatmap = new WaveformTestBeatmap(audio);
        }

        [TestCase(1f)]
        [TestCase(1f / 2)]
        [TestCase(1f / 4)]
        [TestCase(1f / 8)]
        [TestCase(1f / 16)]
        [TestCase(0f)]
        public void TestResolution(float resolution)
        {
            TestWaveformGraph graph = null;

            AddStep("add graph", () =>
            {
                Child = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Children = new Drawable[]
                    {
                        graph = new TestWaveformGraph
                        {
                            RelativeSizeAxes = Axes.Both,
                            Resolution = resolution,
                            Waveform = waveformBeatmap.Waveform,
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.75f
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = $"Resolution: {resolution:0.00}"
                                }
                            }
                        }
                    }
                };
            });

            AddUntilStep("wait for load", () => graph.Loaded.IsSet);
        }

        [Test]
        public void TestDefaultBeatmap()
        {
            TestWaveformGraph graph = null;

            AddStep("add graph", () =>
            {
                Child = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Child = graph = new TestWaveformGraph
                    {
                        RelativeSizeAxes = Axes.Both,
                        Waveform = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo).Waveform,
                    },
                };
            });

            AddUntilStep("wait for load", () => graph.Loaded.IsSet);
        }

        public partial class TestWaveformGraph : WaveformGraph
        {
            public readonly ManualResetEventSlim Loaded = new ManualResetEventSlim();

            protected override void OnWaveformRegenerated(Waveform waveform)
            {
                base.OnWaveformRegenerated(waveform);
                Loaded.Set();
            }
        }
    }
}
