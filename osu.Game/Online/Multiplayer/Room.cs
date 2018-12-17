// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Game.Online.Multiplayer
{
    public class Room
    {
        [JsonProperty("id")]
        public Bindable<int?> RoomID { get; } = new Bindable<int?>();

        [JsonProperty("name")]
        public readonly Bindable<string> Name = new Bindable<string>("My awesome room!");

        [JsonProperty("host")]
        public readonly Bindable<User> Host = new Bindable<User>();

        public bool ShouldSerializeHost() => false;

        [JsonProperty("playlist")]
        public readonly BindableCollection<PlaylistItem> Playlist = new BindableCollection<PlaylistItem>();

        [JsonIgnore]
        public readonly Bindable<TimeSpan> Duration = new Bindable<TimeSpan>(TimeSpan.FromMinutes(30));

        [JsonIgnore]
        public readonly Bindable<int?> MaxAttempts = new Bindable<int?>();

        [JsonProperty("duration")]
        private int duration
        {
            get => (int)Duration.Value.TotalMinutes;
            set => Duration.Value = TimeSpan.FromMinutes(value);
        }
        // Todo: Find a better way to do this (https://github.com/ppy/osu-framework/issues/1930)
        [JsonProperty("max_attempts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? maxAttempts
        {
            get => MaxAttempts;
            set => MaxAttempts.Value = value;
        }

        public Bindable<RoomStatus> Status = new Bindable<RoomStatus>(new RoomStatusOpen());
        public Bindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();
        public Bindable<GameType> Type = new Bindable<GameType>(new GameTypeTimeshift());
        public Bindable<int?> MaxParticipants = new Bindable<int?>();
        public Bindable<IEnumerable<User>> Participants = new Bindable<IEnumerable<User>>(Enumerable.Empty<User>());

        public void CopyFrom(Room other)
        {
            RoomID.Value = other.RoomID;
            Name.Value = other.Name;
            Host.Value = other.Host;
            Status.Value = other.Status;
            Availability.Value = other.Availability;
            Type.Value = other.Type;
            MaxParticipants.Value = other.MaxParticipants;
            Participants.Value = other.Participants.Value.ToArray();

            Playlist.Clear();
            Playlist.AddRange(other.Playlist);
        }
    }

    public class PlaylistItem
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("beatmap")]
        private APIBeatmap apiBeatmap { get; set; }

        public bool ShouldSerializeapiBeatmap() => false;

        private BeatmapInfo beatmap;

        [JsonIgnore]
        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                beatmap = value;
                BeatmapID = value?.OnlineBeatmapID ?? 0;
            }
        }

        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; set; }

        [JsonIgnore]
        public readonly BindableCollection<Mod> AllowedMods = new BindableCollection<Mod>();

        [JsonIgnore]
        public readonly BindableCollection<Mod> RequiredMods = new BindableCollection<Mod>();

        private APIMod[] _allowedMods;

        [JsonProperty("allowed_mods")]
        private APIMod[] allowedMods
        {
            get => AllowedMods.Select(m => new APIMod { Acronym = m.Acronym }).ToArray();
            set => _allowedMods = value;
        }

        private APIMod[] _requiredMods;

        [JsonProperty("required_mods")]
        private APIMod[] requiredMods
        {
            get => RequiredMods.Select(m => new APIMod { Acronym = m.Acronym }).ToArray();
            set => _requiredMods = value;
        }

        [JsonIgnore]
        public RulesetInfo Ruleset { get; set; }

        public void MapObjects(BeatmapManager beatmaps, RulesetStore rulesets)
        {
            // If we don't have an api beatmap, the request occurred as a result of room creation, so we can query the local beatmap instead
            // Todo: Is this a bug?
            Beatmap = apiBeatmap == null ? beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == BeatmapID) : apiBeatmap.ToBeatmap(rulesets);
            Ruleset = rulesets.GetRuleset(RulesetID);

            if (_allowedMods != null)
            {
                AllowedMods.Clear();
                AllowedMods.AddRange(Ruleset.CreateInstance().GetAllMods().Where(mod => _allowedMods.Any(m => m.Acronym == mod.Acronym)));

                _allowedMods = null;
            }

            if (_requiredMods != null)
            {
                RequiredMods.Clear();
                RequiredMods.AddRange(Ruleset.CreateInstance().GetAllMods().Where(mod => _requiredMods.Any(m => m.Acronym == mod.Acronym)));

                _requiredMods = null;
            }
        }

        // Todo: Move this elsewhere for reusability
        private class APIMod : IMod
        {
            public string Acronym { get; set; }
        }
    }
}
