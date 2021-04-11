// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisUISettings : SettingsSubsection
    {
        protected override string Header => "界面";
        private readonly BindableFloat iR = new BindableFloat();
        private readonly BindableFloat iG = new BindableFloat();
        private readonly BindableFloat iB = new BindableFloat();
        private ColourPreviewer preview;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisInterfaceRed, iR);
            config.BindWith(MSetting.MvisInterfaceGreen, iG);
            config.BindWith(MSetting.MvisInterfaceBlue, iB);

            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = "背景模糊",
                    Current = config.GetBindable<float>(MSetting.MvisBgBlur),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时的背景亮度",
                    Current = config.GetBindable<float>(MSetting.MvisIdleBgDim),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "空闲时Mvis面板的不透明度",
                    Current = config.GetBindable<float>(MSetting.MvisContentAlpha),
                    DisplayAsPercentage = true,
                    KeyboardStep = 0.01f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(红)",
                    Current = iR,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(绿)",
                    Current = iG,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                new SettingsSlider<float>
                {
                    LabelText = "界面主题色(蓝)",
                    Current = iB,
                    KeyboardStep = 1,
                    TransferValueOnCommit = false
                },
                preview = new ColourPreviewer(),
                new SettingsCheckbox
                {
                    LabelText = "置顶Proxy",
                    Current = config.GetBindable<bool>(MSetting.MvisStoryboardProxy),
                    TooltipText = "让所有Proxy显示在前景上方"
                },
                new SettingsCheckbox
                {
                    LabelText = "启用背景动画",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableBgTriangles),
                    TooltipText = "如果条件允许,播放器将会在背景显示动画"
                }
            };
        }

        protected override void LoadComplete()
        {
            iR.BindValueChanged(_ => updateColor());
            iG.BindValueChanged(_ => updateColor());
            iB.BindValueChanged(_ => updateColor(), true);
        }

        private void updateColor() => preview.UpdateColor(iR.Value, iG.Value, iB.Value);

        private class ColourPreviewer : Container
        {
            private readonly CustomColourProvider provider = new CustomColourProvider(0, 0, 0);
            private Box bg6;
            private Box bg5;
            private Box bg4;
            private Box bg3;
            private Box bg2;
            private Box bg1;
            private Box hl;
            private Box l4;
            private Box l3;
            private Box c2;
            private OsuSpriteText hueText;

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 75;
                RelativeSizeAxes = Axes.X;
                Padding = new MarginPadding { Horizontal = 15 };
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                        Children = new Drawable[]
                        {
                            bg5 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            bg4 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            bg3 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            bg2 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            bg1 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                        Margin = new MarginPadding { Top = 25 },
                        Children = new Drawable[]
                        {
                            bg6 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            hl = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            l4 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            l3 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                            c2 = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.2f
                            },
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 25,
                        Margin = new MarginPadding { Top = 50 },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black
                            },
                            hueText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            }
                        }
                    }
                };
            }

            public void UpdateColor(float r, float g, float b)
            {
                provider.UpdateHueColor(r, g, b);

                bg5.Colour = provider.Background5;
                bg4.Colour = provider.Background4;
                bg3.Colour = provider.Background3;
                bg2.Colour = provider.Background2;
                bg1.Colour = provider.Background1;

                bg6.Colour = provider.Background6;
                hl.Colour = provider.Highlight1;
                l4.Colour = provider.Light4;
                l3.Colour = provider.Light3;
                c2.Colour = provider.Content2;

                hueText.Text = $"Hue: {(provider.HueColour.Value * 360):#0.00}";
            }
        }
    }
}
