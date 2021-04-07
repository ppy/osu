// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            public Action<Cell> ToggleMaximisationState;
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

            // Todo: Temporary?
            protected override bool ShouldBeConsideredForInput(Drawable child) => false;

            protected override bool OnClick(ClickEvent e)
            {
                ToggleMaximisationState(this);
                return true;
            }
        }
    }
}
