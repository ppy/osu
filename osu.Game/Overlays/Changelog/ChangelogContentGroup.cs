// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Collections.Generic;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContentGroup : FillFlowContainer
    {
        private readonly TooltipIconButton chevronPrevious, chevronNext;

        public Action NextRequested, PreviousRequested;
        public readonly TextFlowContainer ChangelogEntries;

        public ChangelogContentGroup(APIChangelog build)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding
            {
                Left = 70,
                Right = 70,
            };
            Children = new Drawable[]
            {
                // build version, arrows
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding
                    {
                        Top = 20,
                    },
                    Children = new Drawable[]
                    {
                        chevronPrevious = new TooltipIconButton
                        {
                            IsEnabled = false,
                            Icon = FontAwesome.fa_chevron_left,
                            Size = new Vector2(24),
                            Action = () => PreviousRequested(),
                        },
                        new FillFlowContainer<SpriteText>
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding
                            {
                                Left = 40,
                                Right = 40,
                            },
                            Children = new[]
                            {
                                new SpriteText
                                {
                                    Text = build.UpdateStream.DisplayName,
                                    TextSize = 28, // web: 24,
                                    Font = @"Exo2.0-Medium",
                                },
                                new SpriteText
                                {
                                    Text = " ",
                                    TextSize = 28,
                                },
                                new SpriteText
                                {
                                    Text = build.DisplayVersion,
                                    TextSize = 28, // web: 24,
                                    Font = @"Exo2.0-Light",
                                    Colour = StreamColour.FromStreamName(build.UpdateStream.Name),
                                },
                            }
                        },
                        chevronNext = new TooltipIconButton
                        {
                            IsEnabled = false,
                            Icon = FontAwesome.fa_chevron_right,
                            Size = new Vector2(24),
                            Action = () => NextRequested(),
                        },
                    }
                },
                new SpriteText
                {
                    // do we need .ToUniversalTime() here?
                    // also, this should be a temporary solution to weekdays in >localized< date strings
                    Text = build.CreatedAt.HasValue ? build.CreatedAt.Value.Date.ToLongDateString()
                        .Replace(build.CreatedAt.Value.ToString("dddd") + ", ", "") : null,
                    TextSize = 17, // web: 14,
                    Colour = OsuColour.FromHex(@"FD5"),
                    Font = @"Exo2.0-Medium",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding
                    {
                        Top = 5,
                    },
                },
                ChangelogEntries = new TextFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };
        }

        public void UpdateChevronTooltips(string previousVersion, string nextVersion)
        {
            if (!string.IsNullOrEmpty(previousVersion))
            {
                chevronPrevious.TooltipText = previousVersion;
                chevronPrevious.IsEnabled = true;
            }
            if (!string.IsNullOrEmpty(nextVersion))
            {
                chevronNext.TooltipText = nextVersion;
                chevronNext.IsEnabled = true;
            }
        }

        public void GenerateText(List<ChangelogEntry> changelogEntries)
        {
            foreach (ChangelogEntry entry in changelogEntries)
            {
                ChangelogEntries.AddParagraph(entry.Type);
                ChangelogEntries.AddParagraph(entry.Title);
                ChangelogEntries.AddText($"({entry.Repository}#{entry.GithubPullRequestId})");
                ChangelogEntries.AddText($"by {entry.GithubUser.DisplayName}");
            }
        }
        //public ChangelogContentGroup() { } // for listing
    }
}
