// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneSettingsPanel : OsuTestScene
    {
        private readonly SettingsPanel settings;
        private readonly DialogOverlay dialogOverlay;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SettingsFooter),
            typeof(SettingsOverlay),
        };

        public TestSceneSettingsPanel()
        {
            settings = new SettingsOverlay
            {
                State = { Value = Visibility.Visible }
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
