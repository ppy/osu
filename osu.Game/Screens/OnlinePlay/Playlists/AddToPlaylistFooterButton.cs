// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class AddToPlaylistFooterButton : FooterButton
    {
        public AddToPlaylistFooterButton()
            : base(width: 220)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            DarkerColour = colours.Blue3;
            LighterColour = colours.Blue1;

            ButtonContent.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = -10,
                    Font = OsuFont.TorusAlternate.With(size: 17),
                    Text = OnlinePlayStrings.FooterButtonPlaylistAdd,
                    UseFullGlyphHeight = false,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    X = 35,
                    Font = OsuFont.TorusAlternate.With(size: 20),
                    Shadow = false,
                    Text = "+",
                    UseFullGlyphHeight = false,
                },
            };
        }
    }
}
