// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSkinComponent : GameplaySkinComponent<ManiaSkinComponents>
    {
        public readonly int TargetColumn;

        public ManiaSkinComponent(ManiaSkinComponents component, int targetColumn)
            : base(component)
        {
            TargetColumn = targetColumn;
        }

        protected override string RulesetPrefix => ManiaRuleset.SHORT_NAME;

        protected override string ComponentName => Component.ToString().ToLower();
    }

    public enum ManiaSkinComponents
    {
        ColumnBackground,
        HitTarget,
        KeyArea,
        Note,
        HoldNoteHead,
        HoldNoteTail,
        HoldNoteBody,
        HitExplosion
    }
}
