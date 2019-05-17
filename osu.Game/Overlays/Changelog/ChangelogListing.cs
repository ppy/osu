// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
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
            DateTime currentDate = new DateTime();
            Clear();

            ChangelogContentGroup changelogContentGroup = null;

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

                    changelogContentGroup = new ChangelogContentGroup(build, true);
                    changelogContentGroup.BuildSelected += SelectBuild;
                    changelogContentGroup.GenerateText(build.ChangelogEntries);
                    Add(changelogContentGroup);
                    currentDate = build.CreatedAt.Date;
                }
                else
                {
                    changelogContentGroup?.Add(new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Colour = new Color4(32, 24, 35, 255),
                        Margin = new MarginPadding { Top = 30 },
                    });

                    changelogContentGroup = new ChangelogContentGroup(build, false);
                    changelogContentGroup.BuildSelected += SelectBuild;
                    changelogContentGroup.GenerateText(build.ChangelogEntries);
                    Add(changelogContentGroup);
                }
            }
        }
    }
}
