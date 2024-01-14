// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarClock : OsuClickableContainer
    {
        private Bindable<ToolbarClockDisplayMode> clockDisplayMode;
        private Bindable<bool> prefer24HourTime;

        private Box hoverBackground;
        private Box flashBackground;

        private DigitalClockDisplay digital;
        private AnalogClockDisplay analog;

        public ToolbarClock()
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            clockDisplayMode = config.GetBindable<ToolbarClockDisplayMode>(OsuSetting.ToolbarClockDisplayMode);
            prefer24HourTime = config.GetBindable<bool>(OsuSetting.Prefer24HourTime);

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Padding = new MarginPadding(3),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 6,
                            CornerExponent = 3f,
                            Children = new Drawable[]
                            {
                                hoverBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.Gray(80).Opacity(180),
                                    Blending = BlendingParameters.Additive,
                                    Alpha = 0,
                                },
                                flashBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                    Colour = Color4.White.Opacity(100),
                                    Blending = BlendingParameters.Additive,
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Padding = new MarginPadding(10),
                            Children = new Drawable[]
                            {
                                analog = new AnalogClockDisplay
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                digital = new DigitalClockDisplay
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            clockDisplayMode.BindValueChanged(displayMode =>
            {
                bool showAnalog = displayMode.NewValue == ToolbarClockDisplayMode.Analog || displayMode.NewValue == ToolbarClockDisplayMode.Full;
                bool showDigital = displayMode.NewValue != ToolbarClockDisplayMode.Analog;
                bool showRuntime = displayMode.NewValue == ToolbarClockDisplayMode.DigitalWithRuntime || displayMode.NewValue == ToolbarClockDisplayMode.Full;

                digital.FadeTo(showDigital ? 1 : 0);
                digital.ShowRuntime = showRuntime;

                analog.FadeTo(showAnalog ? 1 : 0);
            }, true);

            prefer24HourTime.BindValueChanged(prefer24H => digital.Use24HourDisplay = prefer24H.NewValue, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            flashBackground.FadeOutFromOne(800, Easing.OutQuint);

            cycleDisplayMode();

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverBackground.FadeIn(200);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBackground.FadeOut(200);

            base.OnHoverLost(e);
        }

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverClickSounds(sampleSet);

        private void cycleDisplayMode()
        {
            switch (clockDisplayMode.Value)
            {
                case ToolbarClockDisplayMode.Analog:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.Full;
                    break;

                case ToolbarClockDisplayMode.Digital:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.Analog;
                    break;

                case ToolbarClockDisplayMode.DigitalWithRuntime:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.Digital;
                    break;

                case ToolbarClockDisplayMode.Full:
                    clockDisplayMode.Value = ToolbarClockDisplayMode.DigitalWithRuntime;
                    break;
            }
        }
    }
}
