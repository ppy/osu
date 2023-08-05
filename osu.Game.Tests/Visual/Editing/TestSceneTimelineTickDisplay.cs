// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneTimelineTickDisplay : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => Empty(); // tick display is implicitly inside the timeline.

        [Cached]
        private readonly OverlayColourProvider overlayColour = new OverlayColourProvider(OverlayColourScheme.Green);

        [BackgroundDependencyLoader]
        private void load()
        {
            BeatDivisor.Value = 4;

            Add(new BeatDivisorControl
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Margin = new MarginPadding(30),
                Size = new Vector2(90)
            });
        }
    }
}
