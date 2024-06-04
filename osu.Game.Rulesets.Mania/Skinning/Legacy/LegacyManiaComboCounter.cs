// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyManiaComboCounter : LegacyBaseComboCounter
    {
        public override BindableBool ShouldRoll { get; } = new BindableBool(false);

        public LegacyManiaComboCounter()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Margin = new MarginPadding(0);

            Scale = new Vector2(1.3f);
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            float? comboPosition = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.ScorePosition)?.Value;

            if (comboPosition != null)
                comboPosition -= Stage.HIT_TARGET_POSITION + 306;

            Y = comboPosition ?? 0;
        }
    }
}
