// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class ManiaPlacementBlueprint<T> : PlacementBlueprint
        where T : ManiaHitObject
    {
        protected new T HitObject => (T)base.HitObject;

        [Resolved]
        private ManiaHitObjectComposer composer { get; set; }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        public ManiaPlacementBlueprint(T hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;
        }

        protected double TimeAt(Vector2 screenSpacePosition)
        {
            var column = ColumnAt(screenSpacePosition);
            if (column == null)
                return 0;

            return scrollingInfo.Algorithm.TimeAt(column.HitObjectContainer.ToLocalSpace(applyPositionOffset(screenSpacePosition)).Y,
                EditorClock.CurrentTime,
                scrollingInfo.TimeRange.Value,
                column.HitObjectContainer.DrawHeight);
        }

        protected Column ColumnAt(Vector2 screenSpacePosition)
            => composer.ColumnAt(applyPositionOffset(screenSpacePosition));

        private Vector2 applyPositionOffset(Vector2 position)
        {
            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                    position.Y -= DrawHeight / 2;
                    break;
                case ScrollingDirection.Down:
                    position.Y += DrawHeight / 2;
                    break;
            }

            return position;
        }
    }
}
