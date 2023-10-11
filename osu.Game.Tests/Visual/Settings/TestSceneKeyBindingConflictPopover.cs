// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
                OsuAction.LeftButton,
                OsuAction.RightButton,
                new KeyCombination(InputKey.Z));
        }
    }
}
