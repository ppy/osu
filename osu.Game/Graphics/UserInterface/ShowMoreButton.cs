// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Graphics.UserInterface
{
    public class ShowMoreButton : OsuHoverContainer
    {
        private const float fade_duration = 200;

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

        private bool isLoading;

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;

                Enabled.Value = !isLoading;

                if (value)
                {
                    loading.Show();
                    content.FadeOut(fade_duration, Easing.OutQuint);
                }
                else
                {
                    loading.Hide();
                    content.FadeIn(fade_duration, Easing.OutQuint);
                }
            }
        }

        private readonly Box background;
        private readonly LoadingAnimation loading;
        private readonly FillFlowContainer content;
        private readonly ChevronIcon leftChevron;
        private readonly ChevronIcon rightChevron;
        private readonly SpriteText text;

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        public ShowMoreButton()
        {
            AutoSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new CircularContainer
                {
                    Masking = true,
                    Size = new Vector2(140, 30),
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        content = new FillFlowContainer
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
                        },
                        loading = new LoadingAnimation
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(12)
                        },
                    }
                }
            };
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Enabled.Value)
                return false;

            try
            {
                return base.OnClick(e);
            }
            finally
            {
                // run afterwards as this will disable this button.
                IsLoading = true;
            }
        }

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
