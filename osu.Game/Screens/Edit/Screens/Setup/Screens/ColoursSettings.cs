// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Screens;
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
        private readonly ComboColourButton backgroundColour;
        private readonly TriangleButton addComboColour;
        private readonly TriangleButton removeComboColour;
        private readonly ComboColourButton[] comboColours = new ComboColourButton[]
        {
            CreateComboColourSettingButton(1, Color4.Magenta, 1),
            CreateComboColourSettingButton(2, Color4.Blue, 1),
            CreateComboColourSettingButton(3, Color4.LimeGreen, 1),
            CreateComboColourSettingButton(4, Color4.Red, 1),
            CreateComboColourSettingButton(5, Color4.DarkBlue, 1),
            CreateComboColourSettingButton(6, Color4.Orange, 0),
            CreateComboColourSettingButton(7, Color4.GreenYellow, 0),
            CreateComboColourSettingButton(8, Color4.Pink, 0),
        };

        protected override string Title => @"colours";

        public ColoursSettings()
        {
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
                        backgroundColour = CreateComboColourSettingButton("Background Colour", Color4.LightBlue, 1),
                        comboColours[0],
                        comboColours[1],
                        comboColours[2],
                        comboColours[3],
                        comboColours[4],
                        comboColours[5],
                        comboColours[6],
                        comboColours[7],
                        addComboColour = CreateComboColourManagementButton("Add Combo Colour", Color4.LightBlue, () => { UsedComboColours++; }),
                        removeComboColour = CreateComboColourManagementButton("Remove Combo Colour", Color4.LightBlue, () => { UsedComboColours--; }),
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        static TriangleButton CreateSettingButton(string text) => new TriangleButton
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
            Height = 20,
        };
        static ComboColourButton CreateComboColourSettingButton(string text, Color4 backgroundColour, float alpha)
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
            a.Action = () => a.MainColour = ShowColourPickerDialog();
            return a;
        }
        static ComboColourButton CreateComboColourSettingButton(int colourIndex, Color4 backgroundColour, float alpha)
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
            a.Action = () => a.MainColour = ShowColourPickerDialog();
            return a;
        }
        static TriangleButton CreateComboColourManagementButton(string text, Color4 backgroundColour, Action action) => new TriangleButton
        {
            RelativeSizeAxes = Axes.X,
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
            BackgroundColour = backgroundColour,
            Height = 30,
            Action = action
        };
        static OsuSpriteText CreateSettingLabelText(string text) => new OsuSpriteText
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Text = text,
        };
        static OsuSpriteText CreateSettingLabelTextBold() => new OsuSpriteText
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Font = @"Exo2.0-Bold",
        };

        static Color4 ShowColourPickerDialog()
        {
            // Implement a feature to bring a colour picker dialog
            return Color4.Blue;
        }
    }
}
