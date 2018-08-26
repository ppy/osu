// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Tournament.Components;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchTeam : DrawableTournamentTeam
    {
        private OsuSpriteText scoreText;
        private Box background;

        private readonly Bindable<int?> score = new Bindable<int?>();
        private readonly BindableBool completed = new BindableBool();

        private Color4 colourWinner;
        private Color4 colourNormal;

        private readonly Func<bool> isWinner;

        public DrawableMatchTeam(TournamentTeam team, MatchPairing pairing)
            : base(team)
        {
            Size = new Vector2(150, 40);

            Masking = true;
            CornerRadius = 5;

            Flag.Scale = new Vector2(0.9f);
            Flag.Anchor = Flag.Origin = Anchor.CentreLeft;

            AcronymText.Anchor = AcronymText.Origin = Anchor.CentreLeft;
            AcronymText.Padding = new MarginPadding { Left = 50 };
            AcronymText.TextSize = 24;

            if (pairing != null)
            {
                isWinner = () => pairing.Winner == Team;

                completed.BindTo(pairing.Completed);
                score.BindTo(team == pairing.Team1.Value ? pairing.Team1Score : pairing.Team2Score);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            colourWinner = colours.BlueDarker;
            colourNormal = OsuColour.Gray(0.2f);

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    Padding = new MarginPadding(5),
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        AcronymText,
                        Flag,
                        new Container
                        {
                            Masking = true,
                            CornerRadius = 5,
                            Width = 0.3f,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = OsuColour.Gray(0.1f),
                                    Alpha = 0.8f,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                scoreText = new OsuSpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    TextSize = 20,
                                }
                            }
                        }
                    }
                }
            };

            completed.BindValueChanged(_ => updateWinStyle());

            score.BindValueChanged(val =>
            {
                scoreText.Text = val?.ToString() ?? string.Empty;
                updateWinStyle();
            }, true);
        }

        private void updateWinStyle()
        {
            bool winner = completed && isWinner?.Invoke() == true;

            background.FadeColour(winner ? colourWinner : colourNormal, winner ? 500 : 0, Easing.OutQuint);

            scoreText.Font = AcronymText.Font = winner ? "Exo2.0-Bold" : "Exo2.0-Regular";
        }
    }
}
