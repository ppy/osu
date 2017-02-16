// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Overlays.Mods;
using osu.Framework.GameModes.Testing;
using osu.Game.Modes;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseModSelect : TestCase
    {
        public override string Name => @"Mod Select";

        public override string Description => @"Tests the mod select overlay";

        private ModSelect modSelect;

        public override void Reset()
        {
            base.Reset();

            Add(modSelect = new ModSelect
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
                ModMode = PlayMode.Osu,
            });

            AddButton("Toggle", modSelect.ToggleVisibility);
            AddButton("osu!", () => modSelect.ModMode = PlayMode.Osu);
            AddButton("osu!taiko", () => modSelect.ModMode = PlayMode.Taiko);
            AddButton("osu!catch", () => modSelect.ModMode = PlayMode.Catch);
            AddButton("osu!mania", () => modSelect.ModMode = PlayMode.Mania);
        }
    }
}
