// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogEntry : FillFlowContainer
    {
        private readonly APIChangelogEntry entry;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private FontUsage fontLarge;
        private FontUsage fontMedium;

        public ChangelogEntry(APIChangelogEntry entry)
        {
            this.entry = entry;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            fontLarge = OsuFont.GetFont(size: 16);
            fontMedium = OsuFont.GetFont(size: 12);

            Children = new[]
            {
                createTitle(),
                createMessage()
            };
        }

        private Drawable createTitle()
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
                        Icon = getIconForChangelogEntry(entry.Type),
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
                addRepositoryReference(title, entryColour);

            if (entry.GithubUser != null)
                addGithubAuthorReference(title, entryColour);

            return titleContainer;
        }

        private void addRepositoryReference(LinkFlowContainer title, Color4 entryColour)
        {
            title.AddText(" (", t =>
            {
                t.Font = fontLarge;
                t.Colour = entryColour;
            });
            title.AddLink($"{entry.Repository.Replace("ppy/", "")}#{entry.GithubPullRequestId}", entry.GithubUrl,
                t =>
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

        private void addGithubAuthorReference(LinkFlowContainer title, Color4 entryColour)
        {
            title.AddText("by ", t =>
            {
                t.Font = fontMedium;
                t.Colour = entryColour;
                t.Padding = new MarginPadding { Left = 10 };
            });

            if (entry.GithubUser.UserId != null)
            {
                title.AddUserLink(new APIUser
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

        private Drawable createMessage()
        {
            if (string.IsNullOrEmpty(entry.MessageHtml))
                return Empty();

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

            return message;
        }

        private static IconUsage getIconForChangelogEntry(ChangelogEntryType entryType)
        {
            // compare: https://github.com/ppy/osu-web/blob/master/resources/assets/coffee/react/_components/changelog-entry.coffee#L8-L11
            switch (entryType)
            {
                case ChangelogEntryType.Add:
                    return FontAwesome.Solid.Plus;

                case ChangelogEntryType.Fix:
                    return FontAwesome.Solid.Check;

                case ChangelogEntryType.Misc:
                    return FontAwesome.Regular.Circle;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entryType), $"Unrecognised entry type {entryType}");
            }
        }
    }
}
