// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A test case which can be used to test a screen (that relies on OnEntering being called to execute startup instructions).
    /// </summary>
    public abstract partial class ScreenTestScene : OsuManualInputManagerTestScene, IOverlayManager
    {
        protected readonly OsuScreenStack Stack;

        private readonly Container content;
        private readonly Container overlayContent;

        protected override Container<Drawable> Content => content;

        [Cached(typeof(IDialogOverlay))]
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
                overlayContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = DialogOverlay = new DialogOverlay()
                }
            });

            Stack.ScreenPushed += (_, newScreen) => Logger.Log($"{nameof(ScreenTestScene)} screen changed → {newScreen}");
            Stack.ScreenExited += (_, newScreen) => Logger.Log($"{nameof(ScreenTestScene)} screen changed ← {newScreen}");
        }

        protected void LoadScreen(OsuScreen screen) => Stack.Push(screen);

        [SetUpSteps]
        public virtual void SetUpSteps() => addExitAllScreensStep();

        [TearDownSteps]
        public virtual void TearDownSteps()
        {
            if (DebugUtils.IsNUnitRunning)
                addExitAllScreensStep();
        }

        private void addExitAllScreensStep()
        {
            AddUntilStep("exit all screens", () =>
            {
                if (Stack.CurrentScreen == null) return true;

                Stack.Exit();
                return false;
            });
        }

        #region IOverlayManager

        IBindable<OverlayActivation> IOverlayManager.OverlayActivationMode { get; } = new Bindable<OverlayActivation>(OverlayActivation.All);

        // in the blocking methods below it is important to be careful about threading (e.g. use `Expire()` rather than `Remove()`, and schedule transforms),
        // because in the worst case the clean-up methods could be called from async disposal.

        IDisposable IOverlayManager.RegisterBlockingOverlay(OverlayContainer overlayContainer)
        {
            overlayContent.Add(overlayContainer);
            return new InvokeOnDisposal(() => overlayContainer.Expire());
        }

        void IOverlayManager.ShowBlockingOverlay(OverlayContainer overlay)
            => Schedule(() => Stack.FadeColour(OsuColour.Gray(0.5f), 500, Easing.OutQuint));

        void IOverlayManager.HideBlockingOverlay(OverlayContainer overlay)
            => Schedule(() => Stack.FadeColour(Colour4.White, 500, Easing.OutQuint));

        #endregion
    }
}
