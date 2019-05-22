// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogBuild : FillFlowContainer
    {
        public Action<APIChangelogBuild> SelectBuild;

        protected readonly APIChangelogBuild Build;

        public readonly FillFlowContainer ChangelogEntries;

        public ChangelogBuild(APIChangelogBuild build)
        {
            Build = build;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Padding = new MarginPadding { Horizontal = 70 };

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

            var categories = new SortedDictionary<string, List<APIChangelogEntry>>();

            // sort entries by category
            foreach (APIChangelogEntry entry in build.ChangelogEntries)
            {
                if (!categories.ContainsKey(entry.Category))
                    categories.Add(entry.Category, new List<APIChangelogEntry> { entry });
                else
                    categories[entry.Category].Add(entry);
            }

            foreach (KeyValuePair<string, List<APIChangelogEntry>> category in categories)
            {
                ChangelogEntries.Add(new OsuSpriteText
                {
                    Text = category.Key,
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 24),
                    Margin = new MarginPadding { Top = 35, Bottom = 15 },
                });

                foreach (APIChangelogEntry entry in category.Value)
                var fontMedium = OsuFont.GetFont(size: 14);
                var fontSmall = OsuFont.GetFont(size: 12);

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
                        t.Font = fontSmall;
                        t.Padding = new MarginPadding { Left = -17, Right = 5 };
                    });

                    title.AddText(entry.Title, t => { t.Font = fontLarge; });

                    if (!string.IsNullOrEmpty(entry.Repository))
                    {
                        title.AddText(" (", t => t.Font = fontLarge);
                        title.AddLink($"{entry.Repository.Replace("ppy/", "")}#{entry.GithubPullRequestId}", entry.GithubUrl, Online.Chat.LinkAction.External,
                            creationParameters: t => { t.Font = fontLarge; });
                        title.AddText(")", t => t.Font = fontLarge);
                    }

                    title.AddText(" by ", t => t.Font = fontMedium);

                    if (entry.GithubUser.UserId != null)
                        title.AddUserLink(new User
                        {
                            Username = entry.GithubUser.OsuUsername,
                            Id = entry.GithubUser.UserId.Value
                        }, t => t.Font = fontMedium);
                    else if (entry.GithubUser.GithubUrl != null)
                        title.AddLink(entry.GithubUser.DisplayName, entry.GithubUser.GithubUrl, Online.Chat.LinkAction.External, null, null, t => t.Font = fontMedium);
                    else
                        title.AddText(entry.GithubUser.DisplayName, t => t.Font = fontSmall);

                    ChangelogEntries.Add(title);

                    if (!string.IsNullOrEmpty(entry.MessageHtml))
                    {
                        TextFlowContainer message = new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        };

                        // todo: use markdown parsing once API returns markdown
                        message.AddText(Regex.Replace(entry.MessageHtml, @"<(.|\n)*?>", string.Empty), t =>
                        {
                            t.Font = fontSmall;
                            t.Colour = new Color4(235, 184, 254, 255);
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
