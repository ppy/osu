// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Overlays.KeyBinding;

namespace osu.Game.Tests.Visual.Settings
{
    [TestFixture]
    public class TestSceneKeyBindingPanel : OsuTestScene
    {
        private readonly KeyBindingPanel panel;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(KeyBindingRow),
            typeof(GlobalKeyBindingsSection),
            typeof(KeyBindingRow),
            typeof(KeyBindingsSubsection),
            typeof(RulesetBindingsSection),
            typeof(VariantBindingsSubsection),
        };

        public TestSceneKeyBindingPanel()
        {
            Child = panel = new KeyBindingPanel();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            panel.Show();
        }
    }
}
