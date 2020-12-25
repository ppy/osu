// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Online.API;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    public class MultiplayerRoomSettings : IEquatable<MultiplayerRoomSettings>
    {
        public int BeatmapID { get; set; }

        public int RulesetID { get; set; }

        public string BeatmapChecksum { get; set; } = string.Empty;

        public string Name { get; set; } = "Unnamed room";

        [NotNull]
        public IEnumerable<APIMod> Mods { get; set; } = Enumerable.Empty<APIMod>();

        public bool Equals(MultiplayerRoomSettings other)
            => BeatmapID == other.BeatmapID
               && BeatmapChecksum == other.BeatmapChecksum
               && Mods.SequenceEqual(other.Mods)
               && RulesetID == other.RulesetID
               && Name.Equals(other.Name, StringComparison.Ordinal);

        public override string ToString() => $"Name:{Name} Beatmap:{BeatmapID} ({BeatmapChecksum}) Mods:{string.Join(',', Mods)} Ruleset:{RulesetID}";
    }
}
