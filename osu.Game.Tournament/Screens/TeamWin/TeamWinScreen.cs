// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.TeamWin
{
    public class TeamWinScreen : TournamentScreen, IProvideVideo
    {
        private Container mainContainer;

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();
        private readonly Bindable<bool> currentCompleted = new Bindable<bool>();

        private TourneyVideo blueWinVideo;
        private TourneyVideo redWinVideo;

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, Storage storage)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                blueWinVideo = new TourneyVideo("teamwin-blue")
                {
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                redWinVideo = new TourneyVideo("teamwin-red")
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Loop = true,
                },
                mainContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };

            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            currentCompleted.BindValueChanged(_ => update());
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            currentCompleted.UnbindBindings();
            currentCompleted.BindTo(match.NewValue.Completed);

            update();
        }

        private bool firstDisplay = true;

        private void update() => Schedule(() =>
        {
            var match = currentMatch.Value;

            if (match.Winner == null)
            {
                mainContainer.Clear();
                return;
            }

            redWinVideo.Alpha = match.WinnerColour == TeamColour.Red ? 1 : 0;
            blueWinVideo.Alpha = match.WinnerColour == TeamColour.Blue ? 1 : 0;

            if (firstDisplay)
            {
                if (match.WinnerColour == TeamColour.Red)
                    redWinVideo.Reset();
                else
                    blueWinVideo.Reset();
                firstDisplay = false;
            }

            mainContainer.Children = new Drawable[]
            {
                new DrawableTeamFlag(match.Winner)
                {
                    Size = new Vector2(300, 200),
                    Scale = new Vector2(0.5f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = new Vector2(-300, 10),
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    X = 260,
                    Children = new Drawable[]
                    {
                        new RoundDisplay(match)
                        {
                            Margin = new MarginPadding { Bottom = 30 },
                        },
                        new TournamentSpriteText
                        {
                            Text = "WINNER",
                            Font = OsuFont.Torus.With(size: 100, weight: FontWeight.Bold),
                            Margin = new MarginPadding { Bottom = 50 },
                        },
                        new DrawableTeamWithPlayers(match.Winner, match.WinnerColour)
                    }
                },
            };
            mainContainer.FadeOut();
            mainContainer.Delay(2000).FadeIn(1600, Easing.OutQuint);
        });
    }
}
