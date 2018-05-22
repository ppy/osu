// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Edit.Types;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System;

namespace osu.Game.Rulesets.Mania.Objects
{
    public abstract class ManiaHitObject : HitObject, IHasXPosition, IHasEditableColumn
    {
        public event Action<int> ColumnChanged;

        private int column { get; set; }

        public virtual int Column
        {
            get => column;
            set
            {
                if (column == value)
                    return;
                column = value;

                ColumnChanged?.Invoke(value);
            }
        }

        public virtual int Layer { get; set; }

        public virtual float X
        {
            get => Column;
        }

        public virtual void OffsetColumn(int offset) => Column += offset;

        public virtual void OffsetLayer(int offset) => Layer += offset;

        protected override HitWindows CreateHitWindows() => new ManiaHitWindows();
    }
}
