// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCasePlayerLoader : ManualInputManagerTestCase
    {
        private PlayerLoader loader;
        private ScreenStack stack;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            Beatmap.Value = new DummyWorkingBeatmap(game);

            InputManager.Add(stack = new ScreenStack { RelativeSizeAxes = Axes.Both });

            AddStep("load dummy beatmap", () => stack.Push(loader = new PlayerLoader(() => new Player
            {
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
            })));

            AddUntilStep(() => loader.IsCurrentScreen(), "wait for current");

            AddStep("mouse in centre", () => InputManager.MoveMouseTo(loader.ScreenSpaceDrawQuad.Centre));

            AddUntilStep(() => !loader.IsCurrentScreen(), "wait for no longer current");

            AddStep("exit loader", () => loader.Exit());

            AddUntilStep(() => !loader.IsAlive, "wait for no longer alive");

            AddStep("load slow dummy beatmap", () =>
            {
                SlowLoadPlayer slow = null;

                stack.Push(loader = new PlayerLoader(() => slow = new SlowLoadPlayer
                {
                    AllowPause = false,
                    AllowLeadIn = false,
                    AllowResults = false,
                }));

                Scheduler.AddDelayed(() => slow.Ready = true, 5000);
            });

            AddUntilStep(() => !loader.IsCurrentScreen(), "wait for no longer current");
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
