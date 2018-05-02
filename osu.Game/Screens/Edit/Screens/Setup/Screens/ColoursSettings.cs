// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class ColoursSettings : SettingsGroup
    {
        private int usedComboColours = 5;
        public int UsedComboColours
        {
            get => usedComboColours;
            set
            {
                if (value > usedComboColours)
                {
                    for (int i = usedComboColours; i < value; i++)
                        comboColourButtons[i].Alpha = 1;
                }
                else
                {
                    for (int i = usedComboColours - 1; i >= value; i--)
                        comboColourButtons[i].Alpha = 0;
                }
                // I don't know how "healthy" this statement is
                addComboColourButton.Action = value == 8 ? null as Action : addComboColour;
                removeComboColourButton.Action = value == 2 ? null as Action : removeComboColour;
                usedComboColours = value;
            }
        }
        private readonly TriangleButton addComboColourButton;
        private readonly TriangleButton removeComboColourButton;
        private readonly ComboColourButton[] comboColourButtons =
        {
            createComboColourSettingButton(1, Color4.Magenta, 1),
            createComboColourSettingButton(2, Color4.Blue, 1),
            createComboColourSettingButton(3, Color4.LimeGreen, 1),
            createComboColourSettingButton(4, Color4.Red, 1),
            createComboColourSettingButton(5, Color4.DarkBlue, 1),
            createComboColourSettingButton(6, Color4.Orange, 0),
            createComboColourSettingButton(7, Color4.GreenYellow, 0),
            createComboColourSettingButton(8, Color4.Pink, 0),
        };

        protected override string Title => @"colours";

        public ColoursSettings()
        {
            AllowCollapsing = false;

            ComboColourButton backgroundColour;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        backgroundColour = createComboColourSettingButton("Background Colour", Color4.LightBlue, 1),
                        comboColourButtons[0],
                        comboColourButtons[1],
                        comboColourButtons[2],
                        comboColourButtons[3],
                        comboColourButtons[4],
                        comboColourButtons[5],
                        comboColourButtons[6],
                        comboColourButtons[7],
                        addComboColourButton = createComboColourManagementButton("Add Combo Colour", Color4.LightBlue, () => { UsedComboColours++; }),
                        removeComboColourButton = createComboColourManagementButton("Remove Combo Colour", Color4.LightBlue, () => { UsedComboColours--; }),
                    },
                }
            };
            // Currently only assigned for AppVeyor to not complain for it being unused
            backgroundColour.ColourChanged += a => { };
            comboColourButtons[0].ColourChanged += a => { };
            comboColourButtons[1].ColourChanged += a => { };
            comboColourButtons[2].ColourChanged += a => { };
            comboColourButtons[3].ColourChanged += a => { };
            comboColourButtons[4].ColourChanged += a => { };
            comboColourButtons[5].ColourChanged += a => { };
            comboColourButtons[6].ColourChanged += a => { };
            comboColourButtons[7].ColourChanged += a => { };
        }

        private void addComboColour() => UsedComboColours++;
        private void removeComboColour() => UsedComboColours--;
        private static TriangleButton createSettingButton(string text) => new TriangleButton
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
            Height = 20,
        };
        private static ComboColourButton createComboColourSettingButton(string text, Color4 backgroundColour, float alpha)
        {
            ComboColourButton a = new ComboColourButton(backgroundColour)
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Text = text,
                Height = 30,
                Alpha = alpha,
            };
            a.Action = () => a.MainColour = showColourPickerDialog();
            return a;
        }
        private static ComboColourButton createComboColourSettingButton(int colourIndex, Color4 backgroundColour, float alpha)
        {
            ComboColourButton a = new ComboColourButton(backgroundColour)
            {
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Text = $"Colour {colourIndex}",
                Height = 30,
                Alpha = alpha
            };
            a.Action = () => a.MainColour = showColourPickerDialog();
            return a;
        }
        private static TriangleButton createComboColourManagementButton(string text, Color4 backgroundColour, Action action) => new TriangleButton
        {
            RelativeSizeAxes = Axes.X,
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
            BackgroundColour = backgroundColour,
            Height = 30,
            Action = action
        };
        private static OsuSpriteText createSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
        };
        private static OsuSpriteText createSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };

        private static Color4 showColourPickerDialog()
        {
            // Implement a feature to bring a colour picker dialog
            return Color4.Blue;
        }
    }
}
