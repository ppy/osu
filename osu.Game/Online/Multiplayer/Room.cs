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

        [JsonIgnore]
        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [JsonProperty("name")]
        public readonly Bindable<string> Name = new Bindable<string>("My awesome room!");

        [JsonProperty("host")]
        public readonly Bindable<User> Host = new Bindable<User>();

        public bool ShouldSerializeHost() => false;

        [JsonProperty("playlist")]
        public readonly BindableCollection<PlaylistItem> Playlist = new BindableCollection<PlaylistItem>();

        [JsonProperty("duration")]
        public readonly Bindable<int> Duration = new Bindable<int>(100);

        [JsonProperty("max_attempts")]
        public readonly Bindable<int?> MaxAttempts = new Bindable<int?>(null);

        public Bindable<RoomStatus> Status = new Bindable<RoomStatus>(new RoomStatusOpen());
        public Bindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();
        public Bindable<GameType> Type = new Bindable<GameType>(new GameTypeTimeshift());
        public Bindable<int?> MaxParticipants = new Bindable<int?>();
        public Bindable<IEnumerable<User>> Participants = new Bindable<IEnumerable<User>>(Enumerable.Empty<User>());

        public Room()
        {
            Beatmap.BindValueChanged(b =>
            {
                Playlist.Clear();
                Playlist.Add(new PlaylistItem { Beatmap = b });
            });
        }

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
        }
    }

    public class PlaylistItem
    {
        [JsonProperty("beatmap")]
        private APIBeatmap beatmap { get; set; }

        public bool ShouldSerializebeatmap() => false;

        [JsonIgnore]
        public BeatmapInfo Beatmap { get; set; }

        [JsonProperty("beatmap_id")]
        public int BeatmapID => 847296; //Beatmap.OnlineBeatmapID ?? 0;

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

        private RulesetInfo ruleset;

        [JsonIgnore]
        public RulesetInfo Ruleset
        {
            get => ruleset;
            set
            {
                ruleset = value;

                if (_allowedMods != null)
                {
                    AllowedMods.Clear();
                    AllowedMods.AddRange(value.CreateInstance().GetAllMods().Where(mod => _allowedMods.Any(m => m.Acronym == mod.Acronym)));

                    _allowedMods = null;
                }

                if (_requiredMods != null)
                {
                    RequiredMods.Clear();
                    RequiredMods.AddRange(value.CreateInstance().GetAllMods().Where(mod => _requiredMods.Any(m => m.Acronym == mod.Acronym)));

                    _requiredMods = null;
                }
            }
        }

        public void SetRulesets(RulesetStore rulesets)
        {
            Beatmap = beatmap.ToBeatmap(rulesets);
        }

        // Todo: Move this elsewhere for reusability
        private class APIMod : IMod
        {
            public string Acronym { get; set; }
        }
    }
}
