using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Overlays.Music;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCasePlaylistItem : TestCase
    {
        private PlaylistOverlay testOverlay;

        public override string Description => @"Testing playlist reordering.";

        public TestCasePlaylistItem()
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(testOverlay = new PlaylistOverlay()
            {
                Anchor = Framework.Graphics.Anchor.TopRight,
                Origin = Framework.Graphics.Anchor.TopRight,
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
            } );

            testOverlay.ToggleVisibility();
        }
    }
}
