// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneSettingsPanel : OsuTestScene
    {
        private SettingsPanel settings;
        private DialogOverlay dialogOverlay;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create settings", () =>
            {
                settings?.Expire();

                Add(settings = new SettingsOverlay
                {
                    State = { Value = Visibility.Visible }
                });
            });
        }

        [Test]
        public void ToggleVisibility()
        {
            AddWaitStep("wait some", 5);
            AddToggleStep("toggle editor visibility", visible => settings.ToggleVisibility());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(dialogOverlay = new DialogOverlay
            {
                Depth = -1
            });

            Dependencies.Cache(dialogOverlay);
        }
    }
}
