// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.OSD
{
    public abstract class Toast : Container
    {
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected readonly OsuSpriteText ValueText;

        protected Toast(string description, string value, string keybinding)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Width = 240;

            // A toast's height is decided (and transformed) by the containing OnScreenDisplay.
            RelativeSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.7f
                },
                content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuSpriteText
                {
                    Padding = new MarginPadding(10),
                    Name = "Description",
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Black),
                    Spacing = new Vector2(1, 0),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = description.ToUpperInvariant()
                },
                ValueText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 24, weight: FontWeight.Light),
                    Padding = new MarginPadding { Left = 10, Right = 10 },
                    Name = "Value",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = value
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Name = "Shortcut",
                    Margin = new MarginPadding { Bottom = 15 },
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                    Text = string.IsNullOrEmpty(keybinding) ? "NO KEY BOUND" : keybinding.ToUpperInvariant()
                },
            };
        }
    }
}
