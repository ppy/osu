// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Overlays.OSD
{
    public abstract partial class Toast : Container
    {
        private const int toast_minimum_width = 240;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        protected readonly OsuSpriteText ValueText;

        protected readonly OsuSpriteText ShortcutText;

        protected Toast(LocalisableString description, LocalisableString value, LocalisableString shortcut)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            // A toast's height is decided (and transformed) by the containing OnScreenDisplay.
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                new Container // this container exists just to set a minimum width for the toast
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = toast_minimum_width
                },
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
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold),
                    Spacing = new Vector2(1, 0),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = description.ToUpper()
                },
                ValueText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 24, weight: FontWeight.Light),
                    Padding = new MarginPadding { Horizontal = 10 },
                    Name = "Value",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = value
                },
                ShortcutText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Name = "Shortcut",
                    Alpha = 0.3f,
                    Margin = new MarginPadding { Bottom = 15, Horizontal = 10 },
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                    Text = string.IsNullOrEmpty(shortcut.ToString()) ? ToastStrings.NoKeyBound.ToUpper() : shortcut.ToUpper()
                },
            };
        }
    }
}
