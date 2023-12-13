// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A tournament round, containing many matches, generally executed in a short time period.
    /// </summary>
    [Serializable]
    public class TournamentRound
    {
        public readonly Bindable<string> Name = new Bindable<string>(string.Empty);
        public readonly Bindable<string> Description = new Bindable<string>(string.Empty);

        public readonly BindableInt BestOf = new BindableInt(9) { Default = 9, MinValue = 3, MaxValue = 23 };
        public readonly BindableInt BanCount = new BindableInt(1) { Default = 1, MinValue = 0, MaxValue = 5 };

        [JsonProperty]
        public readonly BindableList<RoundBeatmap> Beatmaps = new BindableList<RoundBeatmap>();

        public readonly Bindable<DateTimeOffset> StartDate = new Bindable<DateTimeOffset> { Value = DateTimeOffset.UtcNow };

        // only used for serialisation
        public List<int> Matches = new List<int>();

        public override string ToString() => Name.Value ?? "None";
    }
}
