// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents
{
    public class LabelledSwitchButton : CompositeDrawable, IHasCurrentValue<bool>
    {
        private readonly Box background;
        private readonly OsuSpriteText label;
        private readonly OsuSpriteText bottomText;
        private readonly OsuSetupSwitchButton switchButton;

        private const float corner_radius = 15;
        private const float default_label_text_size = 16;
        private const float default_bottom_label_text_size = 12;
        private const float normal_height = 40;
        private const float default_label_horizontal_offset = 15;
        private const float default_label_vertical_offset = 12;
        private const float default_switch_horizontal_offset = 15;
        private const float default_switch_vertical_offset = 10;

        public Bindable<bool> Current { get; } = new Bindable<bool>();

        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }

        public string BottomLabelText
        {
            get => bottomText.Text;
            set
            {
                bottomText.Text = value;
                Height = normal_height + (value != "" ? 20 : 0);
            }
        }

        public float LabelTextSize
        {
            get => label.TextSize;
            set => label.TextSize = value;
        }

        public Color4 LabelTextColour
        {
            get => label.Colour;
            set => label.Colour = value;
        }

        public Color4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public LabelledSwitchButton()
        {
            RelativeSizeAxes = Axes.X;
            Height = normal_height;
            Masking = true;
            CornerRadius = corner_radius;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                CornerRadius = corner_radius,
                Masking = true,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.FromHex("1c2125"),
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    label = new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Position = new Vector2(default_label_horizontal_offset, default_label_vertical_offset),
                                        Colour = Color4.White,
                                        TextSize = default_label_text_size,
                                        Font = @"Exo2.0-Bold",
                                    },
                                    switchButton = new OsuSetupSwitchButton
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Position = new Vector2(-default_switch_horizontal_offset, default_switch_vertical_offset),
                                    },
                                },
                            },
                            bottomText = new OsuSpriteText
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Position = new Vector2(default_label_horizontal_offset, -default_label_vertical_offset),
                                TextSize = default_bottom_label_text_size,
                                Font = @"Exo2.0-BoldItalic",
                            },
                        }
                    }
                }
            };

            Current.BindTo(switchButton.Current);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour)
        {
            bottomText.Colour = osuColour.Yellow;
        }
    }
}
