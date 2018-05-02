// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Screens.Setup.Screens;

namespace osu.Game.Screens.Edit.Screens.Setup
{
    public class Setup : EditorScreen
    {
        public readonly GeneralSettings GeneralSettings;
        public readonly DifficultySettings DifficultySettings;
        public readonly ModeSettings ModeSettings;
        public readonly AudioSettings AudioSettings;
        public readonly DesignSettings DesignSettings;
        public readonly ColoursSettings ColoursSettings;
        public readonly AdvancedSettings AdvancedSettings;

        public Setup()
        {
            AlwaysPresent = true;

            Child = new FillFlowContainer
            {
                // TODO: Make sure the container is using the entire screen and all children's size is adjusted according to the window size
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(20, 0),
                Padding = new MarginPadding { Top = 20, Left = 20 },
                Children = new Drawable[]
                {
                    new FillFlowContainer<SettingsGroup>
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Children = new SettingsGroup[]
                        {
                            GeneralSettings = new GeneralSettings(),
                        }
                    },
                    new FillFlowContainer<SettingsGroup>
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Children = new SettingsGroup[]
                        {
                            ModeSettings = new ModeSettings(),
                            DifficultySettings = new DifficultySettings(),
                        }
                    },
                    new FillFlowContainer<SettingsGroup>
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Children = new SettingsGroup[]
                        {
                            ColoursSettings = new ColoursSettings(),
                        }
                    },
                    new FillFlowContainer<SettingsGroup>
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Children = new SettingsGroup[]
                        {
                            AudioSettings = new AudioSettings(),
                        }
                    },
                    new FillFlowContainer<SettingsGroup>
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 20),
                        Children = new SettingsGroup[]
                        {
                            AdvancedSettings = new AdvancedSettings(),
                            DesignSettings = new DesignSettings(),
                        }
                    },
                }
            };
            ModeSettings.AvailableModesChanged += a =>
            {
                if (a == AvailableModes.Mania || a == AvailableModes.Taiko)
                    DifficultySettings.HideApproachRateAndCircleSize();
                else
                    DifficultySettings.ShowApproachRateAndCircleSize();
            };
        }
    }
}
