// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    public class FollowPointRenderer : CompositeDrawable
    {
        private readonly List<FollowPointGroup> groups = new List<FollowPointGroup>();

        /// <summary>
        /// Adds the <see cref="FollowPoint"/>s around a <see cref="DrawableOsuHitObject"/>.
        /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to add <see cref="FollowPoint"/>s for.</param>
        public void AddFollowPoints(DrawableOsuHitObject hitObject)
            => addGroup(new FollowPointGroup(hitObject).With(g => g.StartTime.BindValueChanged(_ => onStartTimeChanged(g))));

        /// <summary>
        /// Removes the <see cref="FollowPoint"/>s around a <see cref="DrawableOsuHitObject"/>.
        /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableOsuHitObject"/> to remove <see cref="FollowPoint"/>s for.</param>
        public void RemoveFollowPoints(DrawableOsuHitObject hitObject) => removeGroup(groups.Single(g => g.Start == hitObject));

        /// <summary>
        /// Adds a <see cref="FollowPointGroup"/> to this <see cref="FollowPointRenderer"/>.
        /// </summary>
        /// <param name="group">The <see cref="FollowPointGroup"/> to add.</param>
        /// <returns>The index of <paramref name="group"/> in <see cref="groups"/>.</returns>
        private int addGroup(FollowPointGroup group)
        {
            AddInternal(group);

            // Groups are sorted by their start time when added such that the index can be used to post-process other surrounding groups
            int index = groups.AddInPlace(group, Comparer<FollowPointGroup>.Create((g1, g2) => g1.StartTime.Value.CompareTo(g2.StartTime.Value)));

            if (index < groups.Count - 1)
            {
                // Update the group's end point to the next hitobject
                //     h1 -> -> -> h2
                //  hitObject   nextGroup

                FollowPointGroup nextGroup = groups[index + 1];
                group.End = nextGroup.Start;
            }
            else
                group.End = null;

            if (index > 0)
            {
                // Previous group's end point to the current group's start point
                //     h1 -> -> -> h2
                //  prevGroup   hitObject

                FollowPointGroup previousGroup = groups[index - 1];
                previousGroup.End = group.Start;
            }

            return index;
        }

        /// <summary>
        /// Removes a <see cref="FollowPointGroup"/> from this <see cref="FollowPointRenderer"/>.
        /// </summary>
        /// <param name="group">The <see cref="FollowPointGroup"/> to remove.</param>
        /// <returns>Whether <paramref name="group"/> was removed.</returns>
        private bool removeGroup(FollowPointGroup group)
        {
            RemoveInternal(group);

            int index = groups.IndexOf(group);

            if (index > 0)
            {
                // Update the previous group's end point to the next group's start point
                //     h1 -> -> -> h2 -> -> -> h3
                //  prevGroup    group       nextGroup
                // The current group's end point is used since there may not be a next group
                FollowPointGroup previousGroup = groups[index - 1];
                previousGroup.End = group.End;
            }

            return groups.Remove(group);
        }

        private void onStartTimeChanged(FollowPointGroup group)
        {
            // Naive but can be improved if performance becomes problematic
            removeGroup(group);
            addGroup(group);
        }
    }
}
