// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public class PageSelectorPrevNextButton : PageSelectorButton
    {
        private readonly string text;

        private Box fadeBox;
        private SpriteIcon icon;
        private OsuSpriteText name;

        private readonly Anchor alignment;

        public PageSelectorPrevNextButton(bool rightAligned, string text)
        {
            this.text = text;
            alignment = rightAligned ? Anchor.x0 : Anchor.x2;
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(3, 0),
                    Children = new Drawable[]
                    {
                        name = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = text.ToUpper(),
                        },
                        icon = new SpriteIcon
                        {
                            Icon = alignment == Anchor.x2 ? FontAwesome.Solid.ChevronLeft : FontAwesome.Solid.ChevronRight,
                            Size = new Vector2(8),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
            }
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Colour = Colours.GreySeaFoamDark;
            name.Colour = icon.Colour = Colours.Lime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CircularContent.Add(fadeBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black.Opacity(100)
            });

            Enabled.BindValueChanged(enabled => fadeBox.FadeTo(enabled.NewValue ? 0 : 1, DURATION), true);
        }

        protected override void UpdateHoverState() => Background.FadeColour(IsHovered ? Colours.GreySeaFoam : Colours.GreySeaFoamDark, DURATION, Easing.OutQuint);
    }
}
