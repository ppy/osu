// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.SelectV2
{
    public class CsItemInfo
    {
        public required string Id { get; set; }
        public required string DisplayName { get; set; }
        public float? CsValue { get; set; }
        public bool IsDefault { get; set; }
    }

    public static class CsItemIds
    {
        private const int mania_ruleset_id = 3;

        public static readonly List<CsItemInfo> ALL = new List<CsItemInfo>
        {
            new CsItemInfo { Id = "All", DisplayName = "All", IsDefault = true },
            new CsItemInfo { Id = "CS1", DisplayName = "1", CsValue = 1 },
            new CsItemInfo { Id = "CS2", DisplayName = "2", CsValue = 2 },
            new CsItemInfo { Id = "CS3", DisplayName = "3", CsValue = 3 },
            new CsItemInfo { Id = "CS4", DisplayName = "4", CsValue = 4 },
            new CsItemInfo { Id = "CS5", DisplayName = "5", CsValue = 5 },
            new CsItemInfo { Id = "CS6", DisplayName = "6", CsValue = 6 },
            new CsItemInfo { Id = "CS7", DisplayName = "7", CsValue = 7 },
            new CsItemInfo { Id = "CS8", DisplayName = "8", CsValue = 8 },
            new CsItemInfo { Id = "CS9", DisplayName = "9", CsValue = 9 },
            new CsItemInfo { Id = "CS10", DisplayName = "10", CsValue = 10 },
            new CsItemInfo { Id = "CS12", DisplayName = "12", CsValue = 12 },
            new CsItemInfo { Id = "CS14", DisplayName = "14", CsValue = 14 },
            new CsItemInfo { Id = "CS16", DisplayName = "16", CsValue = 16 },
            new CsItemInfo { Id = "CS18", DisplayName = "18", CsValue = 18 },
        };

        public static List<CsItemInfo> GetModesForRuleset(int rulesetId)
        {
            if (rulesetId == mania_ruleset_id)
                return ALL.Where(m => m.CsValue == null || m.CsValue >= 4).ToList();

            return ALL.Where(m => m.CsValue == null || m.CsValue <= 10).ToList();
        }

        public static CsItemInfo? GetById(string id) => ALL.FirstOrDefault(m => m.Id == id);
    }

    public class CircleSizeFilter
    {
        public HashSet<string> SelectedModeIds { get; } = new HashSet<string> { "All" };

        public event Action? SelectionChanged;

        public void SetSelection(HashSet<string> modeIds)
        {
            var newSet = new HashSet<string>();
            if (modeIds.Count == 0 || modeIds.Contains("All"))
                newSet.Add("All");
            else
                newSet.UnionWith(modeIds);

            SelectedModeIds.Clear();
            SelectedModeIds.UnionWith(newSet);
            SelectionChanged?.Invoke();
        }
    }
}
