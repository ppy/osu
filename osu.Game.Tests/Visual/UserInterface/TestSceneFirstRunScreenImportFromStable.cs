// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using System.Threading.Tasks;
using Moq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Overlays.FirstRunSetup;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFirstRunScreenImportFromStable : OsuManualInputManagerTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private readonly Mock<LegacyImportManager> legacyImportManager = new Mock<LegacyImportManager>();

        [BackgroundDependencyLoader]
        private void load()
        {
            legacyImportManager.Setup(m => m.GetImportCount(It.IsAny<StableContent>(), It.IsAny<CancellationToken>())).Returns(() => Task.FromResult(RNG.Next(0, 256)));

            Dependencies.CacheAs(legacyImportManager.Object);
        }

        public TestSceneFirstRunScreenImportFromStable()
        {
            AddStep("load screen", () =>
            {
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new ScreenStack(new ScreenImportFromStable())
                    }
                };
            });
        }
    }
}
