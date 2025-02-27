// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelGroup : PanelBase
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT;

        private Drawable chevronIcon = null!;
        private OsuSpriteText titleText = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = HEIGHT;

            Icon = chevronIcon = new SpriteIcon
            {
                AlwaysPresent = true,
                Icon = FontAwesome.Solid.ChevronDown,
                Size = new Vector2(12),
                Margin = new MarginPadding { Horizontal = 5f },
                X = 2f,
                Colour = colourProvider.Background3,
            };
            Background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Dark1,
            };
            AccentColour = colourProvider.Highlight1;
            Content.Children = new Drawable[]
            {
                titleText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    X = 10f,
                },
                new CircularContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Size = new Vector2(50f, 14f),
                    Margin = new MarginPadding { Right = 20f },
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.7f),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                            // TODO: requires Carousel/CarouselItem-side implementation
                            Text = "43",
                            UseFullGlyphHeight = false,
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => onExpanded(), true);
        }

        private void onExpanded()
        {
            const float duration = 500;

            chevronIcon.ResizeWidthTo(Expanded.Value ? 12f : 0f, duration, Easing.OutQuint);
            chevronIcon.FadeTo(Expanded.Value ? 1f : 0f, duration, Easing.OutQuint);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            GroupDefinition group = (GroupDefinition)Item.Model;

            titleText.Text = group.Title;
        }
    }
}
