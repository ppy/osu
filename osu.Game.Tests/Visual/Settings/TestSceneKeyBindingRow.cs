// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Testing;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneKeyBindingRow : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Test]
        public void TestChangesAfterConstruction()
        {
            KeyBindingRow row = null!;

            AddStep("create row", () => Child = new Container
            {
                Width = 500,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = row = new KeyBindingRow(GlobalAction.Back)
                {
                    Defaults = new[]
                    {
                        new KeyCombination(InputKey.Escape),
                        new KeyCombination(InputKey.ExtraMouseButton1)
                    }
                }
            });

            AddStep("change key bindings", () =>
            {
                row.KeyBindings.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.Escape)));
                row.KeyBindings.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.ExtraMouseButton1)));
            });
            AddUntilStep("revert to default button not shown", () => row.ChildrenOfType<RevertToDefaultButton<bool>>().Single().Alpha, () => Is.Zero);

            AddStep("change key bindings", () =>
            {
                row.KeyBindings.Clear();
                row.KeyBindings.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.X)));
                row.KeyBindings.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.Z)));
                row.KeyBindings.Add(new RealmKeyBinding(GlobalAction.Back, new KeyCombination(InputKey.I)));
            });
            AddUntilStep("revert to default button not shown", () => row.ChildrenOfType<RevertToDefaultButton<bool>>().Single().Alpha, () => Is.Not.Zero);
        }
    }
}
