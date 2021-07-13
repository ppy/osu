// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public class PageSelectorButton : PageSelectorItem
    {
        private readonly Box fadeBox;
        private SpriteIcon icon;
        private OsuSpriteText name;
        private FillFlowContainer buttonContent;

        public PageSelectorButton(bool rightAligned, string text)
        {
            Add(fadeBox = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black.Opacity(100)
            });

            var alignment = rightAligned ? Anchor.x0 : Anchor.x2;

            buttonContent.ForEach(drawable =>
            {
                drawable.Anchor = Anchor.y1 | alignment;
                drawable.Origin = Anchor.y1 | alignment;
            });

            icon.Icon = alignment == Anchor.x2 ? FontAwesome.Solid.ChevronLeft : FontAwesome.Solid.ChevronRight;

            name.Text = text.ToUpper();
        }

        protected override Drawable CreateContent() => buttonContent = new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(3, 0),
            Children = new Drawable[]
            {
                name = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 12),
                },
                icon = new SpriteIcon
                {
                    Size = new Vector2(8),
                },
            }
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Colour = Colours.GreySeafoamDark;
            name.Colour = icon.Colour = Colours.Lime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Enabled.BindValueChanged(enabled => fadeBox.FadeTo(enabled.NewValue ? 0 : 1, DURATION), true);
        }

        protected override void UpdateHoverState() => Background.FadeColour(IsHovered ? Colours.GreySeafoam : Colours.GreySeafoamDark, DURATION, Easing.OutQuint);
    }
}
