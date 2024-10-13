// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFormSliderBar : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Test]
        public void TestTransferValueOnCommit()
        {
            OsuSpriteText text;
            FormSliderBar<float> slider = null!;

            AddStep("create content", () =>
            {
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 0.5f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText(),
                        slider = new FormSliderBar<float>
                        {
                            Caption = "Slider",
                            Current = new BindableFloat
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 0.1f,
                                Default = 5f,
                            }
                        },
                    }
                };
                slider.Current.BindValueChanged(_ => text.Text = $"Current value is: {slider.Current.Value}", true);
            });
            AddToggleStep("toggle transfer value on commit", b =>
            {
                if (slider.IsNotNull())
                    slider.TransferValueOnCommit = b;
            });
        }
    }
}
