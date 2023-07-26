// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
            public Action<Cell> ToggleMaximisationState;

            /// <summary>
            /// Whether this cell is currently maximised.
            /// </summary>
            public bool IsMaximised;

            private Facade facade;

            private bool isAnimating;

            public Cell(int facadeIndex, Drawable content)
            {
                FacadeIndex = facadeIndex;

                Origin = Anchor.Centre;
                InternalChild = Content = content;
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
                isAnimating &= !Precision.AlmostEquals(Position, targetPos, 0.01f);
            }

            /// <summary>
            /// Makes this cell track a new facade.
            /// </summary>
            public void SetFacade([NotNull] Facade newFacade)
            {
                facade = newFacade;
                isAnimating = true;
            }

            private Vector2 getFinalPosition() =>
                Parent.ToLocalSpace(facade.ScreenSpaceDrawQuad.Centre);

            private Vector2 getFinalSize() =>
                Parent.ToLocalSpace(facade.ScreenSpaceDrawQuad.BottomRight)
                - Parent.ToLocalSpace(facade.ScreenSpaceDrawQuad.TopLeft);

            protected override bool OnClick(ClickEvent e)
            {
                ToggleMaximisationState(this);
                return true;
            }
        }
    }
}
