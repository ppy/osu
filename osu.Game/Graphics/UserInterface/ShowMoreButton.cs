// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Graphics.UserInterface
{
    public class ShowMoreButton : LoadingButton
    {
        private const int duration = 200;

        public string Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        private SpriteText text;
        private Box background;
        private FillFlowContainer textContainer;

        public ShowMoreButton()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            IdleColour = colourProvider.Background2;
            HoverColour = colourProvider.Background1;
        }

        protected override Drawable CreateContent() => new CircularContainer
        {
            Masking = true,
            AutoSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                textContainer = new FillFlowContainer
                {
                    AlwaysPresent = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7),
                    Margin = new MarginPadding
                    {
                        Horizontal = 20,
                        Vertical = 5,
                    },
                    Children = new Drawable[]
                    {
                        new ChevronIcon(),
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                            Text = "show more".ToUpper(),
                        },
                        new ChevronIcon()
                    }
                }
            }
        };

        protected override void OnLoadStarted() => textContainer.FadeOut(duration, Easing.OutQuint);

        protected override void OnLoadFinished() => textContainer.FadeIn(duration, Easing.OutQuint);

        private class ChevronIcon : SpriteIcon
        {
            public ChevronIcon()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(8);
                Icon = FontAwesome.Solid.ChevronDown;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Colour = colourProvider.Foreground1;
            }
        }
    }
}
