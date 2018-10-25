// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Mods.Evast.LifeGame
{
    public class LifeGameScreen : TestScreen
    {
        private LifeGamePlayfield playfield;
        private GeneralSettings generalSettings;
        private SpeedSettings speedSettings;

        protected override void AddTestObject(Container parent)
        {
            parent.Child = playfield = new LifeGamePlayfield(55, 55, 12)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        protected override void AddSettings(FillFlowContainer parent)
        {
            parent.Children = new Drawable[]
            {
                generalSettings = new GeneralSettings(),
                speedSettings = new SpeedSettings(playfield.UpdateDelay),
            };
        }

        protected override void Connect()
        {
            generalSettings.ResetButton.Action = playfield.Stop;
            generalSettings.StartButton.Action = playfield.Continue;
            generalSettings.PauseButton.Action = playfield.Pause;
            generalSettings.RandomButton.Action = playfield.GenerateRandom;

            speedSettings.SpeedBindable.ValueChanged += newValue => playfield.UpdateDelay = newValue;
        }

        private class GeneralSettings : PlayerSettingsGroup
        {
            protected override string Title => @"general";

            public readonly SettingsButton ResetButton;
            public readonly SettingsButton StartButton;
            public readonly SettingsButton PauseButton;
            public readonly SettingsButton RandomButton;

            public GeneralSettings()
            {
                Children = new Drawable[]
                {
                    ResetButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Reset simulation",
                    },
                    StartButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Start simulation",
                    },
                    PauseButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Pause simulation",
                    },
                    RandomButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Create random map",
                    }
                };
            }
        }
    }

    public class SpeedSettings : PlayerSettingsGroup
    {
        protected override string Title => @"speed";

        public readonly BindableDouble SpeedBindable;

        public SpeedSettings(double defaultValue)
        {
            Children = new Drawable[]
            {
                    new PlayerSliderBar<double>
                    {
                        LabelText = "Update delay",
                        Bindable = SpeedBindable = new BindableDouble(defaultValue)
                        {
                            Default = defaultValue,
                            MinValue = 5,
                            MaxValue = 500,
                        }
                    }
            };
        }
    }
}
