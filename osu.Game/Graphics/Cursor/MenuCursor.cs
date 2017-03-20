// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Configuration;
using System;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Graphics.Cursor
{
    public class MenuCursor : CursorContainer
    {
        protected override Drawable CreateCursor() => new Cursor();

        protected override bool OnMouseMove(InputState state)
        {
            if (state.Mouse.HasMainButtonPressed)
            {
                Vector2 offset = state.Mouse.Position - state.Mouse.PositionMouseDown ?? state.Mouse.Delta;
                float degrees = (float)MathHelper.RadiansToDegrees(Math.Atan2(-offset.X, offset.Y)) + 24.3f;

                // Always rotate in the direction of least distance
                float diff = (degrees - ActiveCursor.Rotation) % 360;
                if (diff < -180) diff += 360;
                if (diff > 180) diff -= 360;
                degrees = ActiveCursor.Rotation + diff;

                ActiveCursor.RotateTo(degrees, 600, EasingTypes.OutQuint);
            }

            return base.OnMouseMove(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            ActiveCursor.Scale = new Vector2(1);
            ActiveCursor.ScaleTo(0.90f, 800, EasingTypes.OutQuint);

            ((Cursor)ActiveCursor).AdditiveLayer.Alpha = 0;
            ((Cursor)ActiveCursor).AdditiveLayer.FadeInFromZero(800, EasingTypes.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (!state.Mouse.HasMainButtonPressed)
            {
                ((Cursor)ActiveCursor).AdditiveLayer.FadeOut(500, EasingTypes.OutQuint);
                ActiveCursor.RotateTo(0, 600 * (1 + Math.Abs(ActiveCursor.Rotation / 720)), EasingTypes.OutElasticHalf);
                ActiveCursor.ScaleTo(1, 500, EasingTypes.OutElastic);
            }

            return base.OnMouseUp(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            ((Cursor)ActiveCursor).AdditiveLayer.FadeOutFromOne(500, EasingTypes.OutQuint);

            return base.OnClick(state);
        }

        protected override void PopIn()
        {
            ActiveCursor.FadeTo(1, 250, EasingTypes.OutQuint);
            ActiveCursor.ScaleTo(1, 1000, EasingTypes.OutElastic);
        }

        protected override void PopOut()
        {
            ActiveCursor.FadeTo(0, 1400, EasingTypes.OutQuint);
            ActiveCursor.ScaleTo(1.1f, 100, EasingTypes.Out);
            ActiveCursor.Delay(100);
            ActiveCursor.ScaleTo(0, 500, EasingTypes.In);
        }

        public class Cursor : Container
        {
            private Container cursorContainer;
            private Bindable<double> cursorScale;

            public Sprite AdditiveLayer;

            public Cursor()
            {
                Size = new Vector2(42);
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, TextureStore textures, OsuColour colour)
            {
                cursorScale = config.GetBindable<double>(OsuConfig.CursorSize);

                Children = new Drawable[]
                {
                    cursorContainer = new Container
                    {
                        Size = new Vector2(32),
                        Children = new Drawable[]
                        {
                            new Sprite
                            {
                                FillMode = FillMode.Fit,
                                Texture = textures.Get(@"Cursor/menu-cursor"),
                            },
                            AdditiveLayer = new Sprite
                            {
                                FillMode = FillMode.Fit,
                                BlendingMode = BlendingMode.Additive,
                                Colour = colour.Pink,
                                Alpha = 0,
                                Texture = textures.Get(@"Cursor/menu-cursor-additive"),
                            },
                        }
                    }
                };
                cursorScale.ValueChanged += scaleChanged;
            }

            private void scaleChanged(object sender, EventArgs e)
            {
                cursorContainer.Scale = new Vector2((float)cursorScale);
            }
        }
    }
}
