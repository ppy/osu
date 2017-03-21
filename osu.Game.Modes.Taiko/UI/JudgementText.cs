// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Modes.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public class JudgementText : Container
    {
        /// <summary>
        /// The Judgement to display.
        /// </summary>
        public TaikoJudgementInfo Judgement;

        private Container textContainer;
        private OsuSpriteText glowText;
        private OsuSpriteText normalText;

        private int movementDirection;

        public JudgementText()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                textContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    
                    AutoSizeAxes = Axes.Both,

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
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Color4 judgementColour = Color4.White;
            string judgementText = string.Empty;

            switch (Judgement.Result)
            {
                case HitResult.Miss:
                    judgementColour = colours.Red;
                    judgementText = "MISS";
                    movementDirection = 1;
                    break;
                case HitResult.Hit:
                    switch (Judgement.TaikoResult)
                    {
                        case TaikoHitResult.Good:
                            judgementColour = colours.Green;
                            judgementText = "GOOD";
                            textContainer.Scale = new Vector2(0.45f);
                            break;
                        case TaikoHitResult.Great:
                            judgementColour = colours.Blue;
                            judgementText = "GREAT";
                            break;
                    }

                    movementDirection = -1;
                    break;
            }

            glowText.Colour = judgementColour;
            glowText.Text = normalText.Text = judgementText;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScaleTo(1.5f, 250, EasingTypes.OutQuint);
            MoveToY(movementDirection * 100, 500);

            Delay(250);
            ScaleTo(0.75f, 250);
            FadeOut(250);

            Expire();
        }
    }
}