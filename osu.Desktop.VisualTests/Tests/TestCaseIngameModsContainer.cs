// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Screens.Play;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseIngameModsContainer : TestCaseModSelectOverlay
    {
        public override string Description => @"Ingame mods visualization";

        private ModsContainer modsContainer;

        public override void Reset()
        {
            base.Reset();

            Add(modsContainer = new ModsContainer
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Position = new Vector2(0, 25),
            });

            modsContainer.Mods.BindTo(ModSelect.SelectedMods);
        }
    }
}
