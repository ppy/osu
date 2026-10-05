// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneShearedDropdown : ThemeComparisonTestScene
    {
        public TestSceneShearedDropdown()
            : base(false)
        {
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black.Opacity(0.75f),
                    RelativeSizeAxes = Axes.Both,
                },
                new ShearedDropdown<string>("Test")
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Y = 300f,
                    Width = 140,
                    Current = new Bindable<string>(),
                    Items = new[] { "Global", "Friends", "Local", "Really lonnnnnnng option" },
                }
            }
        };
    }
}
