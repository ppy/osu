using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens.Testing;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Input;
using osu.Game.Modes.Taiko.UI;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseTaikoPlayfield : TestCase
    {
        public override string Name => "Taiko Playfield";
        public override string Description => "The Taiko playfield.";

        private SpriteText comboCounter;

        public override void Reset()
        {
            base.Reset();

            Add(new BackgroundScreenCustom(@"Backgrounds/bg1"));

            Add(new[]
            {
                new TaikoPlayfield()
            });
        }
    }
}
