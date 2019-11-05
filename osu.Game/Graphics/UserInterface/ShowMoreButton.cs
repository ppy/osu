// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Graphics.UserInterface
{
    public class ShowMoreButton : LoadingButton
    {
        private const int duration = 200;

        private Color4 chevronIconColour;

        protected Color4 ChevronIconColour
        {
            get => chevronIconColour;
            set => chevronIconColour = leftChevron.Colour = rightChevron.Colour = value;
        }

        public string Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        private ChevronIcon leftChevron;
        private ChevronIcon rightChevron;
        private SpriteText text;
        private Box background;
        private FillFlowContainer textContainer;

        public ShowMoreButton()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override Drawable CreateContent() => new CircularContainer
        {
            Masking = true,
            Size = new Vector2(140, 30),
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                textContainer = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(7),
                    Children = new Drawable[]
                    {
                        leftChevron = new ChevronIcon(),
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                            Text = "show more".ToUpper(),
                        },
                        rightChevron = new ChevronIcon(),
                    }
                }
            }
        };

        protected override void OnLoadStarted() => textContainer.FadeOut(duration, Easing.OutQuint);

        protected override void OnLoadFinished() => textContainer.FadeIn(duration, Easing.OutQuint);

        private class ChevronIcon : SpriteIcon
        {
            private const int icon_size = 8;

            public ChevronIcon()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Size = new Vector2(icon_size);
                Icon = FontAwesome.Solid.ChevronDown;
            }
        }
    }
}
