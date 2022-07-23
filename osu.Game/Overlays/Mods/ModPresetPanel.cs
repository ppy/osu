// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class ModPresetPanel : ModSelectPanel, IHasCustomTooltip<ModPreset>, IHasContextMenu
    {
        public readonly Live<ModPreset> Preset;

        public override BindableBool Active { get; } = new BindableBool();

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        public ModPresetPanel(Live<ModPreset> preset)
        {
            Preset = preset;

            Title = preset.Value.Name;
            Description = preset.Value.Description;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Orange1;
        }

        #region IHasCustomTooltip

        public ModPreset TooltipContent => Preset.Value;
        public ITooltip<ModPreset> GetCustomTooltip() => new ModPresetTooltip(ColourProvider);

        #endregion

        #region IHasContextMenu

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => dialogOverlay?.Push(new DeleteModPresetDialog(Preset)))
        };

        #endregion
    }
}
