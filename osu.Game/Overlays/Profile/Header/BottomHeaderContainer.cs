// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class BottomHeaderContainer : CompositeDrawable
    {
        public readonly Bindable<User> User = new Bindable<User>();

        private LinkFlowContainer topLinkContainer;
        private LinkFlowContainer bottomLinkContainer;

        private Color4 iconColour;

        public BottomHeaderContainer()
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            iconColour = colours.GreySeafoamLighter;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.GreySeafoamDark,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                    Spacing = new Vector2(0, 10),
                    Children = new Drawable[]
                    {
                        topLinkContainer = new LinkFlowContainer(text => text.Font = text.Font.With(size: 12))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        bottomLinkContainer = new LinkFlowContainer(text => text.Font = text.Font.With(size: 12))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                }
            };

            User.BindValueChanged(user => updateDisplay(user.NewValue));
        }

        private void updateDisplay(User user)
        {
            topLinkContainer.Clear();
            bottomLinkContainer.Clear();

            if (user == null) return;

            if (user.JoinDate.ToUniversalTime().Year < 2008)
                topLinkContainer.AddText("Here since the beginning");
            else
            {
                topLinkContainer.AddText("Joined ");
                topLinkContainer.AddText(new DrawableDate(user.JoinDate), embolden);
            }

            addSpacer(topLinkContainer);

            if (user.IsOnline)
            {
                topLinkContainer.AddText("Currently online");
                addSpacer(topLinkContainer);
            }
            else if (user.LastVisit.HasValue)
            {
                topLinkContainer.AddText("Last seen ");
                topLinkContainer.AddText(new DrawableDate(user.LastVisit.Value), embolden);

                addSpacer(topLinkContainer);
            }

            if (user.PlayStyles?.Length > 0)
            {
                topLinkContainer.AddText("Plays with ");
                topLinkContainer.AddText(string.Join(", ", user.PlayStyles.Select(style => style.GetDescription())), embolden);

                addSpacer(topLinkContainer);
            }

            topLinkContainer.AddText("Contributed ");
            topLinkContainer.AddLink($@"{user.PostCount:#,##0} forum posts", $"https://osu.ppy.sh/users/{user.Id}/posts", creationParameters: embolden);

            string websiteWithoutProtcol = user.Website;

            if (!string.IsNullOrEmpty(websiteWithoutProtcol))
            {
                if (Uri.TryCreate(websiteWithoutProtcol, UriKind.Absolute, out var uri))
                {
                    websiteWithoutProtcol = uri.Host + uri.PathAndQuery + uri.Fragment;
                    websiteWithoutProtcol = websiteWithoutProtcol.TrimEnd('/');
                }
            }

            tryAddInfo(FontAwesome.Solid.MapMarker, user.Location);
            tryAddInfo(OsuIcon.Heart, user.Interests);
            tryAddInfo(FontAwesome.Solid.Suitcase, user.Occupation);
            bottomLinkContainer.NewLine();
            if (!string.IsNullOrEmpty(user.Twitter))
                tryAddInfo(FontAwesome.Brands.Twitter, "@" + user.Twitter, $@"https://twitter.com/{user.Twitter}");
            tryAddInfo(FontAwesome.Brands.Discord, user.Discord);
            tryAddInfo(FontAwesome.Brands.Skype, user.Skype, @"skype:" + user.Skype + @"?chat");
            tryAddInfo(FontAwesome.Brands.Lastfm, user.Lastfm, $@"https://last.fm/users/{user.Lastfm}");
            tryAddInfo(FontAwesome.Solid.Link, websiteWithoutProtcol, user.Website);
        }

        private void addSpacer(OsuTextFlowContainer textFlow) => textFlow.AddArbitraryDrawable(new Container { Width = 15 });

        private void tryAddInfo(IconUsage icon, string content, string link = null)
        {
            if (string.IsNullOrEmpty(content)) return;

            // newlines could be contained in API returned user content.
            content = content.Replace("\n", " ");

            bottomLinkContainer.AddIcon(icon, text =>
            {
                text.Font = text.Font.With(size: 10);
                text.Colour = iconColour;
            });

            if (link != null)
                bottomLinkContainer.AddLink(" " + content, link, creationParameters: embolden);
            else
                bottomLinkContainer.AddText(" " + content, embolden);

            addSpacer(bottomLinkContainer);
        }

        private void embolden(SpriteText text) => text.Font = text.Font.With(weight: FontWeight.Bold);
    }
}
