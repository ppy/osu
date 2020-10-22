using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    public class SpectatorState : IEquatable<SpectatorState>
    {
        public int? BeatmapID { get; set; }

        [NotNull]
        public IEnumerable<Mod> Mods { get; set; } = Enumerable.Empty<Mod>();

        public SpectatorState(int? beatmapId = null, IEnumerable<Mod> mods = null)
        {
            BeatmapID = beatmapId;
            if (mods != null)
                Mods = mods;
        }

        public SpectatorState()
        {
        }

        public bool Equals(SpectatorState other) => this.BeatmapID == other?.BeatmapID && this.Mods.SequenceEqual(other?.Mods);

        public override string ToString() => $"Beatmap:{BeatmapID} Mods:{string.Join(',', Mods.SelectMany(m => m.Acronym))}";
    }
}
