// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mods;
using osu.Framework.Input.Events;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Display the specified mod at a fixed size.
    /// </summary>
    public partial class ClickableModIcon : ModIcon
    {
        public Action? Action;

        public ClickableModIcon(IMod mod, bool showTooltip = true, bool showExtendedInformation = true)
            : base(mod, showTooltip, showExtendedInformation)
        {
        }

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return true;
        }
    }
}
