// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Screens.Compose.Timeline;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseWaveform : OsuTestCase
    {
        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        public TestCaseWaveform()
        {
            FillFlowContainer flow;
            Child = flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new MusicController
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Y = 100,
                        State = Visibility.Visible
                    },
                }
            };

            for (int i = 1; i <= 16; i *= 2)
            {
                var newDisplay = new BeatmapWaveformGraph
                {
                    RelativeSizeAxes = Axes.Both,
                    Resolution = 1f / i
                };

                newDisplay.Beatmap.BindTo(beatmapBacking);

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

        [BackgroundDependencyLoader]
        private void load(OsuGameBase osuGame) => beatmapBacking.BindTo(osuGame.Beatmap);
    }
}
