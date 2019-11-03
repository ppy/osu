// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens
{
    public class ScreenWhiteBox : OsuScreen
    {
        private readonly UnderConstructionMessage message;

        private const double transition_time = 1000;

        protected virtual IEnumerable<Type> PossibleChildren => null;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg2");

        public override bool OnExiting(IScreen next)
        {
            message.TextContainer.MoveTo(new Vector2(DrawSize.X / 16, 0), transition_time, Easing.OutExpo);
            this.FadeOut(transition_time, Easing.OutExpo);

            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            message.TextContainer.MoveTo(new Vector2(-(DrawSize.X / 16), 0), transition_time, Easing.OutExpo);
            this.FadeOut(transition_time, Easing.OutExpo);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            message.TextContainer.MoveTo(Vector2.Zero, transition_time, Easing.OutExpo);
            this.FadeIn(transition_time, Easing.OutExpo);
        }

        public ScreenWhiteBox()
        {
            FillFlowContainer childModeButtons;

            InternalChildren = new Drawable[]
            {
                message = new UnderConstructionMessage(GetType().Name),
                childModeButtons = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(TwoLayerButton.SIZE_RETRACTED.X, 1)
                }
            };

            if (PossibleChildren != null)
            {
                foreach (Type t in PossibleChildren)
                {
                    childModeButtons.Add(new ChildModeButton
                    {
                        Text = $@"{t.Name}",
                        BackgroundColour = getColourFor(t.Name),
                        HoverColour = getColourFor(t.Name).Lighten(0.2f),
                        Action = delegate { this.Push(Activator.CreateInstance(t) as Screen); }
                    });
                }
            }
        }

        private static Color4 getColourFor(object type)
        {
            int hash = type.GetHashCode();
            byte r = (byte)MathHelper.Clamp(((hash & 0xFF0000) >> 16) * 0.8f, 20, 255);
            byte g = (byte)MathHelper.Clamp(((hash & 0x00FF00) >> 8) * 0.8f, 20, 255);
            byte b = (byte)MathHelper.Clamp((hash & 0x0000FF) * 0.8f, 20, 255);
            return new Color4(r, g, b, 255);
        }

        private class ChildModeButton : TwoLayerButton
        {
            public ChildModeButton()
            {
                Icon = OsuIcon.RightCircle;
                Anchor = Anchor.BottomRight;
                Origin = Anchor.BottomRight;
            }
        }

        public class UnderConstructionMessage : CompositeDrawable
        {
            public FillFlowContainer TextContainer { get; }

            private readonly Container boxContainer;

            public UnderConstructionMessage(string name)
            {
                RelativeSizeAxes = Axes.Both;
                Size = new Vector2(0.3f);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                var colour = getColourFor(name);

                InternalChildren = new Drawable[]
                {
                    boxContainer = new Container
                    {
                        CornerRadius = 20,
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,

                                Colour = colour,
                                Alpha = 0.2f,
                                Blending = BlendingParameters.Additive,
                            },
                            TextContainer = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Vertical,
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Icon = FontAwesome.Solid.UniversalAccess,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Size = new Vector2(50),
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = name,
                                        Colour = colour.Lighten(0.8f),
                                        Font = OsuFont.GetFont(size: 36),
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = "is not yet ready for use!",
                                        Font = OsuFont.GetFont(size: 20),
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = "please check back a bit later.",
                                        Font = OsuFont.GetFont(size: 14),
                                    },
                                }
                            },
                        }
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                TextContainer.Position = new Vector2(DrawSize.X / 16, 0);

                boxContainer.Hide();
                boxContainer.ScaleTo(0.2f);
                boxContainer.RotateTo(-20);

                using (BeginDelayedSequence(300, true))
                {
                    boxContainer.ScaleTo(1, transition_time, Easing.OutElastic);
                    boxContainer.RotateTo(0, transition_time / 2, Easing.OutQuint);

                    TextContainer.MoveTo(Vector2.Zero, transition_time, Easing.OutExpo);
                    boxContainer.FadeIn(transition_time, Easing.OutExpo);
                }
            }
        }
    }
}
