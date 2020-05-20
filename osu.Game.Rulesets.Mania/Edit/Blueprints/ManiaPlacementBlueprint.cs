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
