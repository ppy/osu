using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Mf;
using osu.Game.Overlays.Settings.Sections.Mf;

namespace osu.Game.Tests.Visual.Mvis
{
    public partial class TestSceneFontInfoLabel : ScreenTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#000"), Color4Extensions.FromHex("#333")),
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Top = 8, Horizontal = 15 },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 300,
                    Masking = true,
                    CornerRadius = 5,
                    Children = new Drawable[]
                    {
                        new FontInfoLabel(new ExperimentalSettings.FakeFont())
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new FontInfoLabel(new ExperimentalSettings.FakeFont())
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                        new FontInfoLabel(new ExperimentalSettings.FakeFont())
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        },
                    }
                }
            };
        }
    }
}
