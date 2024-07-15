// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings.Sections.Input;
using osu.Game.Rulesets.Osu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneKeyBindingConflictPopover : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider { get; set; } = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Test]
        public void TestAppearance()
        {
            ButtonWithConflictPopover button = null!;

            AddStep("create content", () =>
            {
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = button = new ButtonWithConflictPopover
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "Open popover",
                        Width = 300
                    }
                };
            });
            AddStep("show popover", () => button.TriggerClick());
        }

        private partial class ButtonWithConflictPopover : RoundedButton, IHasPopover
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                Action = this.ShowPopover;
            }

            public Popover GetPopover() => new KeyBindingConflictPopover(
                new KeyBindingRow.KeyBindingConflictInfo(
                    new KeyBindingRow.ConflictingKeyBinding(Guid.NewGuid(), OsuAction.LeftButton, KeyCombination.FromKey(Key.X), new KeyCombination(InputKey.None)),
                    new KeyBindingRow.ConflictingKeyBinding(Guid.NewGuid(), OsuAction.RightButton, KeyCombination.FromKey(Key.Z), KeyCombination.FromKey(Key.X))
                )
            );
        }
    }
}
