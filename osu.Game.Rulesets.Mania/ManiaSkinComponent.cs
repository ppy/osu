// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSkinComponent : GameplaySkinComponent<ManiaSkinComponents>
    {
        /// <summary>
        /// The intended <see cref="StageDefinition"/> for this component.
        /// May be null if the component is not a direct member of a <see cref="Stage"/>.
        /// </summary>
        public readonly StageDefinition? StageDefinition;

        /// <summary>
        /// Creates a new <see cref="ManiaSkinComponent"/>.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="stageDefinition">The intended <see cref="StageDefinition"/> for this component. May be null if the component is not a direct member of a <see cref="Stage"/>.</param>
        public ManiaSkinComponent(ManiaSkinComponents component, StageDefinition? stageDefinition = null)
            : base(component)
        {
            StageDefinition = stageDefinition;
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
