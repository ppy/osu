// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class LegacyManiaColumnElement : CompositeDrawable
    {
        [Resolved]
        protected Column Column { get; private set; }

        /// <summary>
        /// The column type identifier to use for texture lookups, in the case of no user-provided configuration.
        /// </summary>
        protected string FallbackColumnIndex { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            switch (Column.ColumnType)
            {
                case ColumnType.Special:
                    FallbackColumnIndex = "S";
                    break;

                case ColumnType.Odd:
                    FallbackColumnIndex = "1";
                    break;

                case ColumnType.Even:
                    FallbackColumnIndex = "2";
                    break;
            }
        }

        protected IBindable<T> GetColumnSkinConfig<T>(ISkin skin, LegacyManiaSkinConfigurationLookups lookup)
            => skin.GetManiaSkinConfig<T>(lookup, Column.Index);
    }
}
