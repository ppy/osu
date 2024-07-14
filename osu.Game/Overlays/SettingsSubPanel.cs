// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;

namespace osu.Game.Overlays
{
    public abstract partial class SettingsSubPanel : SettingsPanel
    {
        protected SettingsSubPanel()
            : base(true)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        protected override bool DimMainContent => false; // dimming is handled by main overlay
    }
}
