using osu.Framework.GameModes.Testing;
using osu.Game.Graphics.TimeDisplay;

namespace osu.Desktop.Tests
{
    class TestCaseTimeDisplay : TestCase
    {
        public override string Name => @"Time Display";
        public override string Description => @"A clock for the player to keep track of time";

        public override void Reset()
        {
            base.Reset();

            Add(new TimeDisplay
            {
                Position = new OpenTK.Vector2(100, 100),
                TextSize = 24,
                Format = "'It is 's' seconds and 'm' minutes and 'tt' on 'd' day'"
            });
        }
    }
}
