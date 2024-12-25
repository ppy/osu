// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Audio
{
    [Cached]
    public partial class HitSoundTrackSamplePointBlueprintContainer : TimelinePart<HitSoundTrackSamplePointBlueprint>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public readonly HitSoundTrackMode Mode;

        public HitSoundTrackSamplePointBlueprintContainer(HitSoundTrackMode mode)
        {
            Mode = mode;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            editorBeatmap.HitObjectRemoved += removeHitObjectFromTrack;
            editorBeatmap.HitObjectAdded += addHitObjectToTrack;
            editorBeatmap.HitObjectUpdated += (HitObject hitObject) =>
            {
                if (hitObject is IHasRepeats || hitObject is IHasDuration)
                {
                    removeHitObjectFromTrack(hitObject);
                    addHitObjectToTrack(hitObject);
                }
            };

            List<HitSoundTrackSamplePointBlueprint> objects = [];

            editorBeatmap.HitObjects.ForEach(addHitObjectToTrack);

            AddRange(objects);
        }

        private void removeHitObjectFromTrack(HitObject hitObject)
        {
            Children.Where(v =>
            {
                if (v is HitSoundTrackSamplePointBlueprint samplePoint)
                    return samplePoint.HitObject == hitObject;
                return false;
            }).ForEach(part => part.Expire());
        }

        private void addHitObjectToTrack(HitObject hitObject)
        {
            if (hitObject is IHasRepeats || hitObject is IHasDuration)
            {
                if (hitObject is IHasDuration)
                    Add(new ExtendableHitSoundTrackSamplePointBlueprint(hitObject));

                if (hitObject is IHasRepeats repeatedHitObject)
                    for (int i = 0; i < repeatedHitObject.NodeSamples.Count; i++)
                    {
                        Add(new NodeHitSoundTrackSamplePointBlueprint(hitObject, repeatedHitObject, i));
                    }
            }
            else
                Add(new HitSoundTrackSamplePointBlueprint(hitObject));
        }
    }
}
