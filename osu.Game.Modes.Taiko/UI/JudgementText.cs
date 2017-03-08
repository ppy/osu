// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Modes.Taiko.UI
{
    public class JudgementText : Container
    {
        /// <summary>
        /// The colour of the backing glow of the text.
        /// </summary>
        public Color4 GlowColour;

        /// <summary>
        /// The judgement text.
        /// </summary>
        public string Text;

        /// <summary>
        /// The direction to move in.
        /// </summary>
        public float Direction;

        private OsuSpriteText glowText;
        private OsuSpriteText normalText;

        public JudgementText()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new BufferedContainer
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
                        glowText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,

                            Font = "Venera",
                            TextSize = 22f,
                        }
                    }
                },
                normalText = new OsuSpriteText
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

            ScaleTo(1.5f, 250, EasingTypes.OutQuint);
            MoveToY(Direction * 100, 500);

            Delay(250);
            ScaleTo(0.75f, 250);
            FadeOut(250);

            Expire();
        }
    }
}
