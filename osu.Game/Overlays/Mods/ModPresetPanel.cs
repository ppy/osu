// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class ModPresetPanel : ModSelectPanel, IHasCustomTooltip<ModPreset>
    {
        public readonly ModPreset Preset;

        public override BindableBool Active { get; } = new BindableBool();

        public ModPresetPanel(ModPreset preset)
        {
            Preset = preset;

            Title = preset.Name;
            Description = preset.Description;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Orange1;
        }

        public ModPreset TooltipContent => Preset;
        public ITooltip<ModPreset> GetCustomTooltip() => new ModPresetTooltip(ColourProvider);
    }
}
