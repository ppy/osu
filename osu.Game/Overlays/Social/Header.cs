// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays.SearchableList;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using System.ComponentModel;

namespace osu.Game.Overlays.Social
{
    public class Header : SearchableListHeader<SocialTab>
    {
        private OsuSpriteText browser;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"38202e");
        protected override float TabStripWidth => 438;

        protected override SocialTab DefaultTab => SocialTab.AllPlayers;
        protected override FontAwesome Icon => FontAwesome.fa_users;

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
                        TextSize = 25,
                    },
                    browser = new OsuSpriteText
                    {
                        Text = "browser",
                        TextSize = 25,
                        Font = @"Exo2.0-Light",
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
