// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
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
    public class BottomHeaderContainer : Container
    {
        private LinkFlowContainer bottomTopLinkContainer;
        private LinkFlowContainer bottomLinkContainer;
        private Color4 linkBlue, communityUserGrayGreenLighter;

        private User user;
        public User User
        {
            get => user;
            set
            {
                if (user == value) return;
                user = value;
                updateDisplay();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.CommunityUserGrayGreenDarker,
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
                        bottomTopLinkContainer = new LinkFlowContainer(text => text.TextSize = 12)
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        bottomLinkContainer = new LinkFlowContainer(text => text.TextSize = 12)
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
                    }
                }
            };

            linkBlue = colours.BlueLight;
            communityUserGrayGreenLighter = colours.CommunityUserGrayGreenLighter;
        }

        private void updateDisplay()
        {
            void bold(SpriteText t) => t.Font = @"Exo2.0-Bold";
            void addSpacer(OsuTextFlowContainer textFlow) => textFlow.AddArbitraryDrawable(new Container { Width = 15 });

            bottomTopLinkContainer.Clear();
            bottomLinkContainer.Clear();

            if (user == null) return;

            if (user.JoinDate.ToUniversalTime().Year < 2008)
            {
                bottomTopLinkContainer.AddText("Here since the beginning");
            }
            else
            {
                bottomTopLinkContainer.AddText("Joined ");
                bottomTopLinkContainer.AddText(new DrawableDate(user.JoinDate), bold);
            }

            addSpacer(bottomTopLinkContainer);

            if (user.PlayStyles?.Length > 0)
            {
                bottomTopLinkContainer.AddText("Plays with ");
                bottomTopLinkContainer.AddText(string.Join(", ", user.PlayStyles.Select(style => style.GetDescription())), bold);

                addSpacer(bottomTopLinkContainer);
            }

            if (user.LastVisit.HasValue)
            {
                bottomTopLinkContainer.AddText("Last seen ");
                bottomTopLinkContainer.AddText(new DrawableDate(user.LastVisit.Value), bold);

                addSpacer(bottomTopLinkContainer);
            }

            bottomTopLinkContainer.AddText("Contributed ");
            bottomTopLinkContainer.AddLink($@"{user.PostCount:#,##0} forum posts", $"https://osu.ppy.sh/users/{user.Id}/posts", creationParameters: bold);

            void tryAddInfo(FontAwesome icon, string content, string link = null)
            {
                if (string.IsNullOrEmpty(content)) return;

                bottomLinkContainer.AddIcon(icon, text =>
                {
                    text.TextSize = 10;
                    text.Colour = communityUserGrayGreenLighter;
                });
                if (link != null)
                {
                    bottomLinkContainer.AddLink(" " + content, link, creationParameters: text =>
                    {
                        bold(text);
                        text.Colour = linkBlue;
                    });
                }
                else
                {
                    bottomLinkContainer.AddText(" " + content, bold);
                }
                addSpacer(bottomLinkContainer);
            }

            string websiteWithoutProtcol = user.Website;
            if (!string.IsNullOrEmpty(websiteWithoutProtcol))
            {
                int protocolIndex = websiteWithoutProtcol.IndexOf("//", StringComparison.Ordinal);
                if (protocolIndex >= 0)
                    websiteWithoutProtcol = websiteWithoutProtcol.Substring(protocolIndex + 2);
            }

            tryAddInfo(FontAwesome.fa_map_marker, user.Location);
            tryAddInfo(FontAwesome.fa_heart_o, user.Interests);
            tryAddInfo(FontAwesome.fa_suitcase, user.Occupation);
            bottomLinkContainer.NewLine();
            if (!string.IsNullOrEmpty(user.Twitter))
                tryAddInfo(FontAwesome.fa_twitter, "@" + user.Twitter, $@"https://twitter.com/{user.Twitter}");
            tryAddInfo(FontAwesome.fa_gamepad, user.Discord); //todo: update fontawesome to include discord logo
            tryAddInfo(FontAwesome.fa_skype, user.Skype, @"skype:" + user.Skype + @"?chat");
            tryAddInfo(FontAwesome.fa_lastfm, user.Lastfm, $@"https://last.fm/users/{user.Lastfm}");
            tryAddInfo(FontAwesome.fa_link, websiteWithoutProtcol, user.Website);
        }
    }
}
