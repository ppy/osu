//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Game.GameModes.Backgrounds;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;

namespace osu.Game.GameModes
{
    public class GameModeWhiteBox : OsuGameMode
    {
        private Button popButton;

        const int transition_time = 1000;

        protected virtual IEnumerable<Type> PossibleChildren => null;

        private FlowContainer childModeButtons;
        private Container textContainer;
        private Box box;

        protected override BackgroundMode CreateBackground() => new BackgroundModeCustom(@"Backgrounds/bg2");

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);

            //only show the pop button if we are entered form another gamemode.
            if (last != null)
                popButton.Alpha = 1;

            Content.Alpha = 0;
            textContainer.Position = new Vector2(Size.X / 16, 0);

            box.ScaleTo(0.2f);
            box.RotateTo(-20);

            Content.Delay(300, true);

            box.ScaleTo(1, transition_time, EasingTypes.OutElastic);
            box.RotateTo(0, transition_time / 2, EasingTypes.OutQuint);

            textContainer.MoveTo(Vector2.Zero, transition_time, EasingTypes.OutExpo);
            Content.FadeIn(transition_time, EasingTypes.OutExpo);
        }

        protected override bool OnExiting(GameMode next)
        {
            textContainer.MoveTo(new Vector2((Size.X / 16), 0), transition_time, EasingTypes.OutExpo);
            Content.FadeOut(transition_time, EasingTypes.OutExpo);

            return base.OnExiting(next);
        }

        protected override void OnSuspending(GameMode next)
        {
            base.OnSuspending(next);

            textContainer.MoveTo(new Vector2(-(Size.X / 16), 0), transition_time, EasingTypes.OutExpo);
            Content.FadeOut(transition_time, EasingTypes.OutExpo);
        }

        protected override void OnResuming(GameMode last)
        {
            base.OnResuming(last);

            textContainer.MoveTo(Vector2.Zero, transition_time, EasingTypes.OutExpo);
            Content.FadeIn(transition_time, EasingTypes.OutExpo);
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

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
                        Additive = false
                    },
                    textContainer = new AutoSizeContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new[]
                        {
                            new SpriteText
                            {
                                Text = GetType().Name,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                TextSize = 50,
                            },
                            new SpriteText
                            {
                                Text = GetType().Namespace,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Position = new Vector2(0, 30)
                            },
                        }
                    },
                    popButton = new Button
                    {
                        Text = @"Back",
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(0.1f, 40),
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Colour = new Color4(235, 51, 153, 255),
                        Alpha = 0,
                        Action = delegate {
                            Exit();
                        }
                    },
                    childModeButtons = new FlowContainer
                    {
                        Direction = FlowDirection.VerticalOnly,
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
                        Colour = getColourFor(t),
                        Action = delegate
                        {
                            Push(Activator.CreateInstance(t) as GameMode);
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
