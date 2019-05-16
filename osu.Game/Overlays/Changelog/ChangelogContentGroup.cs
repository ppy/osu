// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogContentGroup : FillFlowContainer
    {
        private readonly TooltipIconButton chevronPrevious, chevronNext;

        private readonly SortedDictionary<string, List<APIChangelogEntry>> categories =
            new SortedDictionary<string, List<APIChangelogEntry>>();

        public event Action<APIChangelogBuild> BuildSelected;

        public readonly FillFlowContainer ChangelogEntries;

        public ChangelogContentGroup(APIChangelogBuild build)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Horizontal = 70 };
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Top = 20 },
                    Children = new Drawable[]
                    {
                        chevronPrevious = new TooltipIconButton
                        {
                            IsEnabled = false,
                            Icon = FontAwesome.Solid.ChevronLeft,
                            Size = new Vector2(24),
                            Action = () =>
                            {
                                BuildSelected?.Invoke(build.Versions.Previous);
                                chevronPrevious.IsEnabled = false;
                            },
                        },
                        new FillFlowContainer<SpriteText>
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Horizontal = 40 },
                            Children = new[]
                            {
                                new SpriteText
                                {
                                    Text = build.UpdateStream.DisplayName,
                                    Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 24),
                                },
                                new SpriteText
                                {
                                    Text = " ",
                                    Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 24),
                                },
                                new SpriteText
                                {
                                    Text = build.DisplayVersion,
                                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 24),
                                    Colour = StreamColour.FromStreamName(build.UpdateStream.Name),
                                },
                            }
                        },
                        chevronNext = new TooltipIconButton
                        {
                            IsEnabled = false,
                            Icon = FontAwesome.Solid.ChevronRight,
                            Size = new Vector2(24),
                            Action = () =>
                            {
                                BuildSelected?.Invoke(build.Versions.Next);
                                chevronNext.IsEnabled = false;
                            },
                        },
                    }
                },
                new SpriteText
                {
                    // do we need .ToUniversalTime() here?
                    // also, this should be a temporary solution to weekdays in >localized< date strings
                    Text = build.CreatedAt.Date.ToLongDateString().Replace(build.CreatedAt.ToString("dddd") + ", ", ""),
                    Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 14),
                    Colour = OsuColour.FromHex(@"FD5"),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = 5 },
                },
                ChangelogEntries = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                },
            };
        }

        public ChangelogContentGroup(APIChangelogBuild build, bool newDate)
        {
            OsuHoverContainer clickableBuildText;
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
                    Font = OsuFont.GetFont(weight: FontWeight.Light, size: 24),
                    Colour = OsuColour.FromHex(@"FD5"),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = 20 },
                    Alpha = newDate ? 1 : 0,
                },
                clickableBuildText = new OsuHoverContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Top = 20 },
                    Action = () => BuildSelected?.Invoke(build),
                    Child = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(5),
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Text = build.UpdateStream.DisplayName,
                                Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 19),
                            },
                            new SpriteText
                            {
                                Text = build.DisplayVersion,
                                Font = OsuFont.GetFont(weight: FontWeight.Light, size: 19),
                                Colour = StreamColour.FromStreamName(build.UpdateStream.Name),
                            },
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

            // we may not want double clicks,
            // can be clicked again only after a delay
            clickableBuildText.Action += () =>
            {
                clickableBuildText.Action = null;
                clickableBuildText.FadeTo(0.5f, 500);
                Scheduler.AddDelayed(() =>
                {
                    clickableBuildText.Action = () => BuildSelected?.Invoke(build);
                    clickableBuildText.FadeIn(500);
                }, 2000);
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

        public void GenerateText(List<APIChangelogEntry> changelogEntries)
        {
            // sort entries by category
            foreach (APIChangelogEntry entry in changelogEntries)
            {
                if (!categories.ContainsKey(entry.Category))
                    categories.Add(entry.Category, new List<APIChangelogEntry> { entry });
                else
                    categories[entry.Category].Add(entry);
            }

            foreach (KeyValuePair<string, List<APIChangelogEntry>> category in categories)
            {
                ChangelogEntries.Add(new SpriteText
                {
                    Text = category.Key,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 24),
                    Margin = new MarginPadding { Top = 35, Bottom = 15 },
                });

                foreach (APIChangelogEntry entry in category.Value)
                {
                    LinkFlowContainer title = new LinkFlowContainer
                    {
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Vertical = 5 },
                    };

                    title.AddIcon(FontAwesome.Solid.Check, t =>
                    {
                        t.Font = OsuFont.GetFont(size: 12);
                        t.Padding = new MarginPadding { Left = -17, Right = 5 };
                    });

                    title.AddText(entry.Title, t => { t.Font = OsuFont.GetFont(size: 18); });

                    if (!string.IsNullOrEmpty(entry.Repository))
                    {
                        title.AddText(" (", t => t.Font = OsuFont.GetFont(size: 18));
                        title.AddLink($"{entry.Repository.Replace("ppy/", "")}#{entry.GithubPullRequestId}",
                            entry.GithubUrl, Online.Chat.LinkAction.External, null,
                            null, t => { t.Font = OsuFont.GetFont(size: 18); });
                        title.AddText(")", t => t.Font = OsuFont.GetFont(size: 18));
                    }

                    title.AddText(" by ", t => t.Font = OsuFont.GetFont(size: 14));

                    if (entry.GithubUser.GithubUrl != null)
                        title.AddLink(entry.GithubUser.DisplayName, entry.GithubUser.GithubUrl,
                            Online.Chat.LinkAction.External, null, null,
                            t => t.Font = OsuFont.GetFont(size: 14));
                    else
                        title.AddText(entry.GithubUser.DisplayName, t => t.Font = OsuFont.GetFont(size: 12));

                    ChangelogEntries.Add(title);

                    if (!string.IsNullOrEmpty(entry.MessageHtml))
                    {
                        TextFlowContainer messageContainer = new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        };

                        // todo: use markdown parsing once API returns markdown
                        messageContainer.AddText(Regex.Replace(entry.MessageHtml, @"<(.|\n)*?>", string.Empty), t =>
                        {
                            t.Font = OsuFont.GetFont(size: 12);
                            t.Colour = new Color4(235, 184, 254, 255);
                        });

                        ChangelogEntries.Add(messageContainer);
                    }
                }
            }
        }
    }
}
