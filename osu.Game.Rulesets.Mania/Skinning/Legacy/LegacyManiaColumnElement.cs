// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    /// <summary>
    /// A <see cref="CompositeDrawable"/> which is placed somewhere within a <see cref="Column"/>.
    /// </summary>
    public partial class LegacyManiaColumnElement : CompositeDrawable
    {
        [Resolved]
        protected Column Column { get; private set; } = null!;

        [Resolved]
        private StageDefinition stage { get; set; } = null!;

        /// <summary>
        /// The column type identifier to use for texture lookups, in the case of no user-provided configuration.
        /// </summary>
        protected string FallbackColumnIndex { get; private set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (Column.IsSpecial)
                FallbackColumnIndex = "S";
            else
            {
                // Account for cases like dual-stage (assume that all stages have the same column count for now).
                int columnInStage = Column.Index % stage.Columns;
                int distanceToEdge = Math.Min(columnInStage, (stage.Columns - 1) - columnInStage);
                FallbackColumnIndex = distanceToEdge % 2 == 0 ? "1" : "2";
            }
        }

        protected IBindable<T>? GetColumnSkinConfig<T>(ISkin skin, LegacyManiaSkinConfigurationLookups lookup) where T : notnull
            => skin.GetManiaSkinConfig<T>(lookup, Column.Index);
    }
}
