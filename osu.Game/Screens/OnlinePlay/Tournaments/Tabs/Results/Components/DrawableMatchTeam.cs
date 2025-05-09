// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.OnlinePlay.Tournaments.Components;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results.Components
{
    public partial class DrawableMatchTeam : DrawableTournamentTeam, IHasContextMenu
    {
        private readonly TournamentMatch match;
        private readonly bool losers;
        private TournamentSpriteText scoreText = null!;
        private Box background = null!;
        private Box backgroundRight = null!;

        // todo : Add option per round to change what type of score is displayed based on the criteria for winning the match.
        // This can be standard map points, map score, map accuracy
        // private ScoreDisplayType scoreDisplayType;

        private readonly Bindable<int?> score = new Bindable<int?>();
        private readonly BindableBool completed = new BindableBool();

        private Color4 colourWinner;

        [Resolved]
        private TournamentInfo tournamentInfo { get; set; } = null!;

        [Resolved]
        private BracketScreen bracketScreen { get; set; } = null!;

        private void setCurrent()
        {
            if (!tournamentInfo.IsEditing.Value) return;

            //todo: tournamentgamebase?
            if (tournamentInfo.CurrentMatch.Value != null)
                tournamentInfo.CurrentMatch.Value.Current.Value = false;

            tournamentInfo.CurrentMatch.Value = match;
            tournamentInfo.CurrentMatch.Value.Current.Value = true;
        }

        public DrawableMatchTeam(TournamentTeam? team, TournamentMatch match, bool losers)
            : base(team)
        {
            this.match = match;
            this.losers = losers;

            AutoSizeAxes = Axes.X;
            Height = 40;

            Flag.Scale = new Vector2(0.54f);

            // todo : i want this text to scale to fit its container
            // also AcronymText is placed 1 or 2 pixels to high.
            AcronymText.Anchor = AcronymText.Origin = Anchor.TopLeft;
            AcronymText.Font = OsuFont.Torus.With(size: 22, weight: FontWeight.Bold);

            completed.BindTo(match.Completed);
            match.Teams.BindCollectionChanged((temp1, temp2) => bindScore());
        }

        [BackgroundDependencyLoader(true)]
        private void load()
        {
            colourWinner = losers
                ? Color4Extensions.FromHex("#8E7F48")
                : Color4Extensions.FromHex("#1462AA");

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    Padding = new MarginPadding(5),
                    RelativeSizeAxes = Axes.Y,
                    Width = 210,
                    Child = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        RelativeSizeAxes = Axes.Both,
                        Spacing = new Vector2(5, 0),
                        Children = new Drawable[]
                        {
                            Flag,
                            AcronymText,
                        }
                    }
                },
                new Container
                {
                    Masking = true,
                    Width = 44,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        backgroundRight = new Box
                        {
                            Colour = OsuColour.Gray(0.1f),
                            Alpha = 0.8f,
                            RelativeSizeAxes = Axes.Both,
                        },
                        scoreText = new TournamentSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Torus.With(size: 22),
                        }
                    }
                }
            };

            completed.BindValueChanged(_ => updateWinStyle());

            bindScore();
            updateWinStyle(); // In case not run in BindScore
        }

        private MatchTeamStatus getMatchTeamStatus()
        {
            // todo : implement logic
            return MatchTeamStatus.Unplayed;
        }

        private void bindScore()
        {
            if (Team == null)
                return;

            Debug.Assert(match.Teams.IndexOf(Team) != -1, "Team should exist in Match Team list, otherwise this object should be destroyed.");
            Debug.Assert(match.Teams.IndexOf(Team) < match.TeamScores.Count, "TeamScores' size should update before this function.");
            score.UnbindAll();
            score.BindTo(match.TeamScores[match.Teams.IndexOf(Team)]);
            score.BindValueChanged(val =>
            {
                scoreText.Text = val.NewValue?.ToString() ?? string.Empty;
                updateWinStyle();
            }, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Team == null || !tournamentInfo.IsEditing.Value) return false;

            if (!match.Current.Value)
            {
                setCurrent();
                return true;
            }

            if (e.Button == MouseButton.Left)
            {
                Console.WriteLine("Pressed Team");

                if (score.Value == null)
                    match.StartMatch();
                else if (!match.Completed.Value)
                    score.Value++;
            }
            else
            {
                // todo : Currently OsuContextMenuContainer eats right click input. Find a way around that.
                Console.WriteLine("Trying to remove point.1");
                if (match.Progression.Value?.Completed.Value == true)
                    // don't allow changing scores if the match has a progression. can cause large data loss
                    return false;

                Console.WriteLine("Trying to remove point.2");
                if (match.Completed.Value && !match.GetWinners().Contains(Team))
                    // don't allow changing scores from the non-winner
                    return false;

                Console.WriteLine("Trying to remove point.3");
                if (score.Value > 0)
                    score.Value--;
                else
                    match.CancelMatchStart();
            }

            return false;
        }

        private void updateWinStyle()
        {
            // todo : This is where i add checks for being eliminated, protected, winning, losing and not played.
            bool winner = completed.Value;
            // bool winner = completed.Value && isWinner?.Invoke() == true;

            background.FadeColour(winner ? Color4.White : Color4Extensions.FromHex("#444"), winner ? 500 : 0, Easing.OutQuint);
            backgroundRight.FadeColour(winner ? colourWinner : Color4Extensions.FromHex("#333"), winner ? 500 : 0, Easing.OutQuint);

            AcronymText.Colour = winner ? Color4.Black : Color4.White;

            scoreText.Font = scoreText.Font.With(weight: winner ? FontWeight.Bold : FontWeight.Regular);
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!tournamentInfo.IsEditing.Value)
                    return Array.Empty<MenuItem>();

                return new MenuItem[]
                {
                    new OsuMenuItem("Set as current", MenuItemType.Standard, setCurrent),
                    new OsuMenuItem("Join with", MenuItemType.Standard, () => bracketScreen.BeginJoin(match, false)),
                    new OsuMenuItem("Join with (loser)", MenuItemType.Standard, () => bracketScreen.BeginJoin(match, true)),
                    new OsuMenuItem("Remove", MenuItemType.Destructive, () => bracketScreen.Remove(match)),
                };
            }
        }
    }

    public enum MatchTeamStatus
    {
        Unplayed,
        Winner,
        Loser,
        Eliminated,
        Qualified,
    }
}
