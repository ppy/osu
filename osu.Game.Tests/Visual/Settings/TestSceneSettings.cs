// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneSettings : OsuTestScene
    {
        private readonly SettingsPanel settings;
        private readonly DialogOverlay dialogOverlay;

        public TestSceneSettings()
        {
            settings = new SettingsOverlay
            {
                State = Visibility.Visible
            };
            Add(dialogOverlay = new DialogOverlay
            {
                Depth = -1
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(dialogOverlay);

            Add(settings);
        }
    }
}
