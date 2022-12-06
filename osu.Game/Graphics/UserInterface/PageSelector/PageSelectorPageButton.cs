// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface.PageSelector
{
    public partial class PageSelectorPageButton : PageSelectorButton
    {
        private readonly BindableBool selected = new BindableBool();

        public bool Selected
        {
            set => selected.Value = value;
        }

        public int PageNumber { get; }

        private OsuSpriteText text;

        public PageSelectorPageButton(int pageNumber)
        {
            PageNumber = pageNumber;

            Action = () =>
            {
                if (!selected.Value)
                    selected.Value = true;
            };
        }

        protected override Drawable CreateContent() => text = new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
            Text = PageNumber.ToString(),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Colour = ColourProvider.Highlight1;
            Background.Alpha = 0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            selected.BindValueChanged(onSelectedChanged, true);
        }

        private void onSelectedChanged(ValueChangedEvent<bool> selected)
        {
            Background.FadeTo(selected.NewValue ? 1 : 0, DURATION, Easing.OutQuint);

            text.FadeColour(selected.NewValue ? ColourProvider.Dark4 : ColourProvider.Light3, DURATION, Easing.OutQuint);
            text.Font = text.Font.With(weight: IsHovered ? FontWeight.SemiBold : FontWeight.Regular);
        }

        protected override void UpdateHoverState()
        {
            if (selected.Value)
                return;

            text.FadeColour(IsHovered ? ColourProvider.Light2 : ColourProvider.Light1, DURATION, Easing.OutQuint);
        }
    }
}
