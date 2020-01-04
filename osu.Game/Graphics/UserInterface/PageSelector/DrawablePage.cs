// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Framework.Input.Events;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public class DrawablePage : OsuClickableContainer
    {
        private const int duration = 200;

        private readonly BindableBool selected = new BindableBool();

        public bool Selected
        {
            get => selected.Value;
            set => selected.Value = value;
        }

        [Resolved]
        private OsuColour colours { get; set; }

        private readonly Box background;
        private readonly OsuSpriteText text;

        public DrawablePage(int page)
        {
            AutoSizeAxes = Axes.X;
            Height = PageSelector.HEIGHT;
            Child = new CircularContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = page.ToString(),
                        Font = OsuFont.GetFont(size: 12),
                        Margin = new MarginPadding { Horizontal = 10 }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Lime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            selected.BindValueChanged(onSelectedChanged, true);
        }

        private void onSelectedChanged(ValueChangedEvent<bool> selected)
        {
            background.FadeTo(selected.NewValue ? 1 : 0, duration, Easing.OutQuint);
            text.FadeColour(selected.NewValue ? colours.GreySeafoamDarker : colours.Lime, duration, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!selected.Value)
                selected.Value = true;

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateHoverState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateHoverState();
        }

        private void updateHoverState()
        {
            if (selected.Value)
                return;

            text.FadeColour(IsHovered ? colours.Lime.Lighten(20f) : colours.Lime, duration, Easing.OutQuint);
        }
    }
}
