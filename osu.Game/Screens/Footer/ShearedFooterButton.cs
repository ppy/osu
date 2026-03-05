// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Footer
{
    /// <summary>
    /// A <see cref="ShearedButton"/> intended to be used on the <see cref="ScreenFooter"/>
    /// Allows displaying an additional icon displayed on the right hand side of the button.
    /// </summary>
    public partial class ShearedFooterButton : ShearedButton
    {
        public new LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        public IconUsage Icon
        {
            get => icon.Icon;
            set
            {
                icon.Icon = value;
                icon.Alpha = icon.Icon.Equals(default) ? 0 : 1;
            }
        }

        public Vector2 IconSize
        {
            get => icon.Size;
            set => icon.Size = value;
        }

        private readonly OsuSpriteText text;
        private readonly SpriteIcon icon;

        public ShearedFooterButton()
        {
            ButtonContent.AutoSizeAxes = Axes.None;
            ButtonContent.RelativeSizeAxes = Axes.Both;
            ButtonContent.Padding = new MarginPadding { Horizontal = 15 };

            ButtonContent.Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = text = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.TorusAlternate.With(size: 17),
                            },
                        },
                        icon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(20),
                            Shadow = true,
                            ShadowOffset = new Vector2(0.8f, 0.8f),
                            Alpha = 0,
                        },
                    },
                }
            };
        }
    }
}
