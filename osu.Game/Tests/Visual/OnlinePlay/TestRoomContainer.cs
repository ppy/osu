// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Rooms;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// Contains a <see cref="Room"/> that is resolvable by components in test scenes.
    /// </summary>
    public class TestRoomContainer : Container
    {
        /// <summary>
        /// The cached <see cref="Room"/>.
        /// </summary>
        public readonly Room Room = new Room();

        [Cached]
        private readonly Bindable<Room> roomBindable;

        public TestRoomContainer()
        {
            RelativeSizeAxes = Axes.Both;
            roomBindable = new Bindable<Room>(Room);
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.Value = Room;
            return dependencies;
        }
    }
}
