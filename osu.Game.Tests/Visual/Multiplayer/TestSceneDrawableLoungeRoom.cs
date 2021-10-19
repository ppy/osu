// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableLoungeRoom : OsuTestScene
    {
        private readonly Room room = new Room
        {
            HasPassword = { Value = true }
        };

        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [BackgroundDependencyLoader]
        private void load()
        {
            var mockLounge = new Mock<LoungeSubScreen>();
            mockLounge
                .Setup(l => l.Join(It.IsAny<Room>(), It.IsAny<string>(), It.IsAny<Action<Room>>(), It.IsAny<Action<string>>()))
                .Callback<Room, string, Action<Room>, Action<string>>((a, b, c, d) =>
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(500);
                        Schedule(() => d?.Invoke("Incorrect password"));
                    });
                });

            Dependencies.CacheAs(mockLounge.Object);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create drawable", () =>
            {
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new DrawableLoungeRoom(room)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                };
            });
        }

        [Test]
        public void TestFocus()
        {
        }
    }
}
