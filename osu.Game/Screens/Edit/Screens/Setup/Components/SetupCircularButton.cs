// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System;

namespace osu.Game.Screens.Edit.Screens.Setup.Components
{
    public class SetupCircularButton : TriangleButton, IHasAccentColour
    {
        private readonly Box fill;
        private readonly OsuSpriteText label;

        public const float DEFAULT_LABEL_TEXT_SIZE = 14;
        public const float SIZE_X = 125;
        public const float SIZE_Y = 30;

        public event Action ButtonClicked;

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                fill.Colour = value;
            }
        }
        
        public string LabelText
        {
            get => label.Text;
            set => label.Text = value;
        }

        public SetupCircularButton()
        {
            Size = new Vector2(SIZE_X, SIZE_Y);

            Children = new Drawable[]
            {
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    AlwaysPresent = true,
                },
                label = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.White,
                    TextSize = DEFAULT_LABEL_TEXT_SIZE,
                    Font = @"Exo2.0-Bold",
                }
            };

            Enabled.Value = true;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Triangles.Alpha = 0;
            Content.CornerRadius = 15;
        }

        protected override bool OnClick(InputState state)
        {
            if (Enabled.Value)
                ButtonClicked?.Invoke();
            return base.OnClick(state);
        }
    }
}
