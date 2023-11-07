// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit
{
    public static class EditorTimestampParser
    {
        // 00:00:000 (1,2,3) - test
        // regex from https://github.com/ppy/osu-web/blob/651a9bac2b60d031edd7e33b8073a469bf11edaa/resources/assets/coffee/_classes/beatmap-discussion-helper.coffee#L10
        public static readonly Regex TIME_REGEX = new Regex(@"\b(((\d{2,}):([0-5]\d)[:.](\d{3}))(\s\((?:\d+[,|])*\d+\))?)");

        public static string[] GetRegexGroups(string timestamp)
        {
            Match match = TIME_REGEX.Match(timestamp);
            string[] result = match.Success
                ? match.Groups.Values.Where(x => x is not Match && !x.Value.Contains(':')).Select(x => x.Value).ToArray()
                : Array.Empty<string>();
            return result;
        }

        public static double GetTotalMilliseconds(params string[] timesGroup)
        {
            int[] times = timesGroup.Select(int.Parse).ToArray();

            Debug.Assert(times.Length == 3);

            return (times[0] * 60 + times[1]) * 1_000 + times[2];
        }

        public static List<HitObject> GetSelectedHitObjects(HitObjectComposer composer, IReadOnlyList<HitObject> editorHitObjects, string objectsGroup, double position)
        {
            List<HitObject> hitObjects = editorHitObjects.Where(x => x.StartTime >= position).ToList();
            List<HitObject> selectedObjects = new List<HitObject>();

            string[] objectsToSelect = objectsGroup.Split(composer.ObjectSeparator).ToArray();

            foreach (string objectInfo in objectsToSelect)
            {
                HitObject? current = hitObjects.FirstOrDefault(x => composer.HandleHitObjectSelection(x, objectInfo));

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
    }
}
