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
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
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
        private DragRotationState dragRotationState;
        private Vector2 positionMouseDown;

        private Sample tapSample;

        [BackgroundDependencyLoader(true)]
        private void load([NotNull] OsuConfigManager config, [CanBeNull] ScreenshotManager screenshotManager, AudioManager audio)
        {
            cursorRotate = config.GetBindable<bool>(OsuSetting.CursorRotation);

            if (screenshotManager != null)
                screenshotCursorVisibility.BindTo(screenshotManager.CursorVisibility);

            tapSample = audio.Samples.Get(@"UI/cursor-tap");
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (dragRotationState != DragRotationState.NotDragging)
            {
                var position = e.MousePosition;
                float distance = Vector2Extensions.Distance(position, positionMouseDown);

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
            if (State.Value == Visibility.Visible)
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

                playTapSample();
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

                if (State.Value == Visibility.Visible)
                    playTapSample(0.8);
            }

            base.OnMouseUp(e);
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

        private void playTapSample(double baseFrequency = 1f)
        {
            const float random_range = 0.02f;
            SampleChannel channel = tapSample.GetChannel();

            // Scale to [-0.75, 0.75] so that the sample isn't fully panned left or right (sounds weird)
            channel.Balance.Value = ((activeCursor.X / DrawWidth) * 2 - 1) * 0.75;
            channel.Frequency.Value = baseFrequency - (random_range / 2f) + RNG.NextDouble(random_range);

            channel.Play();
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
        }

        private enum DragRotationState
        {
            NotDragging,
            DragStarted,
            Rotating,
        }
    }
}
