// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;

namespace osu.Game.Tests.Visual
{
    public abstract class RoomTestScene : ScreenTestScene
    {
        [Cached]
        private readonly Bindable<Room> currentRoom = new Bindable<Room>();

        protected Room Room => currentRoom.Value;

        private CachedModelDependencyContainer<Room> dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.BindTo(currentRoom);
            return dependencies;
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            currentRoom.Value = new Room();
        });
    }
}
