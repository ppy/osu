// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.UI
{
    public class JudgementText : Container
    {
        public Color4 GlowColour;
        public string Text;

        private OsuSpriteText glowText;
        private OsuSpriteText normalText;

        public JudgementText()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.BottomCentre;

            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new BufferedContainer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    BlurSigma = new Vector2(10),
                    CacheDrawnFrameBuffer = true,

                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(3),

                    BlendingMode = BlendingMode.Additive,

                    Children = new[]
                    {
                        glowText = new OsuSpriteText()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            Font = "Venera",
                            TextSize = 22f,
                        }
                    }
                },
                normalText = new OsuSpriteText()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Font = "Venera",
                    TextSize = 22f,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            glowText.Colour = GlowColour;
            glowText.Text = Text;
            normalText.Text = Text;

            MoveToY(-50, 500);
            FadeOut(500);
            Expire();
        }
    }
}
