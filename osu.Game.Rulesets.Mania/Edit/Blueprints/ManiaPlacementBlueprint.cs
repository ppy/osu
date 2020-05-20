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
using osuTK.Input;

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
        private IScrollingInfo scrollingInfo { get; set; }

        protected ManiaPlacementBlueprint(T hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            if (Column == null)
                return base.OnMouseDown(e);

            HitObject.Column = Column.Index;
            BeginPlacement(true);
            return true;
        }

        public override void UpdatePosition(SnapResult result)
        {
            if (!PlacementActive)
                Column = (result as ManiaSnapResult)?.Column;

            if (Column == null) return;

            SnappedWidth = Column.DrawWidth;

            // Snap to the column
            var parentPos = Parent.ToLocalSpace(Column.ToScreenSpace(new Vector2(Column.DrawWidth / 2, 0)));
            SnappedMousePosition = new Vector2(parentPos.X, Parent.ToLocalSpace(result.ScreenSpacePosition).Y);
        }

        protected float PositionAt(double time)
        {
            var pos = scrollingInfo.Algorithm.PositionAt(time,
                EditorClock.CurrentTime,
                scrollingInfo.TimeRange.Value,
                Column.HitObjectContainer.DrawHeight);

            if (scrollingInfo.Direction.Value == ScrollingDirection.Down)
                pos = Column.HitObjectContainer.DrawHeight - pos;

            return hitObjectToMousePosition(Column.HitObjectContainer.ToSpaceOfOtherDrawable(new Vector2(0, pos), Parent)).Y;
        }

        /// <summary>
        /// Converts a mouse position to a hitobject position.
        /// </summary>
        /// <remarks>
        /// Blueprints are centred on the mouse position, such that the hitobject position is anchored at the top or bottom of the blueprint depending on the scroll direction.
        /// </remarks>
        /// <param name="mousePosition">The mouse position.</param>
        /// <returns>The resulting hitobject position, acnhored at the top or bottom of the blueprint depending on the scroll direction.</returns>
        private Vector2 mouseToHitObjectPosition(Vector2 mousePosition)
        {
            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                    mousePosition.Y -= DefaultNotePiece.NOTE_HEIGHT / 2;
                    break;

                case ScrollingDirection.Down:
                    mousePosition.Y += DefaultNotePiece.NOTE_HEIGHT / 2;
                    break;
            }

            return mousePosition;
        }

        /// <summary>
        /// Converts a hitobject position to a mouse position.
        /// </summary>
        /// <param name="hitObjectPosition">The hitobject position.</param>
        /// <returns>The resulting mouse position, anchored at the centre of the hitobject.</returns>
        private Vector2 hitObjectToMousePosition(Vector2 hitObjectPosition)
        {
            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                    hitObjectPosition.Y += DefaultNotePiece.NOTE_HEIGHT / 2;
                    break;

                case ScrollingDirection.Down:
                    hitObjectPosition.Y -= DefaultNotePiece.NOTE_HEIGHT / 2;
                    break;
            }

            return hitObjectPosition;
        }
    }
}
