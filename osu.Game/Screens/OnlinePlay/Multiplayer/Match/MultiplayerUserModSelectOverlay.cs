// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
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
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public MultiplayerUserModSelectOverlay()
            : base(OverlayColourScheme.Plum)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;

            updateValidMods();
        }

        private void onRoomUpdated()
        {
            // Importantly, this is not scheduled because the client must not skip intermediate server states to validate the allowed mods.
            updateValidMods();
        }

        private void updateValidMods()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            MultiplayerPlaylistItem currentItem = client.Room.CurrentPlaylistItem;
            Ruleset ruleset = rulesets.GetRuleset(client.LocalUser.RulesetId ?? currentItem.RulesetID)!.CreateInstance();
            Mod[] allowedMods = ModUtils.EnumerateUserSelectableFreeMods(client.Room.Settings.MatchType, currentItem.RequiredMods, currentItem.AllowedMods, currentItem.Freestyle, ruleset);

            // Update the mod panels to reflect the ones which are valid for selection.
            IsValidMod = m => allowedMods.Any(a => a.GetType() == m.GetType());

            // Remove any mods that are no longer allowed.
            Mod[] newUserMods = SelectedMods.Value.Where(m => allowedMods.Any(a => m.GetType() == a.GetType())).ToArray();
            if (!newUserMods.SequenceEqual(SelectedMods.Value))
                SelectedMods.Value = newUserMods;

            // The active mods include the playlist item's required mods which change separately from the selected mods.
            IReadOnlyList<Mod> newActiveMods = ComputeActiveMods();
            if (!newActiveMods.SequenceEqual(ActiveMods.Value))
                ActiveMods.Value = newActiveMods;
        }

        protected override IReadOnlyList<Mod> ComputeActiveMods()
        {
            if (client.Room == null || client.LocalUser == null)
                return [];

            MultiplayerPlaylistItem currentItem = client.Room.CurrentPlaylistItem;
            Ruleset ruleset = rulesets.GetRuleset(client.LocalUser.RulesetId ?? currentItem.RulesetID)!.CreateInstance();
            return currentItem.RequiredMods.Select(m => m.ToMod(ruleset)).Concat(base.ComputeActiveMods()).ToArray();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
