// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestCaseWaveform : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = new WaveformTestBeatmap();

            FillFlowContainer flow;
            Child = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
            };

            for (int i = 1; i <= 16; i *= 2)
            {
                var newDisplay = new WaveformGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Resolution = 1f / i,
                    Waveform = Beatmap.Value.Waveform,
                };

                flow.Add(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 100,
                    Children = new Drawable[]
                    {
                        newDisplay,
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
                                    Text = $"Resolution: {1f / i:0.00}"
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
