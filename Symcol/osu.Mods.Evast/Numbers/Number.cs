// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Evast.Numbers
{
    public class Number : Container
    {
        private int colourPointer;

        private int value;
        public int Value => value;

        public bool IsLocked;
        public void Lock() => IsLocked = true;

        private Vector2 coordinates;
        public Vector2 Coordinates
        {
            set { coordinates = value; }
            get { return coordinates; }
        }

        private readonly OsuSpriteText numberText;
        private readonly Box background;

        public Number()
        {
            value = RNG.NextBool(0.9) ? 2 : 4;
            if (value == 4) colourPointer++;

            Anchor = Anchor.TopLeft;
            Origin = Anchor.Centre;
            Size = new Vector2(100);
            Scale = new Vector2(0);
            Alpha = 0;
            CornerRadius = 6;
            Masking = true;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours[colourPointer],
                },
                numberText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = 50,
                    Text = value.ToString(),
                    Font = @"Exo2.0-Bold",
                    Colour = OsuColour.FromHex(@"776E65"),
                    Shadow = false,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.ScaleTo(1, 120);
            this.FadeTo(1, 120);
        }

        public void IncreaseValue() => value *= 2;

        public void IncreaseValueAnimation()
        {
            numberText.Text = value.ToString();

            if (value == 8)
                numberText.Colour = Color4.White;

            colourPointer++;
            background.Colour = colours[colourPointer];

            this.ScaleTo(1.2f, 40, Easing.OutQuint).Then().ScaleTo(1, 160, Easing.OutQuint);
        }

        private static readonly Color4[] colours =
        {
            Color4.White,
            OsuColour.FromHex("EDE0C8"),//4
            OsuColour.FromHex("F3B179"),//8
            OsuColour.FromHex("F59663"),//16
            OsuColour.FromHex("F77C5F"),//32
            OsuColour.FromHex("F65E3C"),//64
            OsuColour.FromHex("EDCF72"),//128
            OsuColour.FromHex("ECCC61"),//256
            OsuColour.FromHex("EDC750"),//512
            OsuColour.FromHex("EEC53F"),//1024
            OsuColour.FromHex("ECC22D"),//2048
            OsuColour.FromHex("ECC22D"),//4096
            OsuColour.FromHex("0011FF"),//8182
        };
    }
}
