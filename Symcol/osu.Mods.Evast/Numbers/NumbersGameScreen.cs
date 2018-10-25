// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Core.Screens.Evast;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Mods.Evast.Numbers
{
    public class NumbersGameScreen : BeatmapScreen
    {
        private readonly BindableInt score = new BindableInt();

        private readonly NumbersPlayfield playfield;
        private readonly OsuClickableContainer resetButton;
        private readonly OsuSpriteText scoreText;

        public NumbersGameScreen()
        {
            Children = new Drawable[]
            {
                resetButton = new OsuClickableContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 6,
                    Masking = true,
                    Margin = new MarginPadding { Top = 240 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"BBADA0"),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = @"Restart".ToUpper(),
                            Font = @"Exo2.0-Bold",
                            TextSize = 25,
                            Colour = OsuColour.FromHex(@"776E65"),
                            Shadow = false,
                            Margin = new MarginPadding { Horizontal = 10, Top = 20, Bottom = 10 }, 
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 6,
                    Masking = true,
                    Margin = new MarginPadding { Bottom = 240 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.FromHex(@"BBADA0"),
                        },
                        scoreText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-Bold",
                            Text = "0",
                            TextSize = 40,
                            Colour = OsuColour.FromHex(@"776E65"),
                            Shadow = false,
                            Margin = new MarginPadding { Horizontal = 10, Bottom = 20, Top = 10 },
                        }
                    }
                },
                playfield = new NumbersPlayfield
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };

            resetButton.Action = playfield.Reset;

            score.BindTo(playfield.Score);
            score.ValueChanged += onScoreChanged;
        }

        private void onScoreChanged(int newScore) => scoreText.Text = newScore.ToString();
    }
}
