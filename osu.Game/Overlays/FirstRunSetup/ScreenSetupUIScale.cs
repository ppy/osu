// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    public class ScreenSetupUIScale : FirstRunSetupScreen
    {
        [Resolved]
        private OsuConfigManager osuConfig { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            OsuScreenStack stack;
            Content.Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(20),
                    Children = new Drawable[]
                    {
                        new OsuTextFlowContainer
                        {
                            Text = "The osu! user interface size can be adjusted to your liking.",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                        new SettingsSlider<float, UIScaleSlider>
                        {
                            LabelText = GraphicsSettingsStrings.UIScaling,
                            TransferValueOnCommit = true,
                            Current = osuConfig.GetBindable<float>(OsuSetting.UIScale),
                            KeyboardStep = 0.01f,
                        },
                        new DrawSizePreservingFillContainer
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            Height = 300,
                            Child = stack = new OsuScreenStack()
                        }
                    }
                },
                new PurpleTriangleButton
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding(10),
                    Text = "Finish",
                    Action = () => Overlay.Hide()
                }
            };

            stack.Push(new PlaySongSelect());
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            Overlay.MoveDisplayTo(new Vector2(0.5f));
            Overlay.ResizeDisplayTo(new Vector2(0.8f));
        }

        private class UIScaleSlider : OsuSliderBar<float>
        {
            public override LocalisableString TooltipText => base.TooltipText + "x";
        }
    }
}
