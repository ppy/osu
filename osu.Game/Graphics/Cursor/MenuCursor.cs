// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using System;
using JetBrains.Annotations;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using OpenTK.Input;

namespace osu.Game.Graphics.Cursor
{
    public class MenuCursor : CursorContainer
    {
        private readonly IBindable<bool> screenshotCursorVisibility = new Bindable<bool>(true);
        public override bool IsPresent => screenshotCursorVisibility.Value && base.IsPresent;

        protected override Drawable CreateCursor() => activeCursor = new Cursor();

        private Cursor activeCursor;

        private Bindable<bool> cursorRotate;
        private DragRotationState dragRotationState;
        private Vector2 positionMouseDown;

        [BackgroundDependencyLoader(true)]
        private void load([NotNull] OsuConfigManager config, [CanBeNull] ScreenshotManager screenshotManager)
        {
            cursorRotate = config.GetBindable<bool>(OsuSetting.CursorRotation);

            if (screenshotManager != null)
                screenshotCursorVisibility.BindTo(screenshotManager.CursorVisibility);
        }

        protected override bool OnMouseMove(InputState state)
        {
            if (dragRotationState != DragRotationState.NotDragging)
            {
                var position = state.Mouse.Position;
                var distance = Vector2Extensions.Distance(position, positionMouseDown);
                // don't start rotating until we're moved a minimum distance away from the mouse down location,
                // else it can have an annoying effect.
                if (dragRotationState == DragRotationState.DragStarted && distance > 30)
                    dragRotationState = DragRotationState.Rotating;
                // don't rotate when distance is zero to avoid NaN
                if (dragRotationState == DragRotationState.Rotating && distance > 0)
                {
                    Vector2 offset = state.Mouse.Position - positionMouseDown;
                    float degrees = (float)MathHelper.RadiansToDegrees(Math.Atan2(-offset.X, offset.Y)) + 24.3f;

                    // Always rotate in the direction of least distance
                    float diff = (degrees - activeCursor.Rotation) % 360;
                    if (diff < -180) diff += 360;
                    if (diff > 180) diff -= 360;
                    degrees = activeCursor.Rotation + diff;

                    activeCursor.RotateTo(degrees, 600, Easing.OutQuint);
                }
            }

            return base.OnMouseMove(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            // only trigger animation for main mouse buttons
            if (args.Button <= MouseButton.Right)
            {
                activeCursor.Scale = new Vector2(1);
                activeCursor.ScaleTo(0.90f, 800, Easing.OutQuint);

                activeCursor.AdditiveLayer.Alpha = 0;
                activeCursor.AdditiveLayer.FadeInFromZero(800, Easing.OutQuint);
            }

            if (args.Button == MouseButton.Left && cursorRotate)
            {
                dragRotationState = DragRotationState.DragStarted;
                positionMouseDown = state.Mouse.Position;
            }
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            if (!state.Mouse.HasMainButtonPressed)
            {
                activeCursor.AdditiveLayer.FadeOutFromOne(500, Easing.OutQuint);
                activeCursor.ScaleTo(1, 500, Easing.OutElastic);
            }

            if (args.Button == MouseButton.Left)
            {
                if (dragRotationState == DragRotationState.Rotating)
                    activeCursor.RotateTo(0, 600 * (1 + Math.Abs(activeCursor.Rotation / 720)), Easing.OutElasticHalf);
                dragRotationState = DragRotationState.NotDragging;
            }
            return base.OnMouseUp(state, args);
        }

        protected override void PopIn()
        {
            activeCursor.FadeTo(1, 250, Easing.OutQuint);
            activeCursor.ScaleTo(1, 400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            activeCursor.FadeTo(0, 250, Easing.OutQuint);
            activeCursor.ScaleTo(0.6f, 250, Easing.In);
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

        private enum DragRotationState
        {
            NotDragging,
            DragStarted,
            Rotating,
        }
    }
}
