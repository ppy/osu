// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public partial class ModPresetPanel : ModSelectPanel, IHasCustomTooltip<ModPreset>, IHasContextMenu, IHasPopover
    {
        public readonly Live<ModPreset> Preset;

        public override BindableBool Active { get; } = new BindableBool();

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        private ModSettingChangeTracker? settingChangeTracker;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(_ => selectedModsChanged(), true);
        }

        protected override void Select()
        {
            // if the preset is not active at the point of the user click, then set the mods using the preset directly, discarding any previous selections,
            // which will also have the side effect of activating the preset (see `updateActiveState()`).
            selectedMods.Value = Preset.Value.Mods.ToArray();
        }

        protected override void Deselect()
        {
            // if the preset is active when the user has clicked it, then it means that the set of active mods is exactly equal to the set of mods in the preset
            // (there are no other active mods than what the preset specifies, and the mod settings match exactly).
            // therefore it's safe to just clear selected mods, since it will have the effect of toggling the preset off.
            selectedMods.Value = Array.Empty<Mod>();
        }

        private void selectedModsChanged()
        {
            settingChangeTracker?.Dispose();
            settingChangeTracker = new ModSettingChangeTracker(selectedMods.Value);
            settingChangeTracker.SettingChanged = _ => updateActiveState();
            updateActiveState();
        }

        private void updateActiveState()
        {
            Active.Value = new HashSet<Mod>(Preset.Value.Mods).SetEquals(selectedMods.Value);
        }

        #region Filtering support

        public override IEnumerable<LocalisableString> FilterTerms => getFilterTerms();

        private IEnumerable<LocalisableString> getFilterTerms()
        {
            var preset = Preset.Value;

            yield return preset.Name;
            yield return preset.Description;

            foreach (Mod mod in preset.Mods)
            {
                yield return mod.Name;
                yield return mod.Acronym;
                yield return mod.Description;
            }
        }

        #endregion

        #region IHasCustomTooltip

        public ModPreset TooltipContent => Preset.Value;
        public ITooltip<ModPreset> GetCustomTooltip() => new ModPresetTooltip(ColourProvider);

        #endregion

        #region IHasContextMenu

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(CommonStrings.ButtonsEdit, MenuItemType.Highlighted, this.ShowPopover),
            new OsuMenuItem(CommonStrings.ButtonsDelete, MenuItemType.Destructive, () => dialogOverlay?.Push(new DeleteModPresetDialog(Preset))),
        };

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            settingChangeTracker?.Dispose();
        }

        public Popover GetPopover() => new EditPresetPopover(Preset);
    }
}
