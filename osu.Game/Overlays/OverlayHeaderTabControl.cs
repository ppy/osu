// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class OverlayHeaderTabControl : OverlayTabControl<string>
    {
        protected override TabItem<string> CreateTabItem(string value) => new OverlayHeaderTabItem(value)
        {
            AccentColour = AccentColour
        };

        private class OverlayHeaderTabItem : OverlayTabItem<string>
        {
            private readonly OsuSpriteText text;

            public OverlayHeaderTabItem(string value)
                : base(value)
            {
                Add(text = new OsuSpriteText
                {
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Text = value,
                    Font = OsuFont.GetFont(),
                });
            }

            protected override void OnActivated()
            {
                base.OnActivated();
                text.FadeColour(Color4.White, 120, Easing.InQuad);
                text.Font = text.Font.With(weight: FontWeight.Bold);
            }

            protected override void OnDeactivated()
            {
                base.OnDeactivated();
                text.FadeColour(AccentColour, 120, Easing.InQuad);
                text.Font = text.Font.With(weight: FontWeight.Medium);
            }
        }
    }
}
