// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Mods.Evast.LifeGame;

namespace osu.Mods.Evast.Snake
{
    public class SnakeScreen : TestScreen
    {
        private SnakePlayfield playfield;
        private SpeedSettings speedSettings;
        private GeneralSettings generalSettings;

        protected override void AddTestObject(Container parent)
        {
            parent.Child = playfield = new SnakePlayfield(20, 20, 25)
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
            generalSettings.StopButton.Action = playfield.Stop;
            generalSettings.RestartButton.Action = playfield.Restart;
            generalSettings.PauseButton.Action = playfield.Pause;
            generalSettings.ContinueButton.Action = playfield.Continue;

            speedSettings.SpeedBindable.ValueChanged += newValue => playfield.UpdateDelay = newValue;
        }

        private class GeneralSettings : PlayerSettingsGroup
        {
            protected override string Title => @"general";

            public readonly SettingsButton StopButton;
            public readonly SettingsButton RestartButton;
            public readonly SettingsButton PauseButton;
            public readonly SettingsButton ContinueButton;

            public GeneralSettings()
            {
                Children = new Drawable[]
                {
                    StopButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Stop",
                    },
                    RestartButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Restart",
                    },
                    PauseButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Pause",
                    },
                    ContinueButton = new SettingsButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Text = "Continue",
                    },
                };
            }
        }
    }
}
