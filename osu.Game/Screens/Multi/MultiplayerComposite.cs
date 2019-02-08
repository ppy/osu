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
        [Resolved(typeof(Room))]
        protected Bindable<int?> RoomID { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<string> Name { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<User> Host { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<RoomStatus> Status { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<GameType> Type { get; private set; }

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<IEnumerable<User>> Participants { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<int> ParticipantCount { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<int?> MaxParticipants { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<DateTimeOffset> EndDate { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<RoomAvailability> Availability { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<TimeSpan> Duration { get; private set; }

        private readonly Bindable<BeatmapInfo> currentBeatmap = new Bindable<BeatmapInfo>();
        protected IBindable<BeatmapInfo> CurrentBeatmap => currentBeatmap;

        private readonly Bindable<IEnumerable<Mod>> currentMods = new Bindable<IEnumerable<Mod>>();
        protected IBindable<IEnumerable<Mod>> CurrentMods => currentMods;

        private readonly Bindable<RulesetInfo> currentRuleset = new Bindable<RulesetInfo>();
        protected IBindable<RulesetInfo> CurrentRuleset => currentRuleset;

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
