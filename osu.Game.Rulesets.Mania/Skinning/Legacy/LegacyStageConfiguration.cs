// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyStageConfiguration : Drawable
    {
        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        public LegacyStageConfiguration()
        {
            RelativeSizeAxes = Axes.Y;
            RelativePositionAxes = Axes.X;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Position = new Vector2(skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.ColumnStart)?.Value ?? 0, 0) / 1024;
        }
    }
}
