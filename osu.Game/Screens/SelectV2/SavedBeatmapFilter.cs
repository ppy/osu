// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;
using Realms;

namespace osu.Game.Screens.SelectV2
{
    public class SavedBeatmapFilter : RealmObject, IHasGuidPrimaryKey
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string SearchQuery { get; set; } = string.Empty;

        public int SortMode { get; set; }
        public int GroupMode { get; set; }

        public bool ShowConverted { get; set; }

        public double MinStars { get; set; }
        public double MaxStars { get; set; }

        public string RulesetShortName { get; set; } = string.Empty;
    }
}
