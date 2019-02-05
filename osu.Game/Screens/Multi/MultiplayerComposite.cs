// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Game.Screens.Multi
{
    public class MultiplayerComposite : CompositeDrawable
    {
        [Resolved]
        public Room Room { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<int?> RoomID { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<string> Name { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<User> Host { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<RoomStatus> Status { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<GameType> Type { get; private set; }

        [Resolved(typeof(Room))]
        public BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<IEnumerable<User>> Participants { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<int> ParticipantCount { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<int?> MaxParticipants { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<DateTimeOffset> EndDate { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<RoomAvailability> Availability { get; private set; }

        [Resolved(typeof(Room))]
        public Bindable<TimeSpan> Duration { get; private set; }

        private readonly Bindable<BeatmapInfo> currentBeatmap = new Bindable<BeatmapInfo>();
        public IBindable<BeatmapInfo> CurrentBeatmap => currentBeatmap;

        private readonly Bindable<IEnumerable<Mod>> currentMods = new Bindable<IEnumerable<Mod>>();
        public IBindable<IEnumerable<Mod>> CurrentMods => currentMods;

        private readonly Bindable<RulesetInfo> currentRuleset = new Bindable<RulesetInfo>();
        public IBindable<RulesetInfo> CurrentRuleset => currentRuleset;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playlist.ItemsAdded += _ => updatePlaylist();
            Playlist.ItemsRemoved += _ => updatePlaylist();

            updatePlaylist();
        }

        private void updatePlaylist()
        {
            // Todo: We only ever have one playlist item for now. In the future, this will be user-settable

            var playlistItem = Playlist.FirstOrDefault();

            currentBeatmap.Value = playlistItem?.Beatmap;
            currentMods.Value = playlistItem?.RequiredMods ?? Enumerable.Empty<Mod>();
            currentRuleset.Value = playlistItem?.Ruleset;
        }
    }
}
