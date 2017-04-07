// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Overlays.Mods;
using osu.Framework.Testing;
using osu.Game.Modes;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseModSelectOverlay : TestCase
    {
        public override string Description => @"Tests the mod select overlay";

        private ModSelectOverlay modSelect;

        public override void Reset()
        {
            base.Reset();

            Add(modSelect = new ModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            AddStep("Toggle", modSelect.ToggleVisibility);
            AddStep("osu!", () => modSelect.PlayMode.Value = PlayMode.Osu);
            AddStep("osu!taiko", () => modSelect.PlayMode.Value = PlayMode.Taiko);
            AddStep("osu!catch", () => modSelect.PlayMode.Value = PlayMode.Catch);
            AddStep("osu!mania", () => modSelect.PlayMode.Value = PlayMode.Mania);
        }
    }
}
