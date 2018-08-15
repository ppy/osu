// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Threading;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCasePlayerLoader : ManualInputManagerTestCase
    {
        private PlayerLoader loader;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            Beatmap.Value = new DummyWorkingBeatmap(game);

            AddStep("load dummy beatmap", () => Add(loader = new PlayerLoader(new Player
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
            })));

            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));

            AddUntilStep(() => !loader.IsCurrentScreen, "wait for no longer current");

            AddStep("load slow dummy beatmap", () =>
            {
                SlowLoadPlayer slow;

                Add(loader = new PlayerLoader(slow = new SlowLoadPlayer
                {
                    AllowPause = false,
                    AllowLeadIn = false,
                    AllowResults = false,
                }));

                Scheduler.AddDelayed(() => slow.Ready = true, 5000);
            });

            AddUntilStep(() => !loader.IsCurrentScreen, "wait for no longer current");
        }

        protected class SlowLoadPlayer : Player
        {
            public bool Ready;

            [BackgroundDependencyLoader]
            private void load()
            {
                while (!Ready)
                    Thread.Sleep(1);
            }
        }
    }

}
