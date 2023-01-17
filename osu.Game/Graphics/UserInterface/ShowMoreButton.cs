// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using System.Collections.Generic;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShowMoreButton : LoadingButton
    {
        private const int duration = 200;

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        private ChevronIcon leftIcon;
        private ChevronIcon rightIcon;
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
                    Spacing = new Vector2(10),
                    Margin = new MarginPadding
                    {
                        Horizontal = 20,
                        Vertical = 5
                    },
                    Children = new Drawable[]
                    {
                        leftIcon = new ChevronIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold),
                            Text = CommonStrings.ButtonsShowMore.ToUpper(),
                        },
                        rightIcon = new ChevronIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            }
        };

        protected override void OnLoadStarted() => textContainer.FadeOut(duration, Easing.OutQuint);

        protected override void OnLoadFinished() => textContainer.FadeIn(duration, Easing.OutQuint);

        protected override bool OnHover(HoverEvent e)
        {
            base.OnHover(e);
            leftIcon.SetHoveredState(true);
            rightIcon.SetHoveredState(true);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            leftIcon.SetHoveredState(false);
            rightIcon.SetHoveredState(false);
        }

        public partial class ChevronIcon : SpriteIcon
        {
            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            public ChevronIcon()
            {
                Size = new Vector2(7.5f);
                Icon = FontAwesome.Solid.ChevronDown;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Colour = colourProvider.Foreground1;
            }

            public void SetHoveredState(bool hovered) =>
                this.FadeColour(hovered ? colourProvider.Light1 : colourProvider.Foreground1, 200, Easing.OutQuint);
        }
    }
}
