// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A test case which can be used to test a screen (that relies on OnEntering being called to execute startup instructions).
    /// </summary>
    public abstract class ScreenTestScene : OsuManualInputManagerTestScene
    {
        protected readonly OsuScreenStack Stack;

        private readonly Container content;

        protected override Container<Drawable> Content => content;

        [Cached]
        protected DialogOverlay DialogOverlay { get; private set; }

        protected ScreenTestScene()
        {
            base.Content.AddRange(new Drawable[]
            {
                Stack = new OsuScreenStack
                {
                    Name = nameof(ScreenTestScene),
                    RelativeSizeAxes = Axes.Both
                },
                content = new Container { RelativeSizeAxes = Axes.Both },
                DialogOverlay = new DialogOverlay()
            });

            Stack.ScreenPushed += (lastScreen, newScreen) => Logger.Log($"{nameof(ScreenTestScene)} screen changed → {newScreen}");
            Stack.ScreenExited += (lastScreen, newScreen) => Logger.Log($"{nameof(ScreenTestScene)} screen changed ← {newScreen}");
        }

        protected void LoadScreen(OsuScreen screen) => Stack.Push(screen);

        [SetUpSteps]
        public virtual void SetUpSteps() => addExitAllScreensStep();

        [TearDownSteps]
        public virtual void TearDownSteps() => addExitAllScreensStep();

        private void addExitAllScreensStep()
        {
            AddUntilStep("exit all screens", () =>
            {
                if (Stack.CurrentScreen == null) return true;

                Stack.Exit();
                return false;
            });
        }
    }
}
