// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public abstract class ManiaPlacementBlueprint<T> : PlacementBlueprint,
                                                       IRequireHighFrequencyMousePosition // the playfield could be moving behind us
        where T : ManiaHitObject
    {
        protected new T HitObject => (T)base.HitObject;

        protected Column Column;

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

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (Column == null)
                return base.OnMouseDown(e);

            HitObject.StartTime = TimeAt(e.ScreenSpaceMousePosition);
            HitObject.Column = Column.Index;

            BeginPlacement();
            return true;
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            EndPlacement();
            return base.OnMouseUp(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (!PlacementBegun)
                Column = ColumnAt(e.ScreenSpaceMousePosition);

            if (Column == null) return false;

            SnappedWidth = Column.DrawWidth;

            // Snap to the column
            var parentPos = Parent.ToLocalSpace(Column.ToScreenSpace(new Vector2(Column.DrawWidth / 2, 0)));
            SnappedMousePosition = new Vector2(parentPos.X, e.MousePosition.Y);
            return true;
        }

        protected double TimeAt(Vector2 screenSpacePosition)
        {
            if (Column == null)
                return 0;

            var hitObjectContainer = Column.HitObjectContainer;

            // If we're scrolling downwards, a position of 0 is actually further away from the hit target
            // so we need to flip the vertical coordinate in the hitobject container's space
            var hitObjectPos = Column.HitObjectContainer.ToLocalSpace(applyPositionOffset(screenSpacePosition, false)).Y;
            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                hitObjectPos = hitObjectContainer.DrawHeight - hitObjectPos;

            return scrollingInfo.Algorithm.TimeAt(hitObjectPos,
                EditorClock.CurrentTime,
                scrollingInfo.TimeRange.Value,
                hitObjectContainer.DrawHeight);
        }

        protected float PositionAt(double time)
        {
            var pos = scrollingInfo.Algorithm.PositionAt(time,
                EditorClock.CurrentTime,
                scrollingInfo.TimeRange.Value,
                Column.HitObjectContainer.DrawHeight);

            return applyPositionOffset(Column.HitObjectContainer.ToSpaceOfOtherDrawable(new Vector2(0, pos), Parent), true).Y;
        }

        protected Column ColumnAt(Vector2 screenSpacePosition)
            => composer.ColumnAt(applyPositionOffset(screenSpacePosition, false));

        private Vector2 applyPositionOffset(Vector2 position, bool reverse)
        {
            position.Y += (scrollingInfo.Direction.Value == ScrollingDirection.Up && !reverse ? -1 : 1) * NotePiece.NOTE_HEIGHT / 2;
            return position;
        }
    }
}
