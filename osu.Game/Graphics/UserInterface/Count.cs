//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Graphics.UserInterface
{
    public class Count : Container
    {
        protected const double FadingTime = 200;

        protected Sprite glow;
        protected Sprite hit;
        protected Sprite up;
        protected Container text;
        protected SpriteText headerText;
        protected SpriteText countText;

        protected string header;
        public string Header
        {
            get { return header; }
            set
            {
                header = value;
                if (headerText != null)
                    headerText.Text = value;
            }
        }

        public int counter = 0;
        public int Counter
        {
            get { return counter; }
            protected set
            {
                counter = value;
                if (countText != null)
                    countText.Text = value.ToString("#,##0");
            }
        }

        public bool lit = false;
        public bool IsLit
        {
            get { return lit; }
            set
            {
                if (IsCounting)
                {
                    if (!lit && value)
                    {
                        Counter++;
                        hit.Alpha = 1;
                        text.Colour = OpenTK.Graphics.Color4.DimGray;
                        glow.FadeIn(FadingTime);
                    }
                    if (lit && !value)
                    {
                        hit.Alpha = 0;
                        text.Colour = OpenTK.Graphics.Color4.White;
                        glow.FadeOut(FadingTime);
                    }
                    lit = value;
                }
            }
        }

        public bool counting = true;
        public bool IsCounting
        {
            get { return counting; }
            set
            {
                if (!value)
                    IsLit = false;
                counting = value;
            }
        }

        public override void Load()
        {
            base.Load();

            Children = new Drawable[]
            {
                glow = new Sprite
                {
                    Texture = Game.Textures.Get(@"KeyCounter/key-glow"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                },
                hit = new Sprite
                {
                    Texture = Game.Textures.Get(@"KeyCounter/key-hit"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0
                },
                up = new Sprite
                {
                    Texture = Game.Textures.Get(@"KeyCounter/key-up"),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                text = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    SizeMode = InheritMode.XY,
                    Children = new Drawable[]
                    {
                        headerText = new SpriteText
                        {
                            Text = header,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            PositionMode = InheritMode.XY,
                            Position = new Vector2(0, 0.125f)
                        },
                        countText = new SpriteText
                        {
                            Text = Counter.ToString("#,##0"),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            PositionMode = InheritMode.XY,
                            Position = new Vector2(0, 0.125f)
                        }
                    }
                }
            };

            Width = hit.Width;
            Height = hit.Height;
        }
    }
}
