// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Overlays.Mods;
using osu.Framework.Screens.Testing;
using osu.Game.Modes;
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseModSelectOverlay : TestCase
    {
        public override string Name => @"Mod Select";

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

            AddButton("Toggle", modSelect.ToggleVisibility);
            AddButton("osu!", () => modSelect.PlayMode.Value = PlayMode.Osu);
            AddButton("osu!taiko", () => modSelect.PlayMode.Value = PlayMode.Taiko);
            AddButton("osu!catch", () => modSelect.PlayMode.Value = PlayMode.Catch);
            AddButton("osu!mania", () => modSelect.PlayMode.Value = PlayMode.Mania);
        }
    }
}
