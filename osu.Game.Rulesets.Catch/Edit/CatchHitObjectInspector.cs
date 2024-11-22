// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchHitObjectInspector(CatchDistanceSnapProvider snapProvider) : HitObjectInspector
    {
        protected override void AddInspectorValues(HitObject[] objects)
        {
            base.AddInspectorValues(objects);

            if (objects.Length > 0)
            {
                HitObject firstSelectedHitObject = objects.MinBy(ho => ho.StartTime)!;
                HitObject lastSelectedHitObject = objects.MaxBy(ho => ho.GetEndTime())!;

                HitObject? precedingObject = EditorBeatmap.HitObjects.LastOrDefault(ho => ho.GetEndTime() < firstSelectedHitObject.StartTime);
                HitObject? nextObject = EditorBeatmap.HitObjects.FirstOrDefault(ho => ho.StartTime > lastSelectedHitObject.GetEndTime());

                if (precedingObject != null && precedingObject is not BananaShower)
                {
                    double previousSnap = snapProvider.ReadCurrentDistanceSnap(precedingObject, firstSelectedHitObject);
                    AddHeader("To previous");
                    AddValue($"{previousSnap:#,0.##}x");
                }

                if (nextObject != null && nextObject is not BananaShower)
                {
                    double nextSnap = snapProvider.ReadCurrentDistanceSnap(lastSelectedHitObject, nextObject);
                    AddHeader("To next");
                    AddValue($"{nextSnap:#,0.##}x");
                }
            }
        }
    }
}
