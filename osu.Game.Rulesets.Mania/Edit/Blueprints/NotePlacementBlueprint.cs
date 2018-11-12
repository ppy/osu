// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class NotePlacementBlueprint : PlacementBlueprint
    {
        protected new Note HitObject => (Note)base.HitObject;

        [Resolved]
        private ManiaHitObjectComposer composer { get; set; }

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        public NotePlacementBlueprint()
            : base(new Note())
        {
            RelativeSizeAxes = Axes.None;

            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Y;
            Width = 45;

            InternalChild = new EditNotePiece { RelativeSizeAxes = Axes.X };
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            Position = e.MousePosition;
            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            var offsetPosition = e.ScreenSpaceMousePosition;
            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                    offsetPosition.Y -= DrawHeight / 2;
                    break;
                case ScrollingDirection.Down:
                    offsetPosition.Y += DrawHeight / 2;
                    break;
            }

            var column = composer.ColumnAt(offsetPosition);
            if (column == null)
                return base.OnClick(e);

            var hitObjectContainer = column.HitObjectContainer;

            HitObject.StartTime = scrollingInfo.Algorithm.TimeAt(hitObjectContainer.ToLocalSpace(offsetPosition).Y,
                EditorClock.CurrentTime,
                scrollingInfo.TimeRange.Value,
                hitObjectContainer.DrawHeight);

            HitObject.Column = column.Index;

            EndPlacement();

            return true;
        }
    }
}
