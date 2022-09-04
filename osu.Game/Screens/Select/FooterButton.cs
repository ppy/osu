// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;

namespace osu.Game.Screens.Select
{
    public abstract class FooterButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        public const float SHEAR_WIDTH = 7.5f;
        private const float corner_radius = 7;

        protected Colour4 BaseColour = Colour4.FromHex("#394642");
        protected Colour4 ColourOnHover = Colour4.FromHex("#394642").Lighten(.1f);

        private static readonly Vector2 shear = new Vector2(SHEAR_WIDTH / Footer.HEIGHT, 0);

        protected LocalisableString Text
        {
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        protected FillFlowContainer ButtonContentContainer;
        protected readonly Container TextContainer;
        private readonly SpriteText spriteText;
        private readonly Box box;

        protected FooterButton()
        {
            Margin = new MarginPadding
            {
                Top = 30
            };
            AutoSizeAxes = Axes.X;
            Height = 100;
            Shear = shear;
            Origin = Anchor.CentreLeft;
            CornerRadius = corner_radius;
            Masking = true;
            Margin = new MarginPadding { Left = 5 };
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = new Colour4(0, 0, 0, 50),
                Radius = corner_radius
            };

            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = new Vector2(2, 0),
                    Colour = BaseColour
                },
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        ButtonContentContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Direction = FillDirection.Horizontal,
                            Shear = -shear,
                            AutoSizeAxes = Axes.X,
                            Height = 100,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                TextContainer = new Container
                                {
                                    Colour = Colour4.White,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Top = 5 },
                                    Child = spriteText = new OsuSpriteText
                                    {
                                        Font = OsuFont.TorusAlternate.With(size: 16),
                                        AlwaysPresent = true,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    }
                                },
                            },
                        },
                    },
                },
            };
        }

        public Action Hovered;
        public Action HoverLost;
        public GlobalAction? Hotkey;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            float horizontalMargin = (100 - TextContainer.Width) / 2;
            ButtonContentContainer.Padding = new MarginPadding
            {
                Left = horizontalMargin,
                // right side margin offset to compensate for shear
                Right = horizontalMargin - SHEAR_WIDTH / 2
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            box.FadeColour(ColourOnHover, 0, Easing.OutQuint);
            Hovered?.Invoke();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            box.FadeColour(BaseColour, 500, Easing.OutQuint);
            HoverLost?.Invoke();
        }


        protected override bool OnClick(ClickEvent e)
        {
            this.ScaleTo(.8f, 900, Easing.OutQuint);
            return base.OnClick(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            this.ScaleTo(1.25f, 500, Easing.OutQuint);
            box.FadeColour(Colour4.White).Then().FadeColour(BaseColour, 300);
            base.OnMouseUp(e);
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action != Hotkey || e.Repeat) return false;

            TriggerClick();
            return true;
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            this.ScaleTo(1.25f, 500, Easing.OutQuint);
            box.FadeColour(Colour4.White).Then().FadeColour(BaseColour, 300);
        }
    }
}
