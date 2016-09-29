//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.MathUtils;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.GameModes
{
    public class GameModeWhiteBox : GameMode
    {
        private Button popButton;

        const int transition_time = 1000;

        protected virtual IEnumerable<Type> PossibleChildren => null;

        private FlowContainer childModeButtons;

        protected override double OnEntering(GameMode last)
        {
            //only show the pop button if we are entered form another gamemode.
            if (last != null)
                popButton.Alpha = 1;

            MoveTo(new Vector2(ActualSize.X, 0));
            MoveTo(Vector2.Zero, transition_time, EasingTypes.OutQuint);
            return transition_time;
        }

        protected override double OnExiting(GameMode next)
        {
            MoveTo(new Vector2(ActualSize.X, 0), transition_time, EasingTypes.OutQuint);
            return transition_time;
        }

        protected override double OnSuspending(GameMode next)
        {
            Content.MoveTo(new Vector2(-ActualSize.X, 0), transition_time, EasingTypes.OutQuint);
            return transition_time;
        }

        protected override double OnResuming(GameMode last)
        {
            Content.MoveTo(Vector2.Zero, transition_time, EasingTypes.OutQuint);
            return transition_time;
        }

        public override void Load()
        {
            base.Load();

            Children = new Drawable[]
            {
                    new Box
                    {
                        SizeMode = InheritMode.XY,
                        Size = new Vector2(1),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = getColourFor(GetType())
                    },
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
                    popButton = new Button
                    {
                        Text = @"Back",
                        SizeMode = InheritMode.X,
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
                        SizeMode = InheritMode.XY,
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
                        SizeMode = InheritMode.X,
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
