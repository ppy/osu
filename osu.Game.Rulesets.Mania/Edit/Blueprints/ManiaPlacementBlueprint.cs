// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public abstract class ManiaPlacementBlueprint<T> : PlacementBlueprint
        where T : ManiaHitObject
    {
        protected new T HitObject => (T)base.HitObject;

        /// <summary>
        /// The current mouse position, snapped to the closest column.
        /// </summary>
        protected Vector2 SnappedMousePosition { get; private set; }

        /// <summary>
        /// The width of the closest column to the current mouse position.
        /// </summary>
        protected float SnappedWidth { get; private set; }

        [Resolved]
        private IManiaHitObjectComposer composer { get; set; }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        protected ManiaPlacementBlueprint(T hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            Column column = ColumnAt(e.ScreenSpaceMousePosition);

            if (column == null)
                SnappedMousePosition = e.MousePosition;
            else
            {
                SnappedWidth = column.DrawWidth;

                // Snap to the column
                var parentPos = Parent.ToLocalSpace(column.ToScreenSpace(new Vector2(column.DrawWidth / 2, 0)));
                SnappedMousePosition = new Vector2(parentPos.X, e.MousePosition.Y);
            }

            return true;
        }

        protected double TimeAt(Vector2 screenSpacePosition)
        {
            var column = ColumnAt(screenSpacePosition);
            if (column == null)
                return 0;

            var hitObjectContainer = column.HitObjectContainer;

            // If we're scrolling downwards, a position of 0 is actually further away from the hit target
            // so we need to flip the vertical coordinate in the hitobject container's space
            var hitObjectPos = column.HitObjectContainer.ToLocalSpace(applyPositionOffset(screenSpacePosition)).Y;
            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                hitObjectPos = hitObjectContainer.DrawHeight - hitObjectPos;

            return scrollingInfo.Algorithm.TimeAt(hitObjectPos,
                EditorClock.CurrentTime,
                scrollingInfo.TimeRange.Value,
                hitObjectContainer.DrawHeight);
        }

        protected Column ColumnAt(Vector2 screenSpacePosition)
            => composer.ColumnAt(applyPositionOffset(screenSpacePosition));

        private Vector2 applyPositionOffset(Vector2 position)
        {
            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                    position.Y -= NotePiece.NOTE_HEIGHT / 2;
                    break;
                case ScrollingDirection.Down:
                    position.Y += NotePiece.NOTE_HEIGHT / 2;
                    break;
            }

            return position;
        }
    }
}
