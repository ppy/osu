// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Logging;
using osu.Framework.Graphics;
using osu.Game.Overlays.Mods;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Colour;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Primitives;
using osu.Game.Modes.UI;
using osu.Game.Modes;
using OpenTK;
using osu.Game.Graphics;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseModSelector : TestCase
    {
        public override string Name => @"Mod Selector";

        public override string Description => @"Tests the mod selector overlay";

        private ModSelector modSelector;

        public override void Reset()
        {
            base.Reset();

            Add(modSelector = new ModSelector
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            AddButton("Toggle", modSelector.ToggleVisibility);
        }
    }
}
