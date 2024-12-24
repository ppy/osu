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
    public partial class SoundTrackObjectsDisplay : TimelinePart<HitSoundTrackPart>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public readonly HitSoundTrackMode Mode;

        public SoundTrackObjectsDisplay(HitSoundTrackMode mode)
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
                if (hitObject.NestedHitObjects.Count < 1)
                {
                    removeHitObjectFromTrack(hitObject);
                    addHitObjectToTrack(hitObject);
                }
            };

            List<HitSoundTrackPart> objects = [];

            editorBeatmap.HitObjects.ForEach(addHitObjectToTrack);

            AddRange(objects);
        }

        private void removeHitObjectFromTrack(HitObject hitObject)
        {
            Children.Where(v =>
            {
                if (v is HitSoundTrackPart hitSoundTrackPart)
                    return hitSoundTrackPart.HitObject == hitObject;
                return false;
            }).ForEach(part => part.Expire());
        }

        private void addHitObjectToTrack(HitObject hitObject)
        {
            if (hitObject is IHasRepeats repeatedHitObject)
            {
                for (int i = 0; i < repeatedHitObject.NodeSamples.Count; i++)
                {
                    Add(new NodeHitSoundTrackPart(hitObject, repeatedHitObject, i));
                }
            }
            else
            {
                Add(new HitSoundTrackPart(hitObject));
            }
        }
    }
}
