// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public partial class ChangelogListing : ChangelogContent
    {
        private readonly List<APIChangelogBuild>? entries;

        public ChangelogListing(List<APIChangelogBuild>? entries)
        {
            this.entries = entries;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            var currentDate = DateTime.MinValue;

            if (entries == null) return;

            foreach (var build in entries)
            {
                if (build.CreatedAt.Date != currentDate)
                {
                    if (Children.Count != 0)
                    {
                        Add(new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Colour = colourProvider.Background6,
                            Margin = new MarginPadding { Top = 30 },
                        });
                    }

                    Add(new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = 20 },
                        Text = build.CreatedAt.Date.ToLocalisableString("dd MMMM yyyy"),
                        Font = OsuFont.GetFont(weight: FontWeight.Regular, size: 24),
                    });

                    currentDate = build.CreatedAt.Date;
                }
                else
                {
                    Add(new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Padding = new MarginPadding { Horizontal = WaveOverlayContainer.HORIZONTAL_PADDING },
                        Margin = new MarginPadding { Top = 30 },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background6,
                        }
                    });
                }

                Add(new ChangelogBuild(build) { SelectBuild = SelectBuild });
            }
        }
    }
}
