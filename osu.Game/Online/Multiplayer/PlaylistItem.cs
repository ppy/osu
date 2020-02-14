// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.Multiplayer
{
    public class PlaylistItem : IEquatable<PlaylistItem>
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; set; }

        [JsonIgnore]
        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        [JsonIgnore]
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        [JsonIgnore]
        public readonly BindableList<Mod> AllowedMods = new BindableList<Mod>();

        [JsonIgnore]
        public readonly BindableList<Mod> RequiredMods = new BindableList<Mod>();

        [JsonProperty("beatmap")]
        private APIBeatmap apiBeatmap { get; set; }

        private APIMod[] allowedModsBacking;

        [JsonProperty("allowed_mods")]
        private APIMod[] allowedMods
        {
            get => AllowedMods.Select(m => new APIMod(m)).ToArray();
            set => allowedModsBacking = value;
        }

        private APIMod[] requiredModsBacking;

        [JsonProperty("required_mods")]
        private APIMod[] requiredMods
        {
            get => RequiredMods.Select(m => new APIMod(m)).ToArray();
            set => requiredModsBacking = value;
        }

        public PlaylistItem()
        {
            Beatmap.BindValueChanged(beatmap => BeatmapID = beatmap.NewValue?.OnlineBeatmapID ?? 0);
            Ruleset.BindValueChanged(ruleset => RulesetID = ruleset.NewValue?.ID ?? 0);
        }

        public void MapObjects(BeatmapManager beatmaps, RulesetStore rulesets)
        {
            // If we don't have an api beatmap, the request occurred as a result of room creation, so we can query the local beatmap instead
            // Todo: Is this a bug? Room creation only returns the beatmap ID
            Beatmap.Value = apiBeatmap == null ? beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == BeatmapID) : apiBeatmap.ToBeatmap(rulesets);
            Ruleset.Value = rulesets.GetRuleset(RulesetID);

            Ruleset rulesetInstance = Ruleset.Value.CreateInstance();

            if (allowedModsBacking != null)
            {
                AllowedMods.Clear();
                AllowedMods.AddRange(allowedModsBacking.Select(m => m.ToMod(rulesetInstance)));

                allowedModsBacking = null;
            }

            if (requiredModsBacking != null)
            {
                RequiredMods.Clear();
                RequiredMods.AddRange(requiredModsBacking.Select(m => m.ToMod(rulesetInstance)));

                requiredModsBacking = null;
            }
        }

        public bool ShouldSerializeID() => false;
        public bool ShouldSerializeapiBeatmap() => false;

        public bool Equals(PlaylistItem other) => ID == other?.ID && BeatmapID == other.BeatmapID && RulesetID == other.RulesetID;

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return HashCode.Combine(ID, BeatmapID, RulesetID);
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }
}
