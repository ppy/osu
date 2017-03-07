﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Backgrounds;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens
{
    public class ScreenWhiteBox : OsuScreen
    {
        private BackButton popButton;

        private const int transition_time = 1000;

        protected virtual IEnumerable<Type> PossibleChildren => null;

        private FillFlowContainer childModeButtons;
        private Container textContainer;
        private Box box;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenCustom(@"Backgrounds/bg2");

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            //only show the pop button if we are entered form another gamemode.
            if (last != null)
                popButton.Alpha = 1;

            Content.Alpha = 0;
            textContainer.Position = new Vector2(DrawSize.X / 16, 0);

            box.ScaleTo(0.2f);
            box.RotateTo(-20);

            Content.Delay(300, true);

            box.ScaleTo(1, transition_time, EasingTypes.OutElastic);
            box.RotateTo(0, transition_time / 2, EasingTypes.OutQuint);

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
            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.3f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = getColourFor(GetType()),
                    Alpha = 1,
                    BlendingMode = BlendingMode.Additive,
                },
                textContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Text = GetType().Name,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            TextSize = 50,
                        },
                        new OsuSpriteText
                        {
                            Text = GetType().Namespace,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Position = new Vector2(0, 30)
                        },
                    }
                },
                popButton = new BackButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Alpha = 0,
                    Action = delegate {
                        Exit();
                    }
                },
                childModeButtons = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.1f, 1)
                }
            };

            if (PossibleChildren != null)
            {
                foreach (Type t in PossibleChildren)
                {
                    childModeButtons.Add(new Button
                    {
                        Text = $@"{t.Name}",
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(1, 40),
                        Anchor = Anchor.BottomRight,
                        Origin = Anchor.BottomRight,
                        BackgroundColour = getColourFor(t),
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
    }
}
