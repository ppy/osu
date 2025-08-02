// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneDifficultyRangeSlider : ThemeComparisonTestScene
    {
        private readonly BindableNumber<double> customStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 10,
            Precision = 0.1f
        };

        private readonly BindableNumber<double> customEnd = new BindableNumber<double>(10)
        {
            MinValue = 0,
            MaxValue = 10,
            Precision = 0.1f
        };

        public TestSceneDifficultyRangeSlider()
            : base(false)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CreateThemedContent(OverlayColourScheme.Aquamarine);
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                new FilterControl.DifficultyRangeSlider
                {
                    Width = 600,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1),
                    LowerBound = customStart,
                    UpperBound = customEnd,
                    NubWidth = 32,
                    MinRange = 0.1f,
                }
            }
        };
    }
}
