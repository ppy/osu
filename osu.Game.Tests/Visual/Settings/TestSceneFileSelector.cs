// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tests.Visual.UserInterface;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneFileSelector : ThemeComparisonTestScene
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Test]
        public void TestJpgFilesOnly()
        {
            AddStep("create", () =>
            {
                Cell(0, 0).Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreySeaFoam
                    },
                    new OsuFileSelector(validFileExtensions: new[] { ".jpg" })
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            });
        }

        protected override Drawable CreateContent() => new OsuFileSelector
        {
            RelativeSizeAxes = Axes.Both,
        };
    }
}
