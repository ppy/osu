// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSkinComponentLookup : GameplaySkinComponentLookup<ManiaSkinComponents>
    {
        /// <summary>
        /// Creates a new <see cref="ManiaSkinComponentLookup"/>.
        /// </summary>
        /// <param name="component">The component.</param>
        public ManiaSkinComponentLookup(ManiaSkinComponents component)
            : base(component)
        {
        }

        protected override string RulesetPrefix => ManiaRuleset.SHORT_NAME;

        protected override string ComponentName => Component.ToString().ToLowerInvariant();
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
        HitExplosion,
        StageBackground,
        StageForeground,
        BarLine
    }
}
