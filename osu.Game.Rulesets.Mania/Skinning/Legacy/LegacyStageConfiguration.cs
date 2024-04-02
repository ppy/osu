// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyStageConfiguration : Drawable
    {
        [Resolved]
        private ManiaPlayfield playfield { get; set; } = null!;

        public LegacyStageConfiguration()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            playfield.StageContainer.X = (skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.ColumnStart)?.Value ?? 0) / 1024;
        }

        public bool IsToolboxVisible => false;
    }
}
