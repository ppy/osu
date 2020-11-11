// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    /// <summary>
    /// Visualises connections between <see cref="DrawableOsuHitObject"/>s.
    /// </summary>
    public class FollowPointRenderer : LifetimeManagementContainer
    {
        /// <summary>
        /// All the <see cref="FollowPointConnection"/>s contained by this <see cref="FollowPointRenderer"/>.
        /// </summary>
        internal IReadOnlyList<FollowPointConnection> Connections => connections;

        private readonly List<FollowPointConnection> connections = new List<FollowPointConnection>();

        public override bool RemoveCompletedTransforms => false;

        /// <summary>
        /// Adds the <see cref="FollowPoint"/>s around an <see cref="OsuHitObject"/>.
        /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="OsuHitObject"/> to add <see cref="FollowPoint"/>s for.</param>
        public void AddFollowPoints(OsuHitObject hitObject)
            => addConnection(new FollowPointConnection(hitObject).With(g => g.StartTime.BindValueChanged(_ => onStartTimeChanged(g))));

        /// <summary>
        /// Removes the <see cref="FollowPoint"/>s around an <see cref="OsuHitObject"/>.
        /// This includes <see cref="FollowPoint"/>s leading into <paramref name="hitObject"/>, and <see cref="FollowPoint"/>s exiting <paramref name="hitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="OsuHitObject"/> to remove <see cref="FollowPoint"/>s for.</param>
        public void RemoveFollowPoints(OsuHitObject hitObject) => removeGroup(connections.Single(g => g.Start == hitObject));

        /// <summary>
        /// Adds a <see cref="FollowPointConnection"/> to this <see cref="FollowPointRenderer"/>.
        /// </summary>
        /// <param name="connection">The <see cref="FollowPointConnection"/> to add.</param>
        /// <returns>The index of <paramref name="connection"/> in <see cref="connections"/>.</returns>
        private void addConnection(FollowPointConnection connection)
        {
            // Groups are sorted by their start time when added such that the index can be used to post-process other surrounding connections
            int index = connections.AddInPlace(connection, Comparer<FollowPointConnection>.Create((g1, g2) =>
            {
                int comp = g1.StartTime.Value.CompareTo(g2.StartTime.Value);

                if (comp != 0)
                    return comp;

                // we always want to insert the new item after equal ones.
                // this is important for beatmaps with multiple hitobjects at the same point in time.
                // if we use standard comparison insert order, there will be a churn of connections getting re-updated to
                // the next object at the point-in-time, adding a construction/disposal overhead (see FollowPointConnection.End implementation's ClearInternal).
                // this is easily visible on https://osu.ppy.sh/beatmapsets/150945#osu/372245
                return -1;
            }));

            if (index < connections.Count - 1)
            {
                // Update the connection's end point to the next connection's start point
                //     h1 -> -> -> h2
                //    connection    nextGroup

                FollowPointConnection nextConnection = connections[index + 1];
                connection.End = nextConnection.Start;
            }
            else
            {
                // The end point may be non-null during re-ordering
                connection.End = null;
            }

            if (index > 0)
            {
                // Update the previous connection's end point to the current connection's start point
                //     h1 -> -> -> h2
                //  prevGroup    connection

                FollowPointConnection previousConnection = connections[index - 1];
                previousConnection.End = connection.Start;
            }

            AddInternal(connection);
        }

        /// <summary>
        /// Removes a <see cref="FollowPointConnection"/> from this <see cref="FollowPointRenderer"/>.
        /// </summary>
        /// <param name="connection">The <see cref="FollowPointConnection"/> to remove.</param>
        /// <returns>Whether <paramref name="connection"/> was removed.</returns>
        private void removeGroup(FollowPointConnection connection)
        {
            RemoveInternal(connection);

            int index = connections.IndexOf(connection);

            if (index > 0)
            {
                // Update the previous connection's end point to the next connection's start point
                //     h1 -> -> -> h2 -> -> -> h3
                //  prevGroup    connection       nextGroup
                // The current connection's end point is used since there may not be a next connection
                FollowPointConnection previousConnection = connections[index - 1];
                previousConnection.End = connection.End;
            }

            connections.Remove(connection);
        }

        private void onStartTimeChanged(FollowPointConnection connection)
        {
            // Naive but can be improved if performance becomes an issue
            removeGroup(connection);
            addConnection(connection);
        }
    }
}
