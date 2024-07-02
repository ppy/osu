// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Footer
{
    public partial class ScreenBackButton : ShearedButton
    {
        public const float BUTTON_WIDTH = 240;

        public ScreenBackButton()
            : base(BUTTON_WIDTH)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ButtonContent.Child = new FillFlowContainer
            {
                X = -10f,
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(20f, 0f),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(17f),
                        Icon = FontAwesome.Solid.ChevronLeft,
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.TorusAlternate.With(size: 17),
                        Text = CommonStrings.Back,
                        UseFullGlyphHeight = false,
                    }
                }
            };

            DarkerColour = Color4Extensions.FromHex("#DE31AE");
            LighterColour = Color4Extensions.FromHex("#FF86DD");
            TextColour = Color4.White;
        }
    }
}
