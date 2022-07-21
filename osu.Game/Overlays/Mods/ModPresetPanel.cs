// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class ModPresetPanel : ModSelectOverlayPanel
    {
        public override BindableBool Active { get; } = new BindableBool();

        public ModPresetPanel(ModPreset preset)
        {
            Title = preset.Name;
            Description = preset.Description;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Orange1;
        }
    }
}
