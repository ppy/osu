﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Backgrounds;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens
{
    public class ScreenWhiteBox : OsuScreen
    {
        private readonly BackButton popButton;

        private const double transition_time = 1000;

        protected virtual IEnumerable<Type> PossibleChildren => null;

        private readonly FillFlowContainer textContainer;
        private readonly Container boxContainer;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg2");

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            //only show the pop button if we are entered form another screen.
            if (last != null)
                popButton.Alpha = 1;

            Content.Alpha = 0;
            textContainer.Position = new Vector2(DrawSize.X / 16, 0);

            boxContainer.ScaleTo(0.2f);
            boxContainer.RotateTo(-20);

            Content.Delay(300, true);

            boxContainer.ScaleTo(1, transition_time, EasingTypes.OutElastic);
            boxContainer.RotateTo(0, transition_time / 2, EasingTypes.OutQuint);

            textContainer.MoveTo(Vector2.Zero, transition_time, EasingTypes.OutExpo);
            Content.FadeIn(transition_time, EasingTypes.OutExpo);
        }

        protected override bool OnExiting(Screen next)
        {
            textContainer.MoveTo(new Vector2(DrawSize.X / 16, 0), transition_time, EasingTypes.OutExpo);
            Content.FadeOut(transition_time, EasingTypes.OutExpo);

            return base.OnExiting(next);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);

            textContainer.MoveTo(new Vector2(-(DrawSize.X / 16), 0), transition_time, EasingTypes.OutExpo);
            Content.FadeOut(transition_time, EasingTypes.OutExpo);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            textContainer.MoveTo(Vector2.Zero, transition_time, EasingTypes.OutExpo);
            Content.FadeIn(transition_time, EasingTypes.OutExpo);
        }

        public ScreenWhiteBox()
        {
            FillFlowContainer childModeButtons;

            Children = new Drawable[]
            {
                boxContainer = new Container
                {
                    Size = new Vector2(0.3f),
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 20,
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,

                            Colour = getColourFor(GetType()),
                            Alpha = 0.2f,
                            BlendingMode = BlendingMode.Additive,
                        },
                        textContainer = new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Children = new[]
                            {
                                new TextAwesome
                                {
                                    Icon = FontAwesome.fa_universal_access,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    TextSize = 50,
                                },
                                new OsuSpriteText
                                {
                                    Text = GetType().Name,
                                    Colour = getColourFor(GetType()).Lighten(0.8f),
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    TextSize = 50,
                                },
                                new OsuSpriteText
                                {
                                    Text = "is not yet ready for use!",
                                    TextSize = 20,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                                new OsuSpriteText
                                {
                                    Text = "please check back a bit later.",
                                    TextSize = 14,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                },
                            }
                        },
                    }
                },
                popButton = new BackButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Alpha = 0,
                    Action = Exit
                },
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
                        BackgroundColour = getColourFor(t),
                        HoverColour = getColourFor(t).Lighten(0.2f),
                        Action = delegate
                        {
                            Push(Activator.CreateInstance(t) as Screen);
                        }
                    });
                }
            }
        }

        private Color4 getColourFor(Type type)
        {
            int hash = type.Name.GetHashCode();
            byte r = (byte)MathHelper.Clamp(((hash & 0xFF0000) >> 16) * 0.8f, 20, 255);
            byte g = (byte)MathHelper.Clamp(((hash & 0x00FF00) >> 8) * 0.8f, 20, 255);
            byte b = (byte)MathHelper.Clamp((hash & 0x0000FF) * 0.8f, 20, 255);
            return new Color4(r, g, b, 255);
        }

        public class ChildModeButton : TwoLayerButton
        {
            public ChildModeButton()
            {
                Icon = FontAwesome.fa_osu_right_o;
                Anchor = Anchor.BottomRight;
                Origin = Anchor.BottomRight;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                ActivationSound = audio.Sample.Get(@"Menu/menuhit");
            }
        }
    }
}
