// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class PlayerGrid
    {
        /// <summary>
        /// A cell of the grid. Contains the content and tracks to the linked facade.
        /// </summary>
        private partial class Cell : CompositeDrawable
        {
            /// <summary>
            /// The index of the original facade of this cell.
            /// </summary>
            public readonly int FacadeIndex;

            /// <summary>
            /// The contained content.
            /// </summary>
            public readonly Drawable Content;

            /// <summary>
            /// An action that toggles the maximisation state of this cell.
            /// </summary>
            public Action<Cell>? ToggleMaximisationState;

            /// <summary>
            /// Whether this cell is currently maximised.
            /// </summary>
            public bool IsMaximised { get; private set; }

            private Facade facade;

            private bool isAnimating;

            public Cell(int facadeIndex, Drawable content, Facade facade)
            {
                FacadeIndex = facadeIndex;
                this.facade = facade;

                Origin = Anchor.Centre;
                InternalChild = Content = content;

                Masking = true;
                CornerRadius = 5;
            }

            protected override void Update()
            {
                base.Update();

                var targetPos = getFinalPosition();
                var targetSize = getFinalSize();

                double duration = isAnimating ? 60 : 0;

                Position = new Vector2(
                    (float)Interpolation.DampContinuously(Position.X, targetPos.X, duration, Time.Elapsed),
                    (float)Interpolation.DampContinuously(Position.Y, targetPos.Y, duration, Time.Elapsed)
                );

                Size = new Vector2(
                    (float)Interpolation.DampContinuously(Size.X, targetSize.X, duration, Time.Elapsed),
                    (float)Interpolation.DampContinuously(Size.Y, targetSize.Y, duration, Time.Elapsed)
                );

                // If we don't track the animating state, the animation will also occur when resizing the window.
                isAnimating &= !Precision.AlmostEquals(Size, targetSize, 0.5f);
            }

            /// <summary>
            /// Makes this cell track a new facade.
            /// </summary>
            public void SetFacade(Facade newFacade, bool isMaximised)
            {
                facade = newFacade;
                IsMaximised = isMaximised;
                isAnimating = true;

                TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = isMaximised ? 30 : 10,
                    Colour = Colour4.Black.Opacity(isMaximised ? 0.5f : 0.2f),
                }, ANIMATION_DELAY, Easing.OutQuint);
            }

            private Vector2 getFinalPosition() =>
                Parent!.ToLocalSpace(facade.ScreenSpaceDrawQuad.Centre);

            private Vector2 getFinalSize() =>
                Parent!.ToLocalSpace(facade.ScreenSpaceDrawQuad.BottomRight)
                - Parent!.ToLocalSpace(facade.ScreenSpaceDrawQuad.TopLeft);

            protected override bool OnClick(ClickEvent e)
            {
                ToggleMaximisationState?.Invoke(this);
                return true;
            }
        }
    }
}
