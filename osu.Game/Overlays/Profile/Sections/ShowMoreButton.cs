// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections
{
    public class ShowMoreButton : OsuHoverContainer
    {
        private const float fade_duration = 200;

        private readonly Box background;
        private readonly LoadingAnimation loading;
        private readonly FillFlowContainer content;

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        private bool isLoading;

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading == value)
                    return;

                isLoading = value;

                Enabled.Value = !isLoading;

                if (value)
                {
                    loading.FadeIn(fade_duration, Easing.OutQuint);
                    content.FadeOut(fade_duration, Easing.OutQuint);
                }
                else
                {
                    loading.FadeOut(fade_duration, Easing.OutQuint);
                    content.FadeIn(fade_duration, Easing.OutQuint);
                }
            }
        }

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
                                new ChevronIcon(),
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold),
                                    Text = "show more".ToUpper(),
                                },
                                new ChevronIcon(),
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colors)
        {
            IdleColour = colors.GreySeafoamDark;
            HoverColour = colors.GreySeafoam;
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
            private const int bottom_margin = 2;
            private const int icon_size = 8;

            public ChevronIcon()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Margin = new MarginPadding { Bottom = bottom_margin };
                Size = new Vector2(icon_size);
                Icon = FontAwesome.Solid.ChevronDown;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colors)
            {
                Colour = colors.Yellow;
            }
        }
    }
}
