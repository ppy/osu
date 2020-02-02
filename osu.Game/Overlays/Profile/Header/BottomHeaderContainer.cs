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

        private LinkFlowContainer linkContainer;

        private Color4 iconColour;

        public BottomHeaderContainer()
        {
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            iconColour = colourProvider.Foreground1;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                linkContainer = new LinkFlowContainer(text => text.Font = text.Font.With(size: 12))
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = UserProfileOverlay.CONTENT_X_MARGIN, Vertical = 10 },
                    Spacing = new Vector2(0, 10),
                }
            };

            User.BindValueChanged(user => updateDisplay(user.NewValue));
        }

        private void updateDisplay(User user)
        {
            linkContainer.Clear();

            if (user == null) return;

            if (user.JoinDate.ToUniversalTime().Year < 2008)
                linkContainer.AddText("Here since the beginning");
            else
            {
                linkContainer.AddText("Joined ");
                linkContainer.AddText(new DrawableDate(user.JoinDate), embolden);
            }

            addSpacer(linkContainer);

            if (user.IsOnline)
            {
                linkContainer.AddText("Currently online");
                addSpacer(linkContainer);
            }
            else if (user.LastVisit.HasValue)
            {
                linkContainer.AddText("Last seen ");
                linkContainer.AddText(new DrawableDate(user.LastVisit.Value), embolden);

                addSpacer(linkContainer);
            }

            if (user.PlayStyles?.Length > 0)
            {
                linkContainer.AddText("Plays with ");
                linkContainer.AddText(string.Join(", ", user.PlayStyles.Select(style => style.GetDescription())), embolden);

                addSpacer(linkContainer);
            }

            linkContainer.AddText("Contributed ");
            linkContainer.AddLink($@"{user.PostCount:#,##0} forum posts", $"https://osu.ppy.sh/users/{user.Id}/posts", creationParameters: embolden);

            string websiteWithoutProtocol = user.Website;

            if (!string.IsNullOrEmpty(websiteWithoutProtocol))
            {
                if (Uri.TryCreate(websiteWithoutProtocol, UriKind.Absolute, out var uri))
                {
                    websiteWithoutProtocol = uri.Host + uri.PathAndQuery + uri.Fragment;
                    websiteWithoutProtocol = websiteWithoutProtocol.TrimEnd('/');
                }
            }

            requireNewLineOnAddInfo = true;

            tryAddInfo(FontAwesome.Solid.MapMarker, user.Location);
            tryAddInfo(OsuIcon.Heart, user.Interests);
            tryAddInfo(FontAwesome.Solid.Suitcase, user.Occupation);

            requireNewLineOnAddInfo = true;

            if (!string.IsNullOrEmpty(user.Twitter))
                tryAddInfo(FontAwesome.Brands.Twitter, "@" + user.Twitter, $@"https://twitter.com/{user.Twitter}");
            tryAddInfo(FontAwesome.Brands.Discord, user.Discord);
            tryAddInfo(FontAwesome.Brands.Skype, user.Skype, @"skype:" + user.Skype + @"?chat");
            tryAddInfo(FontAwesome.Brands.Lastfm, user.Lastfm, $@"https://last.fm/users/{user.Lastfm}");
            tryAddInfo(FontAwesome.Solid.Link, websiteWithoutProtocol, user.Website);
        }

        private void addSpacer(OsuTextFlowContainer textFlow) => textFlow.AddArbitraryDrawable(new Container { Width = 15 });

        private bool requireNewLineOnAddInfo;

        private void tryAddInfo(IconUsage icon, string content, string link = null)
        {
            if (string.IsNullOrEmpty(content)) return;

            if (requireNewLineOnAddInfo)
            {
                linkContainer.NewLine();
                requireNewLineOnAddInfo = false;
            }

            // newlines could be contained in API returned user content.
            content = content.Replace("\n", " ");

            linkContainer.AddIcon(icon, text =>
            {
                text.Font = text.Font.With(size: 10);
                text.Colour = iconColour;
            });

            if (link != null)
                linkContainer.AddLink(" " + content, link, creationParameters: embolden);
            else
                linkContainer.AddText(" " + content, embolden);

            addSpacer(linkContainer);
        }

        private void embolden(SpriteText text) => text.Font = text.Font.With(weight: FontWeight.Bold);
    }
}
