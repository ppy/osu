// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osu.Game.Users;
using osu.Game.Utils;
using osu.Game.Localisation;

namespace osu.Game.Screens.OnlinePlay
{
    public abstract partial class OnlinePlaySongSelect : SongSelect, IOnlinePlaySubScreen
    {
        public string ShortTitle => "song selection";

        public override string Title => ShortTitle.Humanize();

        public override bool AllowEditing => false;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        protected override UserActivity InitialActivity => new UserActivity.InLobby(room);

        protected readonly Bindable<IReadOnlyList<Mod>> FreeMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());
        protected readonly Bindable<bool> Freestyle = new Bindable<bool>(true);

        private readonly Room room;
        private readonly PlaylistItem? initialItem;
        private readonly FreeModSelectOverlay freeModSelect;
        private FooterButton freeModsFooterButton = null!;

        private IDisposable? freeModSelectOverlayRegistration;

        /// <summary>
        /// Creates a new <see cref="OnlinePlaySongSelect"/>.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="initialItem">An optional initial <see cref="PlaylistItem"/> to use for the initial beatmap/ruleset/mods.
        /// If <c>null</c>, the last <see cref="PlaylistItem"/> in the room will be used.</param>
        protected OnlinePlaySongSelect(Room room, PlaylistItem? initialItem = null)
        {
            this.room = room;
            this.initialItem = initialItem ?? room.Playlist.LastOrDefault();

            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };

            freeModSelect = new FreeModSelectOverlay
            {
                SelectedMods = { BindTarget = FreeMods },
                IsValidMod = isValidFreeMod,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LeftArea.Padding = new MarginPadding { Top = Header.HEIGHT };
            LoadComponent(freeModSelect);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (initialItem != null)
            {
                // Prefer using a local databased beatmap lookup since OnlineId may be -1 for an invalid beatmap selection.
                BeatmapInfo? beatmapInfo = initialItem.Beatmap as BeatmapInfo;

                // And in the case that this isn't a local databased beatmap, query by online ID.
                if (beatmapInfo == null)
                {
                    int onlineId = initialItem.Beatmap.OnlineID;
                    beatmapInfo = beatmapManager.QueryBeatmap(b => b.OnlineID == onlineId);
                }

                if (beatmapInfo != null)
                    Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmapInfo);

                RulesetInfo? ruleset = rulesets.GetRuleset(initialItem.RulesetID);

                if (ruleset != null)
                {
                    Ruleset.Value = ruleset;

                    var rulesetInstance = ruleset.CreateInstance();
                    Debug.Assert(rulesetInstance != null);

                    // At this point, Mods contains both the required and allowed mods. For selection purposes, it should only contain the required mods.
                    // Similarly, freeMods is currently empty but should only contain the allowed mods.
                    Mods.Value = initialItem.RequiredMods.Select(m => m.ToMod(rulesetInstance)).ToArray();
                    FreeMods.Value = initialItem.AllowedMods.Select(m => m.ToMod(rulesetInstance)).ToArray();
                }

                Freestyle.Value = initialItem.Freestyle;
            }

            Mods.BindValueChanged(onModsChanged);
            Ruleset.BindValueChanged(onRulesetChanged);
            Freestyle.BindValueChanged(onFreestyleChanged, true);

            freeModSelectOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(freeModSelect);
        }

        private void onFreestyleChanged(ValueChangedEvent<bool> enabled)
        {
            if (enabled.NewValue)
            {
                freeModsFooterButton.Enabled.Value = false;
                freeModsFooterButton.Enabled.Value = false;
                ModsFooterButton.Enabled.Value = false;

                ModSelect.Hide();
                freeModSelect.Hide();

                Mods.Value = [];
                FreeMods.Value = [];
            }
            else
            {
                freeModsFooterButton.Enabled.Value = true;
                ModsFooterButton.Enabled.Value = true;
            }
        }

        private void onModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            FreeMods.Value = FreeMods.Value.Where(isValidFreeMod).ToList();

            // Reset the validity delegate to update the overlay's display.
            freeModSelect.IsValidMod = isValidFreeMod;
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            FreeMods.Value = Array.Empty<Mod>();
        }

        protected sealed override bool OnStart()
        {
            var item = new PlaylistItem(Beatmap.Value.BeatmapInfo)
            {
                RulesetID = Ruleset.Value.OnlineID,
                RequiredMods = Mods.Value.Select(m => new APIMod(m)).ToArray(),
                AllowedMods = FreeMods.Value.Select(m => new APIMod(m)).ToArray(),
                Freestyle = Freestyle.Value
            };

            return SelectItem(item);
        }

        /// <summary>
        /// Invoked when the user has requested a selection of a beatmap.
        /// </summary>
        /// <param name="item">The resultant <see cref="PlaylistItem"/>. This item has not yet been added to the <see cref="Room"/>'s.</param>
        /// <returns><c>true</c> if a selection occurred.</returns>
        protected abstract bool SelectItem(PlaylistItem item);

        public override bool OnBackButton()
        {
            if (freeModSelect.State.Value == Visibility.Visible)
            {
                freeModSelect.Hide();
                return true;
            }

            return base.OnBackButton();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            freeModSelect.Hide();
            return base.OnExiting(e);
        }

        protected override ModSelectOverlay CreateModSelectOverlay() => new UserModSelectOverlay(OverlayColourScheme.Plum)
        {
            IsValidMod = isValidMod
        };

        protected override IEnumerable<(FooterButton button, OverlayContainer? overlay)> CreateSongSelectFooterButtons()
        {
            var baseButtons = base.CreateSongSelectFooterButtons().ToList();

            baseButtons.Single(i => i.button is FooterButtonMods).button.TooltipText = MultiplayerMatchStrings.RequiredModsButtonTooltip;

            baseButtons.InsertRange(baseButtons.FindIndex(b => b.button is FooterButtonMods) + 1, new (FooterButton, OverlayContainer?)[]
            {
                (freeModsFooterButton = new FooterButtonFreeMods(freeModSelect)
                {
                    FreeMods = { BindTarget = FreeMods },
                    Freestyle = { BindTarget = Freestyle }
                }, null),
                (new FooterButtonFreestyle
                {
                    Freestyle = { BindTarget = Freestyle }
                }, null)
            });

            return baseButtons;
        }

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid for global selection.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        /// <returns>Whether <paramref name="mod"/> is a valid mod for online play.</returns>
        private bool isValidMod(Mod mod) => ModUtils.IsValidModForMatchType(mod, room.Type);

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid for per-player free-mod selection.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        /// <returns>Whether <paramref name="mod"/> is a selectable free-mod.</returns>
        private bool isValidFreeMod(Mod mod) => ModUtils.IsValidFreeModForMatchType(mod, room.Type)
                                                // Mod must not be contained in the required mods.
                                                && Mods.Value.All(m => m.Acronym != mod.Acronym)
                                                // Mod must be compatible with all the required mods.
                                                && ModUtils.CheckCompatibleSet(Mods.Value.Append(mod).ToArray());

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            freeModSelectOverlayRegistration?.Dispose();
        }
    }
}
