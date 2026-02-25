// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osu.Game.Users;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerMatchSongSelect : SongSelect, IOnlinePlaySubScreen
    {
        public string ShortTitle => "song selection";

        public override string Title => ShortTitle.Humanize();

        public override bool AllowEditing => false;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private OngoingOperationTracker operationTracker { get; set; } = null!;

        private readonly Room room;
        private readonly IBindable<bool> operationInProgress = new Bindable<bool>();
        private readonly PlaylistItem? itemToEdit;

        private LoadingLayer loadingLayer = null!;
        private IDisposable? selectionOperation;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        protected override UserActivity InitialActivity => new UserActivity.InLobby(room);

        protected readonly Bindable<IReadOnlyList<Mod>> FreeMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private readonly Bindable<bool> freestyle = new Bindable<bool>(true);

        private readonly PlaylistItem? initialItem;
        private readonly FreeModSelectOverlay freeModSelect;
        private FooterButton freeModsFooterButton = null!;

        private IDisposable? freeModSelectOverlayRegistration;

        /// <summary>
        /// Construct a new instance of multiplayer song select.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="itemToEdit">The item to be edited. May be null, in which case a new item will be added to the playlist.</param>
        public MultiplayerMatchSongSelect(Room room, PlaylistItem? itemToEdit = null)
        {
            this.room = room;
            this.itemToEdit = itemToEdit;
            initialItem = itemToEdit ?? room.Playlist.LastOrDefault();

            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };

            freeModSelect = new FreeModSelectOverlay
            {
                SelectedMods = { BindTarget = FreeMods },
                IsValidMod = isValidAllowedMod,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LeftArea.Padding = new MarginPadding { Top = Header.HEIGHT };

            LoadComponent(freeModSelect);
            AddInternal(loadingLayer = new LoadingLayer(true));
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

                freestyle.Value = initialItem.Freestyle;
            }

            Mods.BindValueChanged(_ => updateValidMods());
            Ruleset.BindValueChanged(onRulesetChanged);
            freestyle.BindValueChanged(onFreestyleChanged);

            freeModSelectOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(freeModSelect);

            updateFooterButtons();
            updateValidMods();

            operationInProgress.BindTo(operationTracker.InProgress);
            operationInProgress.BindValueChanged(operation =>
            {
                if (operation.NewValue)
                    loadingLayer.Show();
                else
                    loadingLayer.Hide();
            }, true);
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        private void onFreestyleChanged(ValueChangedEvent<bool> enabled)
        {
            updateFooterButtons();
            updateValidMods();

            if (enabled.NewValue)
            {
                // Freestyle allows all mods to be selected as freemods. This does not play nicely for some components:
                // - We probably don't want to store a gigantic list of acronyms to the database.
                // - The mod select overlay isn't built to handle duplicate mods/mods from all rulesets being shoved into it.
                // Instead, freestyle inherently assumes this list is empty, and must be empty for server-side validation to pass.
                FreeMods.Value = [];
            }
            else
            {
                // When disabling freestyle, enable freemods by default.
                FreeMods.Value = freeModSelect.AllAvailableMods.Where(state => state.ValidForSelection.Value).Select(state => state.Mod).ToArray();
            }
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            // Todo: We can probably attempt to preserve across rulesets like the global mods do.
            FreeMods.Value = [];
        }

        private void updateFooterButtons()
        {
            if (freestyle.Value)
            {
                freeModsFooterButton.Enabled.Value = false;
                freeModSelect.Hide();
            }
            else
                freeModsFooterButton.Enabled.Value = true;
        }

        /// <summary>
        /// Removes invalid mods from <see cref="OsuScreen.Mods"/> and <see cref="FreeMods"/>,
        /// and updates mod selection overlays to display the new mods valid for selection.
        /// </summary>
        private void updateValidMods()
        {
            Mod[] validMods = Mods.Value.Where(isValidRequiredMod).ToArray();
            if (!validMods.SequenceEqual(Mods.Value))
                Mods.Value = validMods;

            Mod[] validFreeMods = FreeMods.Value.Where(isValidAllowedMod).ToArray();
            if (!validFreeMods.SequenceEqual(FreeMods.Value))
                FreeMods.Value = validFreeMods;

            ModSelect.IsValidMod = isValidRequiredMod;
            freeModSelect.IsValidMod = isValidAllowedMod;
        }

        protected sealed override bool OnStart()
        {
            var item = new PlaylistItem(Beatmap.Value.BeatmapInfo)
            {
                RulesetID = Ruleset.Value.OnlineID,
                RequiredMods = Mods.Value.Select(m => new APIMod(m)).ToArray(),
                AllowedMods = FreeMods.Value.Select(m => new APIMod(m)).ToArray(),
                Freestyle = freestyle.Value
            };

            return selectItem(item);
        }

        private bool selectItem(PlaylistItem item)
        {
            if (operationInProgress.Value)
            {
                Logger.Log($"{nameof(selectItem)} aborted due to {nameof(operationInProgress)}");
                return false;
            }

            // If the client is already in a room, update via the client.
            // Otherwise, update the playlist directly in preparation for it to be submitted to the API on match creation.
            if (client.Room != null)
            {
                selectionOperation = operationTracker.BeginOperation();

                var multiplayerItem = new MultiplayerPlaylistItem
                {
                    ID = itemToEdit?.ID ?? 0,
                    BeatmapID = item.Beatmap.OnlineID,
                    BeatmapChecksum = item.Beatmap.MD5Hash,
                    RulesetID = item.RulesetID,
                    RequiredMods = item.RequiredMods.ToArray(),
                    AllowedMods = item.AllowedMods.ToArray(),
                    Freestyle = item.Freestyle
                };

                Task task = itemToEdit != null ? client.EditPlaylistItem(multiplayerItem) : client.AddPlaylistItem(multiplayerItem);

                task.FireAndForget(onSuccess: () =>
                {
                    selectionOperation.Dispose();

                    Schedule(() =>
                    {
                        // If an error or server side trigger occurred this screen may have already exited by external means.
                        if (this.IsCurrentScreen())
                            this.Exit();
                    });
                }, onError: _ =>
                {
                    selectionOperation.Dispose();

                    Schedule(() =>
                    {
                        Carousel.AllowSelection = true;
                    });
                });
            }
            else
            {
                room.Playlist = [item];
                this.Exit();
            }

            return true;
        }

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
            IsValidMod = isValidRequiredMod
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
                    Freestyle = { BindTarget = freestyle }
                }, null),
                (new FooterButtonFreestyle
                {
                    Freestyle = { BindTarget = freestyle }
                }, null)
            });

            return baseButtons;
        }

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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            freeModSelectOverlayRegistration?.Dispose();
        }
    }
}
