// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using MessagePack;
using osu.Game.Online.API;

namespace osu.Game.Online.Multiplayer.Queueing
{
    public class EnqueuePlaylistItemRequest : MatchUserRequest
    {
        [Key(0)]
        public int BeatmapID { get; set; }

        [Key(1)]
        public int RulesetID { get; set; }

        [Key(2)]
        public string BeatmapChecksum { get; set; } = string.Empty;

        [Key(3)]
        public IEnumerable<APIMod> RequiredMods { get; set; } = Enumerable.Empty<APIMod>();

        [Key(4)]
        public IEnumerable<APIMod> AllowedMods { get; set; } = Enumerable.Empty<APIMod>();
    }
}
