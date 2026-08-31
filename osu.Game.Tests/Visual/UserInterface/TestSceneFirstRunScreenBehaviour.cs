// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Audio;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFirstRunScreenBehaviour : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Cached(typeof(IDialogOverlay))]
        private readonly DialogOverlay dialogOverlay = new DialogOverlay();

        public TestSceneFirstRunScreenBehaviour()
        {
            AudioOffsetCalibrationDialog? dialog = null;

            AddStep("load screen", () =>
            {
                Children = new Drawable[]
                {
                    new ScreenStack(new ScreenBehaviour()),
                    dialogOverlay,
                };
            });

            AddStep("open offset wizard", () => this.ChildrenOfType<OffsetSettings>().Single().ChildrenOfType<SettingsButtonV2>().Single().TriggerClick());
            AddUntilStep("reference playing", () => (dialog = dialogOverlay.ChildrenOfType<AudioOffsetCalibrationDialog>().SingleOrDefault())?.IsPlaying == true);
            AddStep("dismiss wizard", () => dialogOverlay.Hide());
            AddUntilStep("reference stopped", () => dialog?.IsPlaying == false);
        }
    }
}
