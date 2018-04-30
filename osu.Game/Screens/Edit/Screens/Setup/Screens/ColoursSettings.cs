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
    public class ColoursSettings : EditorSettingsGroup
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
                        comboColours[i].Alpha = 1;
                }
                else
                {
                    for (int i = usedComboColours - 1; i >= value; i--)
                        comboColours[i].Alpha = 0;
                }
                // Cannot use lambda expressions and a null at the same time in a conditional statement using ?:
                if (value == 8)
                    addComboColour.Action = null;
                else
                    addComboColour.Action = () => { UsedComboColours++; };
                if (value == 2)
                    removeComboColour.Action = null;
                else
                    removeComboColour.Action = () => { UsedComboColours--; };
                usedComboColours = value;
            }
        }
        private readonly TriangleButton addComboColour;
        private readonly TriangleButton removeComboColour;
        private readonly ComboColourButton[] comboColours =
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
                        comboColours[0],
                        comboColours[1],
                        comboColours[2],
                        comboColours[3],
                        comboColours[4],
                        comboColours[5],
                        comboColours[6],
                        comboColours[7],
                        addComboColour = createComboColourManagementButton("Add Combo Colour", Color4.LightBlue, () => { UsedComboColours++; }),
                        removeComboColour = createComboColourManagementButton("Remove Combo Colour", Color4.LightBlue, () => { UsedComboColours--; }),
                    },
                }
            };
            // Currently only assigned for AppVeyor to not complain for it being unused
            backgroundColour.ColourChanged += a => { };
            comboColours[0].ColourChanged += a => { };
            comboColours[1].ColourChanged += a => { };
            comboColours[2].ColourChanged += a => { };
            comboColours[3].ColourChanged += a => { };
            comboColours[4].ColourChanged += a => { };
            comboColours[5].ColourChanged += a => { };
            comboColours[6].ColourChanged += a => { };
            comboColours[7].ColourChanged += a => { };
        }
        
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
