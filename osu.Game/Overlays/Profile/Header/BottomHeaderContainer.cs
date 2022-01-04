// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header
{
    public class BottomHeaderContainer : CompositeDrawable
    {
        public readonly Bindable<APIUser> User = new Bindable<APIUser>();

        private LinkFlowContainer topLinkContainer;
        private LinkFlowContainer bottomLinkContainer;

        private Color4 iconColour;

        [Resolved]
        private IAPIProvider api { get; set; }

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

        private void updateDisplay(APIUser user)
        {
            topLinkContainer.Clear();
            bottomLinkContainer.Clear();

            if (user == null) return;

            if (user.JoinDate.ToUniversalTime().Year < 2008)
                topLinkContainer.AddText("Here since the beginning");
            else
            {
                topLinkContainer.AddText("Joined ");
                topLinkContainer.AddText(new DrawableDate(user.JoinDate, italic: false), embolden);
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
                topLinkContainer.AddText(new DrawableDate(user.LastVisit.Value, italic: false), embolden);

                addSpacer(topLinkContainer);
            }

            if (user.PlayStyles?.Length > 0)
            {
                topLinkContainer.AddText("Plays with ");
                topLinkContainer.AddText(string.Join(", ", user.PlayStyles.Select(style => style.GetDescription())), embolden);

                addSpacer(topLinkContainer);
            }

            topLinkContainer.AddText("Contributed ");
            topLinkContainer.AddLink("forum post".ToQuantity(user.PostCount, "#,##0"), $"{api.WebsiteRootUrl}/users/{user.Id}/posts", creationParameters: embolden);

            addSpacer(topLinkContainer);

            topLinkContainer.AddText("Posted ");
            topLinkContainer.AddLink("comment".ToQuantity(user.CommentsCount, "#,##0"), $"{api.WebsiteRootUrl}/comments?user_id={user.Id}", creationParameters: embolden);

            string websiteWithoutProtocol = user.Website;

            if (!string.IsNullOrEmpty(websiteWithoutProtocol))
            {
                if (Uri.TryCreate(websiteWithoutProtocol, UriKind.Absolute, out var uri))
                {
                    websiteWithoutProtocol = uri.Host + uri.PathAndQuery + uri.Fragment;
                    websiteWithoutProtocol = websiteWithoutProtocol.TrimEnd('/');
                }
            }

            bool anyInfoAdded = false;

            anyInfoAdded |= tryAddInfo(FontAwesome.Solid.MapMarker, user.Location);
            anyInfoAdded |= tryAddInfo(OsuIcon.Heart, user.Interests);
            anyInfoAdded |= tryAddInfo(FontAwesome.Solid.Suitcase, user.Occupation);

            if (anyInfoAdded)
                bottomLinkContainer.NewLine();

            if (!string.IsNullOrEmpty(user.Twitter))
                anyInfoAdded |= tryAddInfo(FontAwesome.Brands.Twitter, "@" + user.Twitter, $@"https://twitter.com/{user.Twitter}");
            anyInfoAdded |= tryAddInfo(FontAwesome.Brands.Discord, user.Discord);
            anyInfoAdded |= tryAddInfo(FontAwesome.Solid.Link, websiteWithoutProtocol, user.Website);

            // If no information was added to the bottomLinkContainer, hide it to avoid unwanted padding
            bottomLinkContainer.Alpha = anyInfoAdded ? 1 : 0;
        }

        private void addSpacer(OsuTextFlowContainer textFlow) => textFlow.AddArbitraryDrawable(new Container { Width = 15 });

        private bool tryAddInfo(IconUsage icon, string content, string link = null)
        {
            if (string.IsNullOrEmpty(content)) return false;

            // newlines could be contained in API returned user content.
            content = content.Replace('\n', ' ');

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
            return true;
        }

        private void embolden(SpriteText text) => text.Font = text.Font.With(weight: FontWeight.Bold);
    }
}
