// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Configuration;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public partial class CursorRippleVisualiser : CompositeDrawable
    {
        private readonly Bindable<bool> showRipples = new Bindable<bool>(true);

        [BackgroundDependencyLoader(true)]
        private void load(OsuRulesetConfigManager rulesetConfig)
        {
            rulesetConfig?.BindWith(OsuRulesetSetting.ShowCursorTrail, showRipples);
        }
    }
}
