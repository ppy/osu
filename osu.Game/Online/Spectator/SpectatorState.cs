// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MessagePack;
using osu.Game.Online.API;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    [MessagePackObject]
    public class SpectatorState : IEquatable<SpectatorState>
    {
        [Key(0)]
        public int? BeatmapID { get; set; }

        [Key(1)]
        public int? RulesetID { get; set; }

        [NotNull]
        [Key(2)]
        public IEnumerable<APIMod> Mods { get; set; } = Enumerable.Empty<APIMod>();

        public bool Equals(SpectatorState other) => BeatmapID == other?.BeatmapID && Mods.SequenceEqual(other?.Mods) && RulesetID == other?.RulesetID;

        public override string ToString() => $"Beatmap:{BeatmapID} Mods:{string.Join(',', Mods)} Ruleset:{RulesetID}";
    }
}
