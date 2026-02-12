// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Screens.Footer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class PlayFooterButton : FooterButton
    {
        public PlayFooterButton()
            : base(220)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            DarkerColour = colours.Green3;
            LighterColour = colours.Green1;

            ButtonContent.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = -10,
                    Font = OsuFont.TorusAlternate.With(size: 17),
                    Text = DailyChallengeStrings.Play,
                    UseFullGlyphHeight = false,
                },
                new SpriteIcon
                {
                    Icon = OsuIcon.Play,
                    Size = new Vector2(24),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    X = 70,
                },
            };
        }
    }
}
