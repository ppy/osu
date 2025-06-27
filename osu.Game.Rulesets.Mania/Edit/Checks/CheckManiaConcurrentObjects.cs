// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mania.Edit.Checks
{
    public class CheckManiaConcurrentObjects : CheckConcurrentObjects
    {
        public override IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var hitObjects = context.Beatmap.HitObjects;

            for (int i = 0; i < hitObjects.Count - 1; ++i)
            {
                var hitobject = hitObjects[i];

                for (int j = i + 1; j < hitObjects.Count; ++j)
                {
                    var nextHitobject = hitObjects[j];

                    // Mania hitobjects are only considered concurrent if they also share the same column.
                    if ((hitobject as IHasColumn)?.Column != (nextHitobject as IHasColumn)?.Column)
                        continue;

                    // Two hitobjects cannot be concurrent without also being concurrent with all objects in between.
                    // So if the next object is not concurrent, then we know no future objects will be either.
                    if (!AreConcurrent(hitobject, nextHitobject))
                        break;

                    if (hitobject.GetType() == nextHitobject.GetType())
                        yield return new IssueTemplateConcurrentSame(this).Create(hitobject, nextHitobject);
                    else
                        yield return new IssueTemplateConcurrentDifferent(this).Create(hitobject, nextHitobject);
                }
            }
        }
    }
}
