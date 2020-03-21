// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class OverlayScrollContainer : OsuScrollContainer
    {
        public ScrollToTopButton Button { get; }

        private float currentTarget;

        public OverlayScrollContainer()
        {
            AddInternal(Button = new ScrollToTopButton
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(20),
                Action = () =>
                {
                    ScrollToStart();
                    currentTarget = Target;
                    Button.State.Value = Visibility.Hidden;
                }
            });
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (ScrollContent.DrawHeight < DrawHeight)
            {
                Button.State.Value = Visibility.Hidden;
                return;
            }

            if (Target == currentTarget)
                return;

            currentTarget = Target;
            Button.State.Value = Current > 200 ? Visibility.Visible : Visibility.Hidden;
        }

        public class ScrollToTopButton : VisibilityContainer
        {
            private const int fade_duration = 500;

            public Action Action
            {
                get => button.Action;
                set => button.Action = value;
            }

            public override bool PropagatePositionalInputSubTree => true;

            protected override bool StartHidden => true;

            private readonly Button button;

            public ScrollToTopButton()
            {
                Size = new Vector2(50);
                Child = button = new Button();
            }

            protected override bool OnMouseDown(MouseDownEvent e) => true;

            protected override void PopIn() => button.FadeIn(fade_duration, Easing.OutQuint);

            protected override void PopOut() => button.FadeOut(fade_duration, Easing.OutQuint);

            private class Button : OsuHoverContainer
            {
                public override bool PropagatePositionalInputSubTree => Alpha == 1;

                protected override IEnumerable<Drawable> EffectTargets => new[] { background };

                private Color4 flashColour;

                private readonly Container content;
                private readonly Box background;

                public Button()
                {
                    RelativeSizeAxes = Axes.Both;
                    Add(content = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Offset = new Vector2(0f, 1f),
                            Radius = 3f,
                            Colour = Color4.Black.Opacity(0.25f),
                        },
                        Children = new Drawable[]
                        {
                            background = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(15),
                                Icon = FontAwesome.Solid.ChevronUp
                            }
                        }
                    });

                    TooltipText = "Scroll to top";
                }

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    IdleColour = colourProvider.Background6;
                    HoverColour = colourProvider.Background5;
                    flashColour = colourProvider.Light1;
                }

                protected override bool OnClick(ClickEvent e)
                {
                    background.FlashColour(flashColour, 800, Easing.OutQuint);
                    return base.OnClick(e);
                }

                protected override bool OnMouseDown(MouseDownEvent e)
                {
                    content.ScaleTo(0.75f, 2000, Easing.OutQuint);
                    return true;
                }

                protected override void OnMouseUp(MouseUpEvent e)
                {
                    content.ScaleTo(1, 1000, Easing.OutElastic);
                    base.OnMouseUp(e);
                }
            }
        }
    }
}
