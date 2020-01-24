// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogListing : ChangelogContent
    {
        private readonly List<APIChangelogBuild> entries;

        public ChangelogListing(List<APIChangelogBuild> entries)
        {
            this.entries = entries;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            DateTime currentDate = DateTime.MinValue;

            if (entries == null) return;

            foreach (APIChangelogBuild build in entries)
            {
                if (build.CreatedAt.Date != currentDate)
                {
                    if (Children.Count != 0)
                    {
                        Add(new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Colour = new Color4(17, 17, 17, 255),
                            Margin = new MarginPadding { Top = 30 },
                        });
                    }

                    Add(new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = 15 },
                        Text = build.CreatedAt.Date.ToString("dd MMM yyyy"),
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 24),
                        Colour = OsuColour.FromHex(@"FD5"),
                    });

                    currentDate = build.CreatedAt.Date;
                }
                else
                {
                    Add(new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Padding = new MarginPadding { Horizontal = ChangelogBuild.HORIZONTAL_PADDING },
                        Margin = new MarginPadding { Top = 30 },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(32, 24, 35, 255),
                        }
                    });
                }

                Add(new ChangelogBuild(build) { SelectBuild = SelectBuild });
            }
        }
    }
}
