// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Users.Profile
{
    public class RankChart : Container
    {
        private readonly SpriteText rank, performance, relative;
        private readonly LineGraph graph;

        private readonly int[] ranks, performances;

        public RankChart(User user)
        {
            Padding = new MarginPadding { Vertical = 10 };
            Children = new Drawable[]
            {
                rank = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = @"Exo2.0-RegularItalic",
                    TextSize = 25
                },
                relative = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = @"Exo2.0-RegularItalic",
                    Y = 25,
                    TextSize = 13
                },
                performance = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Font = @"Exo2.0-RegularItalic",
                    TextSize = 13
                },
                graph = new LineGraph
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Y = -13,
                    DefaultValueCount = 90
                }
            };

            //placeholder text
            rank.Text = "#12,345";
            relative.Text = $"{user.Country?.FullName} #678";
            performance.Text = "4,567pp";
            ranks = Enumerable.Range(1234, 80).ToArray();
            performances = ranks.Select(x => 6000 - x).ToArray();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            graph.Colour = colours.Yellow;
            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(1000);
                // put placeholder data here to show the transform

                // use logarithmic coordinates
                graph.Values = ranks.Select(x => -(float)Math.Log(x));
            });
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) != 0)
            {
                graph.Height = DrawHeight - 71;
            }

            return base.Invalidate(invalidation, source, shallPropagate);
        }
    }
}
