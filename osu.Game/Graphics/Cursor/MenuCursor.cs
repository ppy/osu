// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using System;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Utils;

namespace osu.Game.Graphics.Cursor
{
    public class MenuCursor : CursorContainer
    {
        private readonly IBindable<bool> screenshotCursorVisibility = new Bindable<bool>(true);
        public override bool IsPresent => screenshotCursorVisibility.Value && base.IsPresent;

        protected override Drawable CreateCursor() => activeCursor = new Cursor();

        private Cursor activeCursor;

        private Bindable<bool> cursorRotate;
        public BindableBool UseSystemCursor = new BindableBool();
        private bool changedWhenHidden;
        private DragRotationState dragRotationState;
        private Vector2 positionMouseDown;

        [Resolved]
        private GameHost host { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load([NotNull] OsuConfigManager config, [NotNull] MConfigManager mConfig, [CanBeNull] ScreenshotManager screenshotManager)
        {
            cursorRotate = config.GetBindable<bool>(OsuSetting.CursorRotation);
            mConfig.BindWith(MSetting.UseSystemCursor, UseSystemCursor);

            UseSystemCursor.BindValueChanged(v =>
            {
                if (State.Value == Visibility.Hidden)
                {
                    changedWhenHidden = true;
                    return;
                }

                if (v.NewValue)
                    showSystemCursor(true);
                else
                    hideSystemCursor(true);
            }, true);

            if (screenshotManager != null)
                screenshotCursorVisibility.BindTo(screenshotManager.CursorVisibility);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (dragRotationState != DragRotationState.NotDragging)
            {
                var position = e.MousePosition;
                var distance = Vector2Extensions.Distance(position, positionMouseDown);

                // don't start rotating until we're moved a minimum distance away from the mouse down location,
                // else it can have an annoying effect.
                if (dragRotationState == DragRotationState.DragStarted && distance > 30)
                    dragRotationState = DragRotationState.Rotating;

                // don't rotate when distance is zero to avoid NaN
                if (dragRotationState == DragRotationState.Rotating && distance > 0)
                {
                    Vector2 offset = e.MousePosition - positionMouseDown;
                    float degrees = MathUtils.RadiansToDegrees(MathF.Atan2(-offset.X, offset.Y)) + 24.3f;

                    // Always rotate in the direction of least distance
                    float diff = (degrees - activeCursor.Rotation) % 360;
                    if (diff < -180) diff += 360;
                    if (diff > 180) diff -= 360;
                    degrees = activeCursor.Rotation + diff;

                    activeCursor.RotateTo(degrees, 600, Easing.OutQuint);
                }
            }

            return base.OnMouseMove(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // only trigger animation for main mouse buttons
            activeCursor.Scale = new Vector2(1);
            activeCursor.ScaleTo(0.90f, 800, Easing.OutQuint);

            activeCursor.AdditiveLayer.Alpha = 0;
            activeCursor.AdditiveLayer.FadeInFromZero(800, Easing.OutQuint);

            if (cursorRotate.Value && dragRotationState != DragRotationState.Rotating)
            {
                // if cursor is already rotating don't reset its rotate origin
                dragRotationState = DragRotationState.DragStarted;
                positionMouseDown = e.MousePosition;
            }

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (!e.HasAnyButtonPressed)
            {
                activeCursor.AdditiveLayer.FadeOutFromOne(500, Easing.OutQuint);
                activeCursor.ScaleTo(1, 500, Easing.OutElastic);

                if (dragRotationState != DragRotationState.NotDragging)
                {
                    activeCursor.RotateTo(0, 600 * (1 + Math.Abs(activeCursor.Rotation / 720)), Easing.OutElasticHalf);
                    dragRotationState = DragRotationState.NotDragging;
                }
            }

            base.OnMouseUp(e);
        }

        protected override void PopIn()
        {
            activeCursor.FadeTo(1, 250, Easing.OutQuint);
            activeCursor.ScaleTo(1, 400, Easing.OutQuint);

            if (UseSystemCursor.Value) showSystemCursor(changedWhenHidden);
            else hideSystemCursor(changedWhenHidden);
            changedWhenHidden = false;
        }

        private void showSystemCursor(bool toggleGameCursor = false)
        {
            switch (host.Window)
            {
                // SDL2 DesktopWindow
                case SDL2DesktopWindow desktopWindow:
                    desktopWindow.CursorState = CursorState.Default;
                    break;
            }

            if (toggleGameCursor) activeCursor.FadeOutContainer();
        }

        private void hideSystemCursor(bool toggleGameCursor = false)
        {
            switch (host.Window)
            {
                // SDL2 DesktopWindow
                case SDL2DesktopWindow desktopWindow:
                    desktopWindow.CursorState = CursorState.Hidden;
                    break;
            }

            if (toggleGameCursor) activeCursor.FadeInContainer();
        }

        protected override void PopOut()
        {
            activeCursor.FadeTo(0, 250, Easing.OutQuint);
            activeCursor.ScaleTo(0.6f, 250, Easing.In);

            if (UseSystemCursor.Value) hideSystemCursor();
        }

        public class Cursor : Container
        {
            private Container cursorContainer;
            private Bindable<float> cursorScale;
            private const float base_scale = 0.15f;

            public Sprite AdditiveLayer;

            public Cursor()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuConfigManager config, TextureStore textures, OsuColour colour, MConfigManager mConfig)
            {
                Children = new Drawable[]
                {
                    cursorContainer = new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Alpha = mConfig.Get<bool>(MSetting.UseSystemCursor) ? 0 : 1,
                        Children = new Drawable[]
                        {
                            new Sprite
                            {
                                Texture = textures.Get(@"Cursor/menu-cursor"),
                            },
                            AdditiveLayer = new Sprite
                            {
                                Blending = BlendingParameters.Additive,
                                Colour = colour.Pink,
                                Alpha = 0,
                                Texture = textures.Get(@"Cursor/menu-cursor-additive"),
                            },
                        }
                    }
                };

                cursorScale = config.GetBindable<float>(OsuSetting.MenuCursorSize);
                cursorScale.BindValueChanged(scale => cursorContainer.Scale = new Vector2(scale.NewValue * base_scale), true);
            }

            public void FadeOutContainer()
            {
                cursorContainer.FadeOut(200);
                AutoSizeAxes = Axes.None;
                Width = 15;
                Height = 0;
            }

            public void FadeInContainer()
            {
                cursorContainer.FadeIn(200);
                AutoSizeAxes = Axes.Both;
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
