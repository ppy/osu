using OpenTK;
using OpenTK.Graphics;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Timing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseModSelection : TestCase
    {
        public override string Name => @"Mod Selection";
        public override string Description => @"Test mod selection overlay.";

        private ModSelection selection;

        public override void Reset()
        {
            base.Reset();

            Children = new Drawable[] {
                selection = new ModSelection(),
                new Button()
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Size = new Vector2(150, 50),
                    Text = "Mods",
                    Colour = Color4.Pink,
                    Action = selection.ToggleVisibility
                }
            };
        }
    }
}
