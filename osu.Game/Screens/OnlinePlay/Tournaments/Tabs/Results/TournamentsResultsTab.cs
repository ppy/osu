// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results
{
    public partial class TournamentsResultsTab : TournamentsBaseTab
    {
        public override TournamentsTab TabType => TournamentsTab.Results;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Masking = true;
            MaskingSmoothness = 0.5f;
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Container // Will be sub-tab manager
                    {
                        RelativeSizeAxes = Axes.Y,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Width = 200,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Colour = new Colour4(34, 34, 34, 255),
                        }
                    },
                    new Container // Will be where sub-tab info is displayed.
                    {
                        RelativeSizeAxes = Axes.Both,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                Colour = new Colour4(255, 34, 34, 128),
                            },

                            new BracketScreen()
                        }
                    }
                }
            };
        }
    }
}
