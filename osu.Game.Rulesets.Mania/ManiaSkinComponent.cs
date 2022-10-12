// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSkinComponent : GameplaySkinComponent<ManiaSkinComponents>
    {
        /// <summary>
        /// Creates a new <see cref="ManiaSkinComponent"/>.
        /// </summary>
        /// <param name="component">The component.</param>
        public ManiaSkinComponent(ManiaSkinComponents component)
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
    }
}
