// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Audio
{
    public enum HitSoundTrackMode
    {
        Sample,
        NormalBank,
        AdditionBank,
        Volume,
    }

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
            editorBeatmap.HitObjectUpdated += updateHitObjectFromTrack;

            editorBeatmap.HitObjects.ForEach(addHitObjectToTrack);
        }

        private void addHitObjectToTrack(HitObject hitObject)
        {
            IList<HitSampleInfo> samples = hitObject.SamplesBindable;

            if (hitObject is IHasRepeats repeatedHitObject)
            {
                if (Mode == HitSoundTrackMode.Volume)
                    Add(new HitSoundTrackSamplePointVolumeControlBlueprint(hitObject, samples));
                else
                    Add(new HitSoundTrackSamplePointBlueprint(hitObject, samples));

                for (int nodeIndex = 0; nodeIndex < repeatedHitObject.NodeSamples.Count; nodeIndex++)
                {
                    samples = nodeIndex < repeatedHitObject.NodeSamples.Count ? repeatedHitObject.NodeSamples[nodeIndex] : hitObject.Samples;
                    if (Mode == HitSoundTrackMode.Volume)
                        Add(new NodeHitSoundTrackSamplePointVolumeControlBlueprint(hitObject, samples, nodeIndex));
                    else
                        Add(new NodeHitSoundTrackSamplePointBlueprint(hitObject, samples, nodeIndex));
                }
            }
            else
            {
                if (Mode == HitSoundTrackMode.Volume)
                    Add(new HitSoundTrackSamplePointVolumeControlBlueprint(hitObject, samples));
                else
                    Add(new HitSoundTrackSamplePointBlueprint(hitObject, samples));
            }
        }

        private void updateHitObjectFromTrack(HitObject hitObject)
        {
            if (hitObject is not IHasRepeats hasRepeats)
                return;

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

                blueprint.UpdateWidthAndPosition();
            });

            if (Mode != HitSoundTrackMode.Volume)
            {
                // hasRepeats.RepeatCount + 2 is because we only add the missing points between two ends
                for (int nodeIndex = existsNodeSamplePoints; nodeIndex < hasRepeats.RepeatCount + 2; nodeIndex++)
                {
                    IList<HitSampleInfo> samples = nodeIndex < hasRepeats.NodeSamples.Count ? hasRepeats.NodeSamples[nodeIndex] : hitObject.Samples;
                    Add(new NodeHitSoundTrackSamplePointBlueprint(hitObject, samples, nodeIndex));
                }
            }
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
    }
}
