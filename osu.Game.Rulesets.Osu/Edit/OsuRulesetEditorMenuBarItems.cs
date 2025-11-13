// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuRulesetEditorMenuBarItems : RulesetEditorMenuBarItems
    {
        private readonly BindableBool showCursor = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(IRulesetConfigCache configCache)
        {
            var config = configCache.GetConfigFor(new OsuRuleset()) as OsuRulesetConfigManager;

            config?.BindWith(OsuRulesetSetting.EditorShowGameplayCursor, showCursor);
        }

        public override IEnumerable<MenuItem> CreateMenuItems(EditorMenuBarItemType type)
        {
            switch (type)
            {
                case EditorMenuBarItemType.View:
                    yield return new ToggleMenuItem("Show gameplay cursor")
                    {
                        State = { BindTarget = showCursor }
                    };

                    break;
            }
        }
    }
}
