// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Menu
{
    public class IntroSequence : Container
    {
        private OsuSpriteText welcomeText;

        public IntroSequence()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                welcomeText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "welcome",
                    Font = @"Exo2.0-Light",
                    TextSize = 50,
                    Alpha = 0,
                }
            };
        }

        public void Start()
        {
            welcomeText.FadeIn(1000);
            welcomeText.TransformSpacingTo(new Vector2(20, 0), 3000, Easing.OutQuint);
        }

        public void Restart()
        {
            FinishTransforms(true);

            welcomeText.Alpha = 0;
            welcomeText.Spacing = Vector2.Zero;

            Start();
        }
    }
}
