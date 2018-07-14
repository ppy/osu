// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class ColoursScreen : EditorScreen
    {
        private readonly FillFlowContainer comboColourButtonContainer;
        private readonly NewComboColourButton newComboColourButton;
        private readonly OsuCircularButton removeComboColourButton;
        private readonly OsuSpriteText backgroundColourBottomLabel;

        private int currentComboColours = 5;

        public Color4[] DefaultComboColours { get; private set; } = new Color4[8];

        public ColoursScreen()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = Setup.SCREEN_LEFT_PADDING, Top = Setup.SCREEN_TOP_PADDING },
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(3),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Colour = Color4.White,
                                    Text = "Hitcircle / Slider Combo Colours",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    Height = 170,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 170,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    CornerRadius = 15,
                                                    Masking = true,
                                                    Height = 160,
                                                    Children = new Drawable[]
                                                    {
                                                        new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = OsuColour.FromHex("1c2125"),
                                                        },
                                                    }
                                                },
                                                removeComboColourButton = new OsuCircularButton
                                                {
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.BottomRight,
                                                    Margin = new MarginPadding { Bottom = 15, Right = 15 },
                                                    LabelText = "Remove Combo Colour",
                                                    Width = 175,
                                                },
                                                comboColourButtonContainer = new FillFlowContainer
                                                {
                                                    Direction = FillDirection.Horizontal,
                                                    Anchor = Anchor.TopLeft,
                                                    Origin = Anchor.TopLeft,
                                                    Height = 100,
                                                    Padding = new MarginPadding { Left = 15, Top = 15, Right = 15 },
                                                    //Position = new Vector2(15),
                                                    Spacing = new Vector2(5),
                                                },
                                            }
                                        },
                                    }
                                },
                                new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Top = 10 },
                                    Colour = Color4.White,
                                    Text = "Playfield Background",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    Height = 160,
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 160,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    CornerRadius = 15,
                                                    Masking = true,
                                                    Height = 160,
                                                    Children = new Drawable[]
                                                    {
                                                        new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = OsuColour.FromHex("1c2125"),
                                                        },
                                                    }
                                                },
                                                backgroundColourBottomLabel = new OsuSpriteText
                                                {
                                                    Anchor = Anchor.BottomLeft,
                                                    Origin = Anchor.BottomLeft,
                                                    Margin = new MarginPadding { Bottom = 15, Left = 15 },
                                                    Text = "Please note that this colour is dimmed in the editor and during gameplay. It will be the exact colour during break time.",
                                                    TextSize = 12,
                                                    Font = @"Exo2.0-BoldItalic",
                                                },
                                                new OsuColourButton(true)
                                                {
                                                    Anchor = Anchor.TopLeft,
                                                    Origin = Anchor.TopLeft,
                                                    Position = new Vector2(15),
                                                    BottomLabelText = "Background Colour"
                                                },
                                            }
                                        },
                                    }
                                },
                            }
                        },
                    },
                },
            };

            for (int i = 1; i <= 8; i++)
                comboColourButtonContainer.Add(new OsuColourButton
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    BottomLabelText = $"Combo {i}"
                });
            // ReSharper disable once PossibleNullReferenceException
            (comboColourButtonContainer[7] as OsuColourButton).ColourPickerOrigin = Anchor.TopRight;
            comboColourButtonContainer.Add(newComboColourButton = new NewComboColourButton
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
            });

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            newComboColourButton.ButtonClicked += AddNewComboColour;
            removeComboColourButton.ButtonClicked += RemoveLastComboColour;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            // The first 5 colours are taken from osu!stable and the last 3 are 
            DefaultComboColours = new[]
            {
                osuColour.PurpleDark,
                osuColour.Blue,
                osuColour.Green,
                osuColour.RedLight,
                osuColour.BlueDarker,
                osuColour.Yellow,
                osuColour.Pink,
                osuColour.RedDark,
            };

            setDefaultColours();

            removeComboColourButton.DefaultColour = osuColour.BlueDark;
            backgroundColourBottomLabel.Colour = osuColour.Yellow;
        }

        private void setDefaultColours()
        {
            for (int i = 0; i < 8; i++)
                // ReSharper disable once PossibleNullReferenceException
                (comboColourButtonContainer[i] as OsuColourButton).Current.Value = DefaultComboColours[i];
        }

        private void updateInfo()
        {
            // currentComboColours = Beatmap?.Value.BeatmapInfo.ComboColours.Count;
            setDefaultColours(); // Replace with function that sets the actual beatmap colours once available
            for (int i = 0; i < 8; i++)
                comboColourButtonContainer[i].Alpha = Convert.ToInt32(i < currentComboColours);
            newComboColourButton.Alpha = Convert.ToInt32(currentComboColours < 8);
            removeComboColourButton.Disabled = currentComboColours <= 2;
        }

        public void AddNewComboColour()
        {
            if (currentComboColours == 8)
                return;

            currentComboColours++;
            updateInfo();
        }

        public void RemoveLastComboColour()
        {
            if (currentComboColours == 2)
                return;

            currentComboColours--;
            updateInfo();
        }
    }
}
