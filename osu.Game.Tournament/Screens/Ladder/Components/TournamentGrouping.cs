// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Configuration;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    [Serializable]
    public class TournamentGrouping
    {
        public readonly Bindable<string> Name = new Bindable<string>();
        public readonly Bindable<string> Description = new Bindable<string>();

        public readonly BindableInt BestOf = new BindableInt(9) { Default = 9, MinValue = 3, MaxValue = 23 };

        [JsonProperty]
        public readonly List<GroupingBeatmap> Beatmaps = new List<GroupingBeatmap>();

        public readonly Bindable<DateTimeOffset> StartDate = new Bindable<DateTimeOffset>();

        public List<int> Pairings = new List<int>();

        public override string ToString() => Name.Value ?? "None";
    }
}
