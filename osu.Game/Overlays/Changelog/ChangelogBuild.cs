// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK.Graphics;
using osu.Framework.Allocation;
using System.Net;
using osuTK;
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogBuild : FillFlowContainer
    {
        public const float HORIZONTAL_PADDING = 70;

        public Action<APIChangelogBuild> SelectBuild;

        protected readonly APIChangelogBuild Build;

        public readonly FillFlowContainer ChangelogEntries;

        public ChangelogBuild(APIChangelogBuild build)
        {
            Build = build;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Horizontal = HORIZONTAL_PADDING };

            Children = new Drawable[]
            {
                CreateHeader(),
                ChangelogEntries = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            foreach (var categoryEntries in Build.ChangelogEntries.GroupBy(b => b.Category).OrderBy(c => c.Key))
            {
                ChangelogEntries.Add(new OsuSpriteText
                {
                    Text = categoryEntries.Key,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 18),
                    Margin = new MarginPadding { Top = 35, Bottom = 15 },
                });

                var fontLarge = OsuFont.GetFont(size: 16);
                var fontMedium = OsuFont.GetFont(size: 12);

                foreach (var entry in categoryEntries)
                {
                    var entryColour = entry.Major ? colours.YellowLight : Color4.White;

                    LinkFlowContainer title;

                    var titleContainer = new Container
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Margin = new MarginPadding { Vertical = 5 },
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreRight,
                                Size = new Vector2(10),
                                Icon = entry.Type == ChangelogEntryType.Fix ? FontAwesome.Solid.Check : FontAwesome.Solid.Plus,
                                Colour = entryColour.Opacity(0.5f),
                                Margin = new MarginPadding { Right = 5 },
                            },
                            title = new LinkFlowContainer
                            {
                                Direction = FillDirection.Full,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                TextAnchor = Anchor.BottomLeft,
                            }
                        }
                    };

                    title.AddText(entry.Title, t =>
                    {
                        t.Font = fontLarge;
                        t.Colour = entryColour;
                    });

                    if (!string.IsNullOrEmpty(entry.Repository))
                    {
                        title.AddText(" (", t =>
                        {
                            t.Font = fontLarge;
                            t.Colour = entryColour;
                        });
                        title.AddLink($"{entry.Repository.Replace("ppy/", "")}#{entry.GithubPullRequestId}", entry.GithubUrl,
                            creationParameters: t =>
                            {
                                t.Font = fontLarge;
                                t.Colour = entryColour;
                            });
                        title.AddText(")", t =>
                        {
                            t.Font = fontLarge;
                            t.Colour = entryColour;
                        });
                    }

                    title.AddText("by ", t =>
                    {
                        t.Font = fontMedium;
                        t.Colour = entryColour;
                        t.Padding = new MarginPadding { Left = 10 };
                    });

                    if (entry.GithubUser != null)
                    {
                        if (entry.GithubUser.UserId != null)
                        {
                            title.AddUserLink(new User
                            {
                                Username = entry.GithubUser.OsuUsername,
                                Id = entry.GithubUser.UserId.Value
                            }, t =>
                            {
                                t.Font = fontMedium;
                                t.Colour = entryColour;
                            });
                        }
                        else if (entry.GithubUser.GithubUrl != null)
                        {
                            title.AddLink(entry.GithubUser.DisplayName, entry.GithubUser.GithubUrl, t =>
                            {
                                t.Font = fontMedium;
                                t.Colour = entryColour;
                            });
                        }
                        else
                        {
                            title.AddText(entry.GithubUser.DisplayName, t =>
                            {
                                t.Font = fontMedium;
                                t.Colour = entryColour;
                            });
                        }
                    }

                    ChangelogEntries.Add(titleContainer);

                    if (!string.IsNullOrEmpty(entry.MessageHtml))
                    {
                        var message = new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        };

                        // todo: use markdown parsing once API returns markdown
                        message.AddText(WebUtility.HtmlDecode(Regex.Replace(entry.MessageHtml, @"<(.|\n)*?>", string.Empty)), t =>
                        {
                            t.Font = fontMedium;
                            t.Colour = colourProvider.Foreground1;
                        });

                        ChangelogEntries.Add(message);
                    }
                }
            }
        }

        protected virtual FillFlowContainer CreateHeader() => new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Margin = new MarginPadding { Top = 20 },
            Children = new Drawable[]
            {
                new OsuHoverContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Action = () => SelectBuild?.Invoke(Build),
                    Child = new FillFlowContainer<SpriteText>
                    {
                        AutoSizeAxes = Axes.Both,
                        Margin = new MarginPadding { Horizontal = 40 },
                        Children = new[]
                        {
                            new OsuSpriteText
                            {
                                Text = Build.UpdateStream.DisplayName,
                                Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 19),
                            },
                            new OsuSpriteText
                            {
                                Text = " ",
                                Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 19),
                            },
                            new OsuSpriteText
                            {
                                Text = Build.DisplayVersion,
                                Font = OsuFont.GetFont(weight: FontWeight.Light, size: 19),
                                Colour = Build.UpdateStream.Colour,
                            },
                        }
                    }
                },
            }
        };
    }
}
