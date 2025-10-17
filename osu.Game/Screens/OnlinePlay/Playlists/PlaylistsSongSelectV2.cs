// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.SelectV2;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsSongSelectV2 : SongSelect
    {
        private readonly Bindable<IReadOnlyList<Mod>> freeMods = new Bindable<IReadOnlyList<Mod>>([]);
        private readonly Bindable<bool> freestyle = new Bindable<bool>(true);

        private readonly Room room;
        private ModSelectOverlay modSelect = null!;
        private FreeModSelectOverlay freeModSelect = null!;

        public PlaylistsSongSelectV2(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(freeModSelect = new FreeModSelectOverlay
            {
                SelectedMods = { BindTarget = freeMods },
                IsValidMod = isValidAllowedMod,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Mods.BindValueChanged(onGlobalModsChanged);
            Ruleset.BindValueChanged(onRulesetChanged);
            freestyle.BindValueChanged(onFreestyleChanged);

            updateValidMods();
        }

        private void onGlobalModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            updateValidMods();
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            // Todo: We can probably attempt to preserve across rulesets like the global mods do.
            freeMods.Value = [];
        }

        private void onFreestyleChanged(ValueChangedEvent<bool> enabled)
        {
            updateValidMods();

            if (enabled.NewValue)
            {
                freeModSelect.Hide();

                // Freestyle allows all mods to be selected as freemods. This does not play nicely for some components:
                // - We probably don't want to store a gigantic list of acronyms to the database.
                // - The mod select overlay isn't built to handle duplicate mods/mods from all rulesets being shoved into it.
                // Instead, freestyle inherently assumes this list is empty, and must be empty for server-side validation to pass.
                freeMods.Value = [];
            }
            else
            {
                // When disabling freestyle, enable freemods by default.
                freeMods.Value = freeModSelect.AllAvailableMods.Where(state => state.ValidForSelection.Value).Select(state => state.Mod).ToArray();
            }
        }

        /// <summary>
        /// Removes invalid mods from <see cref="OsuScreen.Mods"/> and <see cref="freeMods"/>,
        /// and updates mod selection overlays to display the new mods valid for selection.
        /// </summary>
        private void updateValidMods()
        {
            Mod[] validMods = Mods.Value.Where(isValidRequiredMod).ToArray();
            if (!validMods.SequenceEqual(Mods.Value))
                Mods.Value = validMods;

            Mod[] validFreeMods = freeMods.Value.Where(isValidAllowedMod).ToArray();
            if (!validFreeMods.SequenceEqual(freeMods.Value))
                freeMods.Value = validFreeMods;

            modSelect.IsValidMod = isValidRequiredMod;
            freeModSelect.IsValidMod = isValidAllowedMod;
        }

        protected override void OnStart()
        {
            room.Playlist = [createNewItem()];
            this.Exit();
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons()
        {
            var buttons = base.CreateFooterButtons().ToList();

            buttons.Single(i => i is FooterButtonMods).TooltipText = MultiplayerMatchStrings.RequiredModsButtonTooltip;

            buttons.Insert(0, new FooterButtonPlaylistV2(room)
            {
                CreateNewItem = () => room.Playlist = room.Playlist.Append(createNewItem()).ToArray()
            });

            buttons.InsertRange(buttons.FindIndex(b => b is FooterButtonMods) + 1,
            [
                new FooterButtonFreeModsV2(freeModSelect)
                {
                    FreeMods = { BindTarget = freeMods },
                    Freestyle = { BindTarget = freestyle }
                },
                new FooterButtonFreestyleV2
                {
                    Freestyle = { BindTarget = freestyle }
                }
            ]);

            return buttons;
        }

        protected override ModSelectOverlay CreateModSelectOverlay() => modSelect = new UserModSelectOverlay(OverlayColourScheme.Plum)
        {
            IsValidMod = isValidRequiredMod
        };

        private PlaylistItem createNewItem() => new PlaylistItem(Beatmap.Value.BeatmapInfo)
        {
            ID = room.Playlist.Count == 0 ? 0 : room.Playlist.Max(p => p.ID) + 1,
            RulesetID = Ruleset.Value.OnlineID,
            RequiredMods = Mods.Value.Select(m => new APIMod(m)).ToArray(),
            AllowedMods = freeMods.Value.Select(m => new APIMod(m)).ToArray(),
            Freestyle = freestyle.Value
        };

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid to be selected as a required mod.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        private bool isValidRequiredMod(Mod mod) => ModUtils.IsValidModForMatch(mod, true, room.Type, freestyle.Value);

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid to be selected as an allowed mod.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        private bool isValidAllowedMod(Mod mod) => ModUtils.IsValidModForMatch(mod, false, room.Type, freestyle.Value)
                                                   // Mod must not be contained in the required mods.
                                                   && Mods.Value.All(m => m.Acronym != mod.Acronym)
                                                   // Mod must be compatible with all the required mods.
                                                   && ModUtils.CheckCompatibleSet(Mods.Value.Append(mod).ToArray());
    }
}
