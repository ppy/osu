// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit
{
    public static class EditorTimestampParser
    {
        private static readonly Regex timestamp_regex = new Regex(@"^(\d+:\d+:\d+)(?: \((\d+(?:[|,]\d+)*)\))?$", RegexOptions.Compiled);

        public static string[] GetRegexGroups(string timestamp)
        {
            Match match = timestamp_regex.Match(timestamp);
            return match.Success
                ? match.Groups.Values.Where(x => x is not Match).Select(x => x.Value).ToArray()
                : Array.Empty<string>();
        }

        public static double GetTotalMilliseconds(string timeGroup)
        {
            int[] times = timeGroup.Split(':').Select(int.Parse).ToArray();

            Debug.Assert(times.Length == 3);

            return (times[0] * 60 + times[1]) * 1_000 + times[2];
        }

        public static List<HitObject> GetSelectedHitObjects(IReadOnlyList<HitObject> editorHitObjects, string objectsGroup, double position)
        {
            List<HitObject> hitObjects = editorHitObjects.Where(x => x.StartTime >= position).ToList();
            List<HitObject> selectedObjects = new List<HitObject>();

            string[] objectsToSelect = objectsGroup.Split(',').ToArray();

            foreach (string objectInfo in objectsToSelect)
            {
                HitObject? current = hitObjects.FirstOrDefault(x => shouldHitObjectBeSelected(x, objectInfo));

                if (current == null)
                    continue;

                selectedObjects.Add(current);
                hitObjects = hitObjects.Where(x => x != current && x.StartTime >= current.StartTime).ToList();
            }

            // Stable behavior
            // - always selects the next closest object when `objectsGroup` only has one (combo) item
            if (objectsToSelect.Length != 1 || objectsGroup.Contains('|'))
                return selectedObjects;

            HitObject? nextClosest = editorHitObjects.FirstOrDefault(x => x.StartTime >= position);
            if (nextClosest == null)
                return selectedObjects;

            if (nextClosest.StartTime <= (selectedObjects.FirstOrDefault()?.StartTime ?? position))
            {
                selectedObjects.Clear();
                selectedObjects.Add(nextClosest);
            }

            return selectedObjects;
        }

        private static bool shouldHitObjectBeSelected(HitObject hitObject, string objectInfo)
        {
            switch (hitObject)
            {
                // (combo)
                case IHasComboInformation comboInfo:
                {
                    if (!double.TryParse(objectInfo, out double comboValue) || comboValue < 1)
                        return false;

                    return comboInfo.IndexInCurrentCombo + 1 == comboValue;
                }

                // (time|column)
                case IHasColumn column:
                {
                    double[] split = objectInfo.Split('|').Select(double.Parse).ToArray();
                    if (split.Length != 2)
                        return false;

                    double timeValue = split[0];
                    double columnValue = split[1];
                    return hitObject.StartTime == timeValue && column.Column == columnValue;
                }

                default:
                    return false;
            }
        }
    }
}
