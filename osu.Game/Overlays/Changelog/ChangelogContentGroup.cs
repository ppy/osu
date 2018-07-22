// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Collections.Generic;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContentGroup : FillFlowContainer
    {
        private readonly TooltipIconButton chevronPrevious, chevronNext;
        private readonly SortedDictionary<string, List<ChangelogEntry>> categories =
            new SortedDictionary<string, List<ChangelogEntry>>();

        public Action NextRequested, PreviousRequested, BuildRequested;
        public readonly FillFlowContainer ChangelogEntries;

        public ChangelogContentGroup(APIChangelog build)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Horizontal = 70 };
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
                    Text = build.CreatedAt.Date.ToLongDateString().Replace(build.CreatedAt.ToString("dddd") + ", ", ""),
                    TextSize = 17, // web: 14,
                    Colour = OsuColour.FromHex(@"FD5"),
                    Font = @"Exo2.0-Medium",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding{ Top = 5, }
                },
                ChangelogEntries = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                },
            };
        }

        public ChangelogContentGroup(APIChangelog build, bool newDate = false)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Horizontal = 70 };
            Children = new Drawable[]
            {
                new SpriteText
                {
                    // do we need .ToUniversalTime() here?
                    // also, this should be a temporary solution to weekdays in >localized< date strings
                    Text = build.CreatedAt.Date.ToLongDateString().Replace(build.CreatedAt.ToString("dddd") + ", ", ""),
                    TextSize = 28, // web: 24,
                    Colour = OsuColour.FromHex(@"FD5"),
                    Font = @"Exo2.0-Light",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding{ Top = 20, },
                    Alpha = newDate ? 1 : 0,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding{ Top = 20, },
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = build.UpdateStream.DisplayName,
                            TextSize = 20, // web: 18,
                            Font = @"Exo2.0-Medium",
                        },
                        new SpriteText
                        {
                            Text = build.DisplayVersion,
                            TextSize = 20, // web: 18,
                            Font = @"Exo2.0-Light",
                            Colour = StreamColour.FromStreamName(build.UpdateStream.Name),
                        },
                        new ClickableText
                        {
                            Text = " ok ",
                            TextSize = 20,
                            Action = BuildRequested,
                        },
                    }
                },
                ChangelogEntries = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
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
            // sort entries by category
            foreach (ChangelogEntry entry in changelogEntries)
            {
                if (!categories.ContainsKey(entry.Category))
                    categories.Add(entry.Category, new List<ChangelogEntry> { entry });
                else
                    categories[entry.Category].Add(entry);
            }

            foreach (KeyValuePair<string, List<ChangelogEntry>> category in categories)
            {
                ChangelogEntries.Add(new SpriteText
                {
                    Text = category.Key,
                    TextSize = 24, // web: 18,
                    Font = @"Exo2.0-Bold",
                    Margin = new MarginPadding { Top = 35, Bottom = 15, },
                });
                foreach (ChangelogEntry entry in category.Value)
                {
                    OsuTextFlowContainer title;

                    ChangelogEntries.Add(title = new OsuTextFlowContainer
                    {
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        LineSpacing = 0.25f,
                    });
                    title.AddIcon(FontAwesome.fa_check, t => { t.TextSize = 12; t.Padding = new MarginPadding { Left = -17, Right = 5 }; });
                    title.AddText(entry.Title, t => { t.TextSize = 18; }); //t.Padding = new MarginPadding(10); });
                    if (!string.IsNullOrEmpty(entry.Repository))
                    {
                        title.AddText($" ({entry.Repository.Substring(4)}#{entry.GithubPullRequestId})", t =>
                        {
                            t.TextSize = 18;
                            t.Colour = Color4.SkyBlue;
                        });
                    }
                    title.AddText($" by {entry.GithubUser.DisplayName}", t => t.TextSize = 14); //web: 12;
                    ChangelogEntries.Add(new SpriteText
                    {
                        TextSize = 14, // web: 12,
                        Colour = new Color4(235, 184, 254, 255),
                        Text = $"{entry.MessageHtml?.Replace("<p>", "").Replace("</p>", "")}\n",
                        Margin = new MarginPadding { Bottom = 10, },
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                    });
                }
            }
        }
    }
}
