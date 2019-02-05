// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Tests.Visual
{
    public class MultiplayerTestCase : OsuTestCase
    {
        private Room room;

        protected Room Room
        {
            get => room;
            set
            {
                if (room == value)
                    return;
                room = value;

                if (dependencies != null)
                    dependencies.Model.Value = value;
            }
        }

        private CachedModelDependencyContainer<Room> dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent));
            dependencies.Model.Value = room;
            return dependencies;
        }
    }
}
