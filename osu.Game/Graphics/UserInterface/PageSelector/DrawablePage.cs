// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public class DrawablePage : PageSelectorItem
    {
        private readonly BindableBool selected = new BindableBool();

        public bool Selected
        {
            get => selected.Value;
            set => selected.Value = value;
        }

        public int Page { get; private set; }

        private OsuSpriteText text;

        public DrawablePage(int page)
        {
            Page = page;
            text.Text = page.ToString();

            Background.Alpha = 0;

            Action = () =>
            {
                if (!selected.Value)
                    selected.Value = true;
            };
        }

        protected override Drawable CreateContent() => text = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 12),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Colour = Colours.Lime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            selected.BindValueChanged(onSelectedChanged, true);
        }

        private void onSelectedChanged(ValueChangedEvent<bool> selected)
        {
            Background.FadeTo(selected.NewValue ? 1 : 0, DURATION, Easing.OutQuint);
            text.FadeColour(selected.NewValue ? Colours.GreySeafoamDarker : Colours.Lime, DURATION, Easing.OutQuint);
        }

        protected override void UpdateHoverState()
        {
            if (selected.Value)
                return;

            text.FadeColour(IsHovered ? Colours.Lime.Lighten(20f) : Colours.Lime, DURATION, Easing.OutQuint);
        }
    }
}
