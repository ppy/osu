// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MultiplayerUserModSelectOverlay : UserModSelectOverlay
    {
        private const double flush_debounce_time = 500;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;

        public MultiplayerUserModSelectOverlay()
            : base(OverlayColourScheme.Plum)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.UserStyleChanged += onUserStyleChanged;
            client.UserModsChanged += onUserModsChanged;
            client.ItemChanged += onPlaylistItemChanged;
            client.SettingsChanged += onSettingsChanged;

            SelectedMods.BindValueChanged(onSelectedModsChanged);

            updateFromOnlineState();
        }

        /// <summary>
        /// The last selected playlist item.
        /// </summary>
        private long lastPlaylistItemId;

        /// <summary>
        /// The current debounced <see cref="flushPendingChanges"/> operation resulting from a change in a mod's settings.
        /// </summary>
        private ScheduledDelegate? scheduledFlush;

        /// <summary>
        /// The number of pending changes to the local selection that have not yet been processed by the server.
        /// </summary>
        private int countPendingChanges;

        /// <summary>
        /// Responds to changes in the local user's style to take on the server-side state.
        /// </summary>
        private void onUserStyleChanged(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            if (user.Equals(client.LocalUser))
                updateFromOnlineState();
        });

        /// <summary>
        /// Responds to changes in the local user's mods to take on the server-side state.
        /// </summary>
        private void onUserModsChanged(MultiplayerRoomUser user) => Scheduler.Add(() =>
        {
            if (user.Equals(client.LocalUser))
                updateFromOnlineState();
        });

        /// <summary>
        /// Responds to changes in the current playlist item to take on the server-side state,
        /// potentially also changing the visible mod panels depending on the new item state.
        /// </summary>
        private void onPlaylistItemChanged(MultiplayerPlaylistItem item) => Scheduler.Add(() =>
        {
            if (item.ID == client.Room?.Settings.PlaylistItemId)
                updateFromOnlineState();
        });

        /// <summary>
        /// Responds to changes in the current playlist item to take on the server-side state,
        /// potentially also changing the visible mod panels depending on the new item state.
        /// </summary>
        private void onSettingsChanged(MultiplayerRoomSettings settings) => Scheduler.Add(() =>
        {
            if (settings.PlaylistItemId != lastPlaylistItemId)
            {
                updateFromOnlineState();
                lastPlaylistItemId = settings.PlaylistItemId;
            }
        });

        /// <summary>
        /// Tracks changes to the local selected mods to notify the server of any changes.
        /// </summary>
        private void onSelectedModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            // Process changes to the selection immediately because these occur somewhat infrequently.
            flushPendingChanges();

            modSettingChangeTracker?.Dispose();
            modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
            modSettingChangeTracker.SettingChanged += _ =>
            {
                // Debounce changes to mod settings because these can be continuous (e.g. moving a slider bar).
                scheduledFlush ??= Scheduler.AddDelayed(flushPendingChanges, flush_debounce_time);
            };
        }

        /// <summary>
        /// Notifies the server of changes to the selected mods.
        /// </summary>
        private void flushPendingChanges()
        {
            scheduledFlush?.Cancel();
            scheduledFlush = null;

            if (client.Room == null)
                return;

            countPendingChanges++;

            client.ChangeUserMods(SelectedMods.Value)
                  .FireAndForget(endChange, _ => endChange());

            void endChange() => Scheduler.Add(() =>
            {
                countPendingChanges--;
                updateFromOnlineState();
            });
        }

        /// <summary>
        /// Updates the selected mods from the online state if the server is authoritative.
        /// </summary>
        private void updateFromOnlineState()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            // Check if there are any pending changes, in which case we should prefer the local state.
            if (countPendingChanges > 0 || scheduledFlush != null)
                return;

            MultiplayerPlaylistItem currentItem = client.Room.CurrentPlaylistItem;
            Ruleset ruleset = rulesets.GetRuleset(client.LocalUser.RulesetId ?? currentItem.RulesetID)!.CreateInstance();
            Mod[] allowedMods = currentItem.Freestyle
                ? ruleset.AllMods.OfType<Mod>().Where(m => ModUtils.IsValidFreeModForMatchType(m, client.Room.Settings.MatchType)).ToArray()
                : currentItem.AllowedMods.Select(m => m.ToMod(ruleset)).ToArray();

            // Update the mod panels to reflect the ones which are valid for selection.
            IsValidMod = allowedMods.Length > 0
                ? m => allowedMods.Any(a => a.GetType() == m.GetType())
                : _ => false;

            // Update the selection to reflect the server's current state.
            Mod[] userMods = client.LocalUser.Mods.Select(m => m.ToMod(ruleset)).ToArray();

            if (!userMods.SequenceEqual(SelectedMods.Value))
            {
                SelectedMods.ValueChanged -= onSelectedModsChanged;
                SelectedMods.Value = userMods;
                SelectedMods.ValueChanged += onSelectedModsChanged;
            }

            ActiveMods.Value = ComputeActiveMods();
        }

        protected override IReadOnlyList<Mod> ComputeActiveMods()
        {
            if (client.Room == null || client.LocalUser == null)
                return [];

            MultiplayerPlaylistItem currentItem = client.Room.CurrentPlaylistItem;
            Ruleset ruleset = rulesets.GetRuleset(currentItem.RulesetID)!.CreateInstance();

            return currentItem.RequiredMods.Select(m => m.ToMod(ruleset)).Concat(base.ComputeActiveMods()).ToArray();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.UserStyleChanged -= onUserStyleChanged;
                client.UserModsChanged -= onUserModsChanged;
                client.ItemChanged -= onPlaylistItemChanged;
                client.SettingsChanged -= onSettingsChanged;
            }

            modSettingChangeTracker?.Dispose();
        }
    }
}
