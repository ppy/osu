// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Graphics.Cursor
{
    public class MenuCursor : CursorContainer
    {
        private readonly IBindable<bool> screenshotCursorVisibility = new Bindable<bool>(true);
        public override bool IsPresent => screenshotCursorVisibility.Value && base.IsPresent;

        protected override Drawable CreateCursor() => new Cursor();

        private Bindable<bool> cursorRotate;
        private bool dragging;

        private bool startRotation;

        [BackgroundDependencyLoader(true)]
        private void load([NotNull] OsuConfigManager config, [CanBeNull] ScreenshotManager screenshotManager)
        {
            cursorRotate = config.GetBindable<bool>(OsuSetting.CursorRotation);

            if (screenshotManager != null)
                screenshotCursorVisibility.BindTo(screenshotManager.CursorVisibility);
        }

        protected override bool OnMouseMove(InputState state)
        {
            if (cursorRotate && dragging)
            {
                Debug.Assert(state.Mouse.PositionMouseDown != null);

                // don't start rotating until we're moved a minimum distance away from the mouse down location,
                // else it can have an annoying effect.
                // ReSharper disable once PossibleInvalidOperationException
                startRotation |= Vector2Extensions.Distance(state.Mouse.Position, state.Mouse.PositionMouseDown.Value) > 30;

                if (startRotation)
                {
                    Vector2 offset = state.Mouse.Position - state.Mouse.PositionMouseDown.Value;
                    float degrees = (float)MathHelper.RadiansToDegrees(Math.Atan2(-offset.X, offset.Y)) + 24.3f;

                    // Always rotate in the direction of least distance
                    float diff = (degrees - ActiveCursor.Rotation) % 360;
                    if (diff < -180) diff += 360;
                    if (diff > 180) diff -= 360;
                    degrees = ActiveCursor.Rotation + diff;

                    ActiveCursor.RotateTo(degrees, 600, Easing.OutQuint);
                }
            }

            return base.OnMouseMove(state);
        }

        protected override bool OnDragStart(InputState state)
        {
            dragging = true;
            return base.OnDragStart(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            ActiveCursor.Scale = new Vector2(1);
            ActiveCursor.ScaleTo(0.90f, 800, Easing.OutQuint);

            ((Cursor)ActiveCursor).AdditiveLayer.Alpha = 0;
            ((Cursor)ActiveCursor).AdditiveLayer.FadeInFromZero(800, Easing.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (!state.Mouse.HasMainButtonPressed)
            {
                dragging = false;
                startRotation = false;

                ((Cursor)ActiveCursor).AdditiveLayer.FadeOut(500, Easing.OutQuint);
                ActiveCursor.RotateTo(0, 600 * (1 + Math.Abs(ActiveCursor.Rotation / 720)), Easing.OutElasticHalf);
                ActiveCursor.ScaleTo(1, 500, Easing.OutElastic);
            }

            return base.OnMouseUp(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            ((Cursor)ActiveCursor).AdditiveLayer.FadeOutFromOne(500, Easing.OutQuint);

            return base.OnClick(state);
        }

        protected override void PopIn()
        {
            ActiveCursor.FadeTo(1, 250, Easing.OutQuint);
            ActiveCursor.ScaleTo(1, 400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            ActiveCursor.FadeTo(0, 250, Easing.OutQuint);
            ActiveCursor.ScaleTo(0.6f, 250, Easing.In);
        }

        public class Cursor : Container
        {
            private Container cursorContainer;
            private Bindable<double> cursorScale;
            private const float base_scale = 0.15f;

            public Sprite AdditiveLayer;

            public Cursor()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, TextureStore textures, OsuColour colour)
            {
                Children = new Drawable[]
                {
                    cursorContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Sprite
                            {
                                Texture = textures.Get(@"Cursor/menu-cursor"),
                            },
                            AdditiveLayer = new Sprite
                            {
                                Blending = BlendingMode.Additive,
                                Colour = colour.Pink,
                                Alpha = 0,
                                Texture = textures.Get(@"Cursor/menu-cursor-additive"),
                            },
                        }
                    }
                };

                cursorScale = config.GetBindable<double>(OsuSetting.MenuCursorSize);
                cursorScale.ValueChanged += newScale => cursorContainer.Scale = new Vector2((float)newScale * base_scale);
                cursorScale.TriggerChange();
            }
        }
    }
}
