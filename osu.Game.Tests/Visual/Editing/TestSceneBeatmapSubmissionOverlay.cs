// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.Edit.Submission;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneBeatmapSubmissionOverlay : ScreenTestScene
    {
        private TestBeatmapSubmissionOverlayScreen screen = null!;

        [Cached]
        private readonly BeatmapSubmissionSettings beatmapSubmissionSettings = new BeatmapSubmissionSettings();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push screen", () => LoadScreen(screen = new TestBeatmapSubmissionOverlayScreen()));
            AddUntilStep("wait until screen is loaded", () => screen.IsLoaded, () => Is.True);
            AddStep("show overlay", () => screen.Overlay.Show());
        }

        private partial class TestBeatmapSubmissionOverlayScreen : OsuScreen
        {
            public override bool ShowFooter => true;

            public BeatmapSubmissionOverlay Overlay = null!;

            private IDisposable? overlayRegistration;

            [Resolved]
            private IOverlayManager? overlayManager { get; set; }

            [Cached]
            private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(Overlay = new BeatmapSubmissionOverlay());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                overlayRegistration = overlayManager?.RegisterBlockingOverlay(Overlay);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                overlayRegistration?.Dispose();
            }
        }
    }
}
