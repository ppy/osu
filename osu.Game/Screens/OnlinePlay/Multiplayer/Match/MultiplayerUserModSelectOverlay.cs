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
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerUserModSelectOverlay : RoomModSelectOverlay
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;
        private ScheduledDelegate? debouncedModSettingsUpdate;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            IsValidMod = _ => false;

            client.RoomUpdated += onRoomUpdated;

            SelectedItem.BindValueChanged(_ => updateSpecifics());
            SelectedMods.BindValueChanged(_ => updateSpecifics());
            SelectedMods.BindValueChanged(onSelectedModsChanged);
        }

        private void onRoomUpdated()
        {
            if (client.Room == null)
                return;

            SelectedItem.Value = new PlaylistItem(client.Room.CurrentPlaylistItem);
        }

        private void onSelectedModsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingChangeTracker?.Dispose();

            if (client.Room == null)
                return;

            client.ChangeUserMods(mods.NewValue).FireAndForget();

            modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
            modSettingChangeTracker.SettingChanged += _ =>
            {
                // Debounce changes to mod settings so as to not thrash the network.
                debouncedModSettingsUpdate?.Cancel();
                debouncedModSettingsUpdate = Scheduler.AddDelayed(() =>
                {
                    if (client.Room == null)
                        return;

                    client.ChangeUserMods(SelectedMods.Value).FireAndForget();
                }, 500);
            };
        }

        private void updateSpecifics()
        {
            if (client.Room == null || client.LocalUser == null)
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

            // Remove any mods that are no longer allowed.
            Mod[] newUserMods = SelectedMods.Value.Where(m => allowedMods.Any(a => m.GetType() == a.GetType())).ToArray();
            if (!newUserMods.SequenceEqual(SelectedMods.Value))
                SelectedMods.Value = newUserMods;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;

            modSettingChangeTracker?.Dispose();
        }
    }
}
