// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Tests.Visual
{
    public class MultiplayerTestCase : OsuTestCase
    {
        [Cached]
        private readonly Bindable<Room> currentRoom = new Bindable<Room>(new Room());

        protected Room Room
        {
            get => currentRoom;
            set => currentRoom.Value = value;
        }

        private CachedModelDependencyContainer<Room> dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.BindTo(currentRoom);
            return dependencies;
        }
    }
}
