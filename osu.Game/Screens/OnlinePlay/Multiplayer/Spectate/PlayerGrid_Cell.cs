// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class PlayerGrid
    {
        /// <summary>
        /// A cell of the grid. Contains the content and tracks to the linked facade.
        /// </summary>
        private class Cell : CompositeDrawable
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
            private bool isTracking = true;

            public Cell(int facadeIndex, Drawable content)
            {
                FacadeIndex = facadeIndex;

                Origin = Anchor.Centre;
                InternalChild = Content = content;
            }

            protected override void Update()
            {
                base.Update();

                if (isTracking)
                {
                    Position = getFinalPosition();
                    Size = getFinalSize();
                }
            }

            /// <summary>
            /// Makes this cell track a new facade.
            /// </summary>
            public void SetFacade([NotNull] Facade newFacade)
            {
                Facade lastFacade = facade;
                facade = newFacade;

                if (lastFacade == null || lastFacade == newFacade)
                    return;

                isTracking = false;

                this.MoveTo(getFinalPosition(), 400, Easing.OutQuint).ResizeTo(getFinalSize(), 400, Easing.OutQuint)
                    .Then()
                    .OnComplete(_ =>
                    {
                        if (facade == newFacade)
                            isTracking = true;
                    });
            }

            private Vector2 getFinalPosition()
            {
                var topLeft = Parent.ToLocalSpace(facade.ToScreenSpace(Vector2.Zero));
                return topLeft + facade.DrawSize / 2;
            }

            private Vector2 getFinalSize() => facade.DrawSize;

            protected override bool OnClick(ClickEvent e)
            {
                ToggleMaximisationState(this);
                return true;
            }
        }
    }
}
