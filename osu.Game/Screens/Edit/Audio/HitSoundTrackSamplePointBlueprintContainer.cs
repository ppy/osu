// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            editorBeatmap.HitObjectUpdated += hitObject =>
            {
                if (hitObject is IHasRepeats hasRepeats)
                {
                    int existsNodeSamplePoints = 0;
                    Children.ForEach(v =>
                    {
                        if (v is not HitSoundTrackSamplePointBlueprint blueprint)
                            return;

                        if (blueprint.HitObject != hitObject)
                            return;

                        if (blueprint is NodeHitSoundTrackSamplePointBlueprint nodeSamplePoint)
                        {
                            if (hasRepeats.RepeatCount < nodeSamplePoint.NodeIndex - 1)
                                nodeSamplePoint.Expire();
                            else
                                nodeSamplePoint.UpdateWidthAndPosition();

                            existsNodeSamplePoints++;
                        }
                    });

                    // hasRepeats.RepeatCount + 2 is because we only add the point between two ends
                    for (int i = existsNodeSamplePoints; i < hasRepeats.RepeatCount + 2; i++)
                        Add(new NodeHitSoundTrackSamplePointBlueprint(hitObject, hasRepeats, i));
                }
            };

            editorBeatmap.HitObjects.ForEach(addHitObjectToTrack);
        }

        private void removeHitObjectFromTrack(HitObject hitObject)
        {
            RemoveAll(v =>
            {
                if (v is HitSoundTrackSamplePointBlueprint samplePoint)
                    return samplePoint.HitObject == hitObject;

                return false;
            }, true);
        }

        private void addHitObjectToTrack(HitObject hitObject)
        {
            if (hitObject is IHasRepeats || hitObject is IHasDuration)
            {
                if (hitObject is IHasDuration)
                {
                    Add(new ExtendableHitSoundTrackSamplePointBlueprint(hitObject));
                }

                if (hitObject is IHasRepeats repeatedHitObject)
                {
                    for (int i = 0; i < repeatedHitObject.NodeSamples.Count; i++)
                    {
                        Add(new NodeHitSoundTrackSamplePointBlueprint(hitObject, repeatedHitObject, i));
                    }
                }
            }
            else
            {
                Add(new HitSoundTrackSamplePointBlueprint(hitObject));
            }
        }
    }
}
