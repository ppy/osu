// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    public class FollowPointRenderer : CompositeDrawable
    {
        /// <summary>
        /// Adds the <see cref="FollowPoint"/>s around a <see cref="DrawableOsuHitObject"/>.
        /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to add <see cref="FollowPoint"/>s for.</param>
        public void AddFollowPoints(DrawableOsuHitObject hitObject)
        {
            var startGroup = new FollowPointGroup(hitObject);
            AddInternal(startGroup);

            // Groups are sorted by their start time when added, so the index can be used to post-process other surrounding groups
            int startIndex = IndexOfInternal(startGroup);

            if (startIndex < InternalChildren.Count - 1)
            {
                //     h1 -> -> -> h2
                //  hitObject   nextGroup

                var nextGroup = (FollowPointGroup)InternalChildren[startIndex + 1];
                startGroup.End = nextGroup.Start;
            }

            if (startIndex > 0)
            {
                //     h1 -> -> -> h2
                //  prevGroup   hitObject

                var previousGroup = (FollowPointGroup)InternalChildren[startIndex - 1];
                previousGroup.End = startGroup.Start;
            }
        }

        /// <summary>
        /// Removes the <see cref="FollowPoint"/>s around a <see cref="DrawableOsuHitObject"/>.
        /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to remove <see cref="FollowPoint"/>s for.</param>
        public void RemoveFollowPoints(DrawableOsuHitObject hitObject)
        {
            var groups = findGroups(hitObject);

            // Regardless of the position of the hitobject in the beatmap, there will always be a group leading from the hitobject
            RemoveInternal(groups.start);

            if (groups.end != null)
            {
                // When there were two groups referencing the same hitobject,  merge them by updating the end group to point to the new end (the start group was already removed)
                groups.end.End = groups.start.End;
            }
        }

        /// <summary>
        /// Finds the <see cref="FollowPointGroup"/>s with <paramref name="hitObject"/> as the start and end <see cref="DrawableOsuHitObject"/>s.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to find the relevant <see cref="FollowPointGroup"/> of.</param>
        /// <returns>A tuple containing the end group (the <see cref="FollowPointGroup"/> where <paramref name="hitObject"/> is the end of),
        /// and the start group (the <see cref="FollowPointGroup"/> where <paramref name="hitObject"/> is the start of).</returns>
        private (FollowPointGroup start, FollowPointGroup end) findGroups(DrawableOsuHitObject hitObject)
        {
            //           endGroup         startGroup
            //     h1 -> -> -> -> -> h2 -> -> -> -> -> h3
            //                    hitObject

            FollowPointGroup startGroup = null; // The group which the hitobject is the start in
            FollowPointGroup endGroup = null; // The group which the hitobject is the end in

            int startIndex = 0;

            for (; startIndex < InternalChildren.Count; startIndex++)
            {
                var group = (FollowPointGroup)InternalChildren[startIndex];

                if (group.Start == hitObject)
                {
                    startGroup = group;
                    break;
                }
            }

            if (startIndex > 0)
                endGroup = (FollowPointGroup)InternalChildren[startIndex - 1];

            return (startGroup, endGroup);
        }

        protected override int Compare(Drawable x, Drawable y)
        {
            var groupX = (FollowPointGroup)x;
            var groupY = (FollowPointGroup)y;

            return groupX.Start.HitObject.StartTime.CompareTo(groupY.Start.HitObject.StartTime);
        }
    }
}
