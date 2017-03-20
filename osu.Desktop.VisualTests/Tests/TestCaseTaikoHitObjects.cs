using OpenTK;
using osu.Framework.Screens.Testing;
using osu.Game.Modes.Taiko.Objects.Drawable.Pieces;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseTaikoHitObjects : TestCase
    {
        public override string Description => "Taiko hit objects";

        public override void Reset()
        {
            base.Reset();

            Add(new CentreHitCirclePiece
            {
                Position = new Vector2(100, 100)
            });

            Add(new FinisherPiece(new CentreHitCirclePiece())
            {
                Position = new Vector2(350, 100)
            });

            Add(new RimHitCirclePiece
            {
                Position = new Vector2(100, 280)
            });

            Add(new FinisherPiece(new RimHitCirclePiece())
            {
                Position = new Vector2(350, 280)
            });

            Add(new BashCirclePiece
            {
                Position = new Vector2(100, 460)
            });

            Add(new FinisherPiece(new BashCirclePiece())
            {
                Position = new Vector2(350, 460)
            });

            Add(new DrumRollCirclePiece
            {
                Width = 250,
                Position = new Vector2(100, 640)
            });

            Add(new FinisherPiece(new DrumRollCirclePiece()
            {
                Width = 250
            })
            {
                Position = new Vector2(600, 640)
            });
        }
    }
}
