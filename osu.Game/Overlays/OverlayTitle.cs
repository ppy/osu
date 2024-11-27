// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays
{
    public abstract partial class OverlayTitle : CompositeDrawable, INamedOverlayComponent
    {
        public const float ICON_SIZE = 30;

        private readonly OsuSpriteText titleText;
        private readonly Container iconContainer;

        private LocalisableString title;

        public LocalisableString Title
        {
            get => title;
            protected set => titleText.Text = title = value;
        }

        public LocalisableString Description { get; protected set; }

        private IconUsage icon;

        public IconUsage Icon
        {
            get => icon;
            protected set => iconContainer.Child = new SpriteIcon
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,

                Icon = icon = value,
            };
        }

        protected OverlayTitle()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10, 0),
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    iconContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = 5 }, // compensates for osu-web sprites having around 5px of whitespace on each side
                        Size = new Vector2(ICON_SIZE)
                    },
                    titleText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular),
                        Margin = new MarginPadding { Vertical = 17.5f } // 15px padding + 2.5px line-height difference compensation
                    }
                }
            };
        }
    }
}
