// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerUserModSelectOverlay : UserModSelectOverlay
    {
        private const double update_debounce_time = 500;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;
        private ScheduledDelegate? scheduledMakeServerAuthoritive;

        private int clientAuthorityTokenCount;
        private bool hasPendingChanges;
        private double lastChangeTime;
        private long lastPlaylistItemId;

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

        protected override void Update()
        {
            base.Update();
            processPendingChanges();
        }

        /// <summary>
        /// Whether this overlay has complete authority over the selected mods.
        /// When <c>true</c>, any server-side state changes to <see cref="MultiplayerRoomUser.Mods"/> <b>will not</b> be applied to the selected mods.
        /// </summary>
        private bool isClientAuthoritative => hasPendingChanges || clientAuthorityTokenCount > 0;

        /// <summary>
        /// Responds to changes in the local user's style to process server-authoritative changes.
        /// </summary>
        private void onUserStyleChanged(MultiplayerRoomUser user)
        {
            if (user.Equals(client.LocalUser))
                updateFromOnlineState();
        }

        /// <summary>
        /// Responds to changes in the local user's mods to process server-authoritative changes.
        /// </summary>
        private void onUserModsChanged(MultiplayerRoomUser user)
        {
            if (user.Equals(client.LocalUser))
                updateFromOnlineState();
        }

        /// <summary>
        /// Responds to changes in the currently-selected playlist item to process changes to <see cref="MultiplayerPlaylistItem.BeatmapID"/>,
        /// <see cref="MultiplayerPlaylistItem.RulesetID"/>, <see cref="MultiplayerPlaylistItem.AllowedMods"/>, or <see cref="MultiplayerPlaylistItem.Freestyle"/>.
        /// </summary>
        private void onPlaylistItemChanged(MultiplayerPlaylistItem item)
        {
            if (item.ID == client.Room?.Settings.PlaylistItemId)
                updateFromOnlineState();
        }

        /// <summary>
        /// Responds to changes in the active playlist item that could occur as a result of it being deleted or the room's queue mode having been changed.
        /// </summary>
        private void onSettingsChanged(MultiplayerRoomSettings settings)
        {
            if (settings.PlaylistItemId != lastPlaylistItemId)
            {
                updateFromOnlineState();
                lastPlaylistItemId = settings.PlaylistItemId;
            }
        }

        /// <summary>
        /// Tracks any changes to the selected mods.
        /// </summary>
        private void onSelectedModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            hasPendingChanges = true;
            modSettingChangeTracker?.Dispose();
            modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
            modSettingChangeTracker.SettingChanged += _ => hasPendingChanges = true;
        }

        /// <summary>
        /// Processes changes to the selected mods to notify the server of any changes at a debounced rate.
        /// </summary>
        private void processPendingChanges()
        {
            if (!hasPendingChanges)
                return;

            if (Time.Current - lastChangeTime < update_debounce_time)
                return;

            IDisposable clientAuthority = createClientAuthorityToken();

            client.ChangeUserMods(SelectedMods.Value)
                  .FireAndForget()
                  .ContinueWith(_ => clientAuthority.Dispose());

            hasPendingChanges = false;
            lastChangeTime = Time.Current;
        }

        /// <summary>
        /// Creates a token that causes this overlay to take complete authority over all changes to the selected mods,
        /// preventing server-side changes from being applied (<see cref="isClientAuthoritative"/>).
        /// </summary>
        /// <returns>
        /// The token, which should be disposed in order to restore the server's authority.
        /// </returns>
        private IDisposable createClientAuthorityToken()
        {
            clientAuthorityTokenCount++;

            ScheduledDelegate restoreDelegate = new ScheduledDelegate(() =>
            {
                clientAuthorityTokenCount--;
                updateFromOnlineState();
            });

            scheduledMakeServerAuthoritive?.Cancel();
            scheduledMakeServerAuthoritive = restoreDelegate;

            return new InvokeOnDisposal(() => Scheduler.Add(restoreDelegate));
        }

        /// <summary>
        /// Updates the selected mods from the online state if the server is authoritative.
        /// </summary>
        private void updateFromOnlineState()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            // Check if client is authoritative due to a pending change - either one that hasn't yet completed or a new one waiting to be sent across.
            if (isClientAuthoritative)
                return;

            MultiplayerPlaylistItem currentItem = client.Room.Playlist.Single(item => item.ID == client.Room.Settings.PlaylistItemId);
            Ruleset ruleset = rulesets.GetRuleset(client.LocalUser.RulesetId ?? currentItem.RulesetID)!.CreateInstance();

            // Suppose the host changed the room in some way such that some mods are no longer valid...
            // We can assume that the online state will reflect the valid mods that remain, but we have to re-filter the buttons.
            Mod[] allowedMods = currentItem.Freestyle
                ? ruleset.AllMods.OfType<Mod>().Where(m => ModUtils.IsValidFreeModForMatchType(m, client.Room.Settings.MatchType)).ToArray()
                : currentItem.AllowedMods.Select(m => m.ToMod(ruleset)).ToArray();
            IsValidMod = allowedMods.Length > 0
                ? m => allowedMods.Any(a => a.GetType() == m.GetType())
                : _ => false;

            // Update the local state from the online one.
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

            MultiplayerPlaylistItem currentItem = client.Room.Playlist.Single(item => item.ID == client.Room.Settings.PlaylistItemId);
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
