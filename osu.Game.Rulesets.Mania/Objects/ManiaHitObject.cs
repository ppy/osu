// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Mania.Objects.Types;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Objects
{
    public abstract class ManiaHitObject : HitObject, IHasColumn
    {
        public readonly Bindable<int> ColumnBindable = new Bindable<int>();

        public virtual int Column
        {
            get => ColumnBindable.Value;
            set => ColumnBindable.Value = value;
        }

        protected override HitWindows CreateHitWindows() => new ManiaHitWindows();
    }
}
