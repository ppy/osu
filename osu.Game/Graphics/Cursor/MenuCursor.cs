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
                ActiveCursor.RotateTo(0, 200, EasingTypes.OutQuint);
                ActiveCursor.ScaleTo(1, 500, EasingTypes.OutElastic);
            }

            return base.OnMouseUp(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            ((Cursor)ActiveCursor).AdditiveLayer.FadeOutFromOne(500, EasingTypes.OutQuint);

            return base.OnClick(state);
        }

        protected override bool OnDragStart(InputState state)
        {
            ActiveCursor.RotateTo(-30, 600, EasingTypes.OutElastic);
            return base.OnDragStart(state);
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
            private void load(OsuConfigManager config, TextureStore textures)
            {
                cursorScale = config.GetBindable<double>(OsuConfig.CursorSize);

                Children = new Drawable[]
                {
                    cursorContainer = new Container
                    {
                        Size = new Vector2(28),
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
                                Alpha = 0,
                                Texture = textures.Get(@"Cursor/menu-cursor"),
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
