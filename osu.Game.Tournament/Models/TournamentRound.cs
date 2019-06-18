// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    [Serializable]
    public class TournamentRound
    {
        public readonly Bindable<string> Name = new Bindable<string>();
        public readonly Bindable<string> Description = new Bindable<string>();

        public readonly BindableInt BestOf = new BindableInt(9) { Default = 9, MinValue = 3, MaxValue = 23 };

        [JsonProperty]
        public readonly List<RoundBeatmap> Beatmaps = new List<RoundBeatmap>();

        public readonly Bindable<DateTimeOffset> StartDate = new Bindable<DateTimeOffset>();

        // only used for serialisation
        public List<int> Pairings = new List<int>();

        public override string ToString() => Name.Value ?? "None";
    }
}
