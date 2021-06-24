// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// A <see cref="ScreenTestScene"/> providing all the dependencies cached by <see cref="OnlinePlayScreen"/> for testing <see cref="OnlinePlaySubScreen"/>s.
    /// </summary>
    public abstract class OnlinePlaySubScreenTestScene : ScreenTestScene
    {
        /// <summary>
        /// The cached <see cref="SelectedRoom"/>.
        /// </summary>
        protected Bindable<Room> SelectedRoom { get; private set; }

        /// <summary>
        /// The cached <see cref="IRoomManager"/>
        /// </summary>
        protected TestRoomManager RoomManager { get; private set; }

        protected Bindable<FilterCriteria> Filter { get; private set; }

        protected OngoingOperationTracker OngoingOperationTracker { get; private set; }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create dependencies", () => LoadScreen(new DependenciesScreen(CreateScreenDependencies)));
        }

        /// <summary>
        /// Creates dependencies for any <see cref="OsuScreen"/> pushed via <see cref="ScreenTestScene.LoadScreen"/>.
        /// Invoked at the start of every test via <see cref="SetUpSteps"/>.
        /// </summary>
        /// <remarks>
        /// This should be overridden to add any custom dependencies required by subclasses of <see cref="OnlinePlaySubScreen"/>.
        /// </remarks>
        /// <param name="parent">The parent dependency container.</param>
        /// <returns>The resultant dependency container.</returns>
        protected virtual IReadOnlyDependencyContainer CreateScreenDependencies(IReadOnlyDependencyContainer parent)
        {
            SelectedRoom = new Bindable<Room>();
            RoomManager = new TestRoomManager();
            Filter = new Bindable<FilterCriteria>(new FilterCriteria());
            OngoingOperationTracker = new OngoingOperationTracker();

            var dependencies = new DependencyContainer(new CachedModelDependencyContainer<Room>(parent) { Model = { BindTarget = SelectedRoom } });
            dependencies.CacheAs(SelectedRoom);
            dependencies.CacheAs<IRoomManager>(RoomManager);
            dependencies.CacheAs(Filter);
            dependencies.CacheAs(OngoingOperationTracker);

            return dependencies;
        }

        protected class TestRoomManager : IRoomManager
        {
            public event Action RoomsUpdated
            {
                add { }
                remove { }
            }

            public readonly BindableList<Room> Rooms = new BindableList<Room>();

            public IBindable<bool> InitialRoomsReceived { get; } = new Bindable<bool>(true);

            IBindableList<Room> IRoomManager.Rooms => Rooms;

            public void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            {
                room.RoomID.Value ??= Rooms.Select(r => r.RoomID.Value).Where(id => id != null).Select(id => id.Value).DefaultIfEmpty().Max() + 1;
                Rooms.Add(room);
                onSuccess?.Invoke(room);
            }

            public void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null) => onSuccess?.Invoke(room);

            public void PartRoom()
            {
            }

            public void AddRooms(int count, RulesetInfo ruleset = null)
            {
                for (int i = 0; i < count; i++)
                {
                    var room = new Room
                    {
                        RoomID = { Value = i },
                        Name = { Value = $"Room {i}" },
                        Host = { Value = new User { Username = "Host" } },
                        EndDate = { Value = DateTimeOffset.Now + TimeSpan.FromSeconds(10) },
                        Category = { Value = i % 2 == 0 ? RoomCategory.Spotlight : RoomCategory.Normal }
                    };

                    if (ruleset != null)
                    {
                        room.Playlist.Add(new PlaylistItem
                        {
                            Ruleset = { Value = ruleset },
                            Beatmap =
                            {
                                Value = new BeatmapInfo
                                {
                                    Metadata = new BeatmapMetadata()
                                }
                            }
                        });
                    }

                    CreateRoom(room);
                }
            }
        }

        /// <summary>
        /// A dummy screen used for injecting new dependencies into the hierarchy before any screen is pushed via <see cref="ScreenTestScene.LoadScreen"/>.
        /// </summary>
        private class DependenciesScreen : OsuScreen
        {
            private readonly Func<IReadOnlyDependencyContainer, IReadOnlyDependencyContainer> createDependenciesFunc;

            public DependenciesScreen(Func<IReadOnlyDependencyContainer, IReadOnlyDependencyContainer> createDependenciesFunc)
            {
                this.createDependenciesFunc = createDependenciesFunc;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
                => createDependenciesFunc(base.CreateChildDependencies(parent));
        }
    }
}
