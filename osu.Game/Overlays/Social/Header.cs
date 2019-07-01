// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.SearchableList;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using System.ComponentModel;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Social
{
    public class Header : SearchableListHeader<SocialTab>
    {
        private OsuSpriteText browser;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"38202e");

        protected override SocialTab DefaultTab => SocialTab.AllPlayers;
        protected override IconUsage Icon => FontAwesome.Solid.Users;

        protected override Drawable CreateHeaderText()
        {
            return new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new[]
                {
                    new OsuSpriteText
                    {
                        Text = "social ",
                        Font = OsuFont.GetFont(size: 25),
                    },
                    browser = new OsuSpriteText
                    {
                        Text = "browser",
                        Font = OsuFont.GetFont(size: 25, weight: FontWeight.Light),
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            browser.Colour = colours.Pink;
        }
    }

    public enum SocialTab
    {
        [Description("All Players")]
        AllPlayers,

        [Description("Friends")]
        Friends,
        //[Description("Team Members")]
        //TeamMembers,
        //[Description("Chat Channels")]
        //ChatChannels,
    }
}
