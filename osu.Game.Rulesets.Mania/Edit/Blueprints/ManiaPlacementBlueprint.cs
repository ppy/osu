// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osuTK.Input;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public abstract class ManiaPlacementBlueprint<T> : PlacementBlueprint
        where T : ManiaHitObject
    {
        protected new T HitObject => (T)base.HitObject;

        private Column column;

        public Column Column
        {
            get => column;
            set
            {
                if (value == column)
                    return;

                column = value;
                HitObject.Column = column.Index;
            }
        }

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
                return false;

            BeginPlacement(true);
            return true;
        }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            if (!PlacementActive)
                Column = result.Playfield as Column;
        }
    }
}
