// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.Rooms
{
    public class PlaylistItem : IEquatable<PlaylistItem>
    {
        [JsonProperty("id")]
        public long ID { get; set; }

        [JsonProperty("owner_id")]
        public int OwnerID { get; set; }

        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonProperty("ruleset_id")]
        public int RulesetID { get; set; }

        /// <summary>
        /// Whether this <see cref="PlaylistItem"/> is still a valid selection for the <see cref="Room"/>.
        /// </summary>
        [JsonProperty("expired")]
        public bool Expired { get; set; }

        [JsonProperty("playlist_order")]
        public ushort? PlaylistOrder { get; set; }

        [JsonProperty("played_at")]
        public DateTimeOffset? PlayedAt { get; set; }

        [JsonIgnore]
        public IBindable<bool> Valid => valid;

        private readonly Bindable<bool> valid = new BindableBool(true);

        [JsonIgnore]
        public readonly Bindable<IBeatmapInfo> Beatmap = new Bindable<IBeatmapInfo>();

        [JsonProperty("beatmap")]
        private APIBeatmap apiBeatmap { get; set; }

        [JsonProperty("allowed_mods")]
        public APIMod[] AllowedMods { get; set; } = Array.Empty<APIMod>();

        [JsonProperty("required_mods")]
        public APIMod[] RequiredMods { get; set; } = Array.Empty<APIMod>();

        public PlaylistItem()
        {
            Beatmap.BindValueChanged(beatmap => BeatmapID = beatmap.NewValue?.OnlineID ?? -1);
        }

        public void MarkInvalid() => valid.Value = false;

        public void MapObjects()
        {
            Beatmap.Value ??= apiBeatmap;
        }

        #region Newtonsoft.Json implicit ShouldSerialize() methods

        // The properties in this region are used implicitly by Newtonsoft.Json to not serialise certain fields in some cases.
        // They rely on being named exactly the same as the corresponding fields (casing included) and as such should NOT be renamed
        // unless the fields are also renamed.

        [UsedImplicitly]
        public bool ShouldSerializeID() => false;

        // ReSharper disable once IdentifierTypo
        [UsedImplicitly]
        public bool ShouldSerializeapiBeatmap() => false;

        #endregion

        public bool Equals(PlaylistItem other)
            => ID == other?.ID
               && BeatmapID == other.BeatmapID
               && RulesetID == other.RulesetID
               && Expired == other.Expired
               && AllowedMods.SequenceEqual(other.AllowedMods)
               && RequiredMods.SequenceEqual(other.RequiredMods);
    }
}
