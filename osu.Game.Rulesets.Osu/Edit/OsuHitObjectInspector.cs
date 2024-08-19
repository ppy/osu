// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuHitObjectInspector : HitObjectInspector
    {
        protected override void AddInspectorValues(HitObject[] objects)
        {
            base.AddInspectorValues(objects);

            if (objects.Length > 0)
            {
                var firstInSelection = (OsuHitObject)objects.MinBy(ho => ho.StartTime)!;
                var lastInSelection = (OsuHitObject)objects.MaxBy(ho => ho.GetEndTime())!;

                Debug.Assert(firstInSelection != null && lastInSelection != null);

                var precedingObject = (OsuHitObject?)EditorBeatmap.HitObjects.LastOrDefault(ho => ho.GetEndTime() < firstInSelection.StartTime);
                var nextObject = (OsuHitObject?)EditorBeatmap.HitObjects.FirstOrDefault(ho => ho.StartTime > lastInSelection.GetEndTime());

                if (precedingObject != null && precedingObject is not Spinner)
                {
                    AddHeader("To previous");
                    AddValue($"{(firstInSelection.StackedPosition - precedingObject.StackedEndPosition).Length:#,0.##}px");
                }

                if (nextObject != null && nextObject is not Spinner)
                {
                    AddHeader("To next");
                    AddValue($"{(nextObject.StackedPosition - lastInSelection.StackedEndPosition).Length:#,0.##}px");
                }
            }
        }
    }
}
