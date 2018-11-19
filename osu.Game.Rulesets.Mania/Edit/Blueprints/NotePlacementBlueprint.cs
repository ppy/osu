// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class NotePlacementBlueprint : ManiaPlacementBlueprint<Note>
    {
        public NotePlacementBlueprint()
            : base(new Note())
        {
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Y;

            InternalChild = new EditNotePiece { RelativeSizeAxes = Axes.X };
        }

        protected override void Update()
        {
            base.Update();

            Position = SnappedMousePosition;
        }

        protected override bool OnClick(ClickEvent e)
        {
            Column column;
            if ((column = ColumnAt(e.ScreenSpaceMousePosition)) == null)
                return base.OnClick(e);

            HitObject.StartTime = TimeAt(e.ScreenSpaceMousePosition);
            HitObject.Column = column.Index;

            EndPlacement();

            return true;
        }
    }
}
