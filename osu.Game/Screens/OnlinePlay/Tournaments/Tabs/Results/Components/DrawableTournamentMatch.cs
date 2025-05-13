// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results.Components
{
    public partial class DrawableTournamentMatch : CompositeDrawable
    {
        public readonly TournamentMatch Match;
        protected readonly FillFlowContainer<DrawableMatchTeam> Flow;
        private readonly Drawable selectionBox;
        private readonly Drawable currentMatchSelectionBox;
        private Bindable<TournamentMatch>? globalSelection;

        [Resolved]
        private BracketScreen bracketScreen { get; set; } = null!;

        [Resolved]
        private TournamentInfo tournamentInfo { get; set; } = null!;

        // todo : editor is unused, idk what its purpose was. Might be useful
        public DrawableTournamentMatch(TournamentMatch match, bool editor = false)
        {
            Match = match;
            // this.editor = editor;

            AutoSizeAxes = Axes.Both;

            const float border_thickness = 5;
            const float spacing = 2;

            Margin = new MarginPadding(10);

            InternalChildren = new Drawable[]
            {
                Flow = new FillFlowContainer<DrawableMatchTeam>
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(spacing)
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(-10),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = selectionBox = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Masking = true,
                        BorderColour = Color4.YellowGreen,
                        BorderThickness = border_thickness,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            AlwaysPresent = true,
                            Alpha = 0,
                        }
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(-(spacing + border_thickness)),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = currentMatchSelectionBox = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        BorderColour = Color4.White,
                        BorderThickness = border_thickness,
                        Masking = true,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            AlwaysPresent = true,
                            Alpha = 0,
                        }
                    },
                }
            };

            boundReference(match.Teams).BindCollectionChanged((temp1, temp2) => updateTeams());
            match.TeamScores.ForEach(score => boundScore(score).BindValueChanged(_ => updateWinConditions()));
            boundReference(match.Round).BindValueChanged(_ =>
            {
                updateWinConditions();
                Changed?.Invoke();
            });
            boundReference(match.Completed).BindValueChanged(_ => updateProgression());
            boundReference(match.Progression).BindValueChanged(_ => updateProgression());
            boundReference(match.LosersProgression).BindValueChanged(_ => updateProgression());
            boundReference(match.Losers).BindValueChanged(_ =>
            {
                updateTeams();
                Changed?.Invoke();
            });
            boundReference(match.Current).BindValueChanged(_ => updateCurrentMatch(), true);
            boundReference(match.Position).BindValueChanged(pos =>
            {
                if (!IsDragged)
                    Position = new Vector2(pos.NewValue.X, pos.NewValue.Y);
                Changed?.Invoke();
            }, true);
            updateTeams();
            Console.WriteLine("Created DrawableMatch");
        }

        /// <summary>
        /// Fired when something changed that requires a ladder redraw.
        /// </summary>
        public Action? Changed;

        private readonly List<IUnbindable> refBindables = new List<IUnbindable>();
        private readonly List<IUnbindable> scoreBindables = new List<IUnbindable>();

        private T boundScore<T>(T obj)
            where T : IBindable
        {
            obj = (T)obj.GetBoundCopy();
            scoreBindables.Add(obj);
            return obj;
        }

        private T boundReference<T>(T obj)
            where T : IBindable
        {
            obj = (T)obj.GetBoundCopy();
            refBindables.Add(obj);
            return obj;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            foreach (var b in refBindables)
                b.UnbindAll();
            foreach (var b in scoreBindables)
                b.UnbindAll();
        }

        private void updateCurrentMatch()
        {
            if (Match.Current.Value)
                currentMatchSelectionBox.Show();
            else
                currentMatchSelectionBox.Hide();
        }

        private bool selected;

        public bool Selected
        {
            get => selected;

            set
            {
                if (value == selected) return;

                selected = value;

                if (selected)
                {
                    selectionBox.Show();
                    // todo : not sure if this translated correctly
                    if (tournamentInfo.IsEditing.Value)
                        bracketScreen.Selected.Value = Match;
                    else
                        tournamentInfo.CurrentMatch.Value = Match;
                }
                else
                    selectionBox.Hide();
            }
        }

        private void updateProgression()
        {
            if (!Match.Completed.Value)
            {
                // ensure we clear any of our teams from our progression.
                // this is not pretty logic but should suffice for now.
                // if (Match.Progression.Value != null && Match.Progression.Value.Team1.Value == Match.Team1.Value)
                //     Match.Progression.Value.Team1.Value = null;

                // if (Match.Progression.Value != null && Match.Progression.Value.Team2.Value == Match.Team2.Value)
                //     Match.Progression.Value.Team2.Value = null;

                // if (Match.LosersProgression.Value != null && Match.LosersProgression.Value.Team1.Value == Match.Team1.Value)
                //     Match.LosersProgression.Value.Team1.Value = null;

                // if (Match.LosersProgression.Value != null && Match.LosersProgression.Value.Team2.Value == Match.Team2.Value)
                //     Match.LosersProgression.Value.Team2.Value = null;
            }
            else
            {
                var winners = Match.GetWinners();
                var losers = Match.GetLosers();
                Debug.Assert(winners.Count != 0);
                Debug.Assert(losers.Count != 0);
                Debug.Assert(winners.Count + losers.Count == Match.Teams.Count);
                // transferProgression(Match.Progression.Value, Match.Winner);
                // transferProgression(Match.LosersProgression.Value, Match.Loser);
            }

            Changed?.Invoke();
        }

        // Does this do anything?
        // private void transferProgression(TournamentMatch? destination, TournamentTeam team)
        // {
        //     if (destination == null) return;

        //     bool progressionAbove = destination.ID < Match.ID;

        //     Bindable<TournamentTeam?> destinationTeam;

        //     // check for the case where we have already transferred out value
        //     if (destination.Team1.Value == team)
        //         destinationTeam = destination.Team1;
        //     else if (destination.Team2.Value == team)
        //         destinationTeam = destination.Team2;
        //     else
        //     {
        //         destinationTeam = progressionAbove ? destination.Team2 : destination.Team1;
        //         if (destinationTeam.Value != null)
        //             destinationTeam = progressionAbove ? destination.Team1 : destination.Team2;
        //     }

        //     destinationTeam.Value = team;
        // }

        private void updateWinConditions()
        {
            // if (Match.Round.Value == null)
            //     return;

            // todo : Create function for determining who won a match. Also figure out how to display a winner/winners.

            // int instantWinAmount = Match.Round.Value.BestOf.Value / 2;

            // Match.Completed.Value = Match.Round.Value.BestOf.Value > 0
            //                         && (Match.Team1Score.Value + Match.Team2Score.Value >= Match.Round.Value.BestOf.Value || Match.Team1Score.Value > instantWinAmount
            //                                                                                                               || Match.Team2Score.Value > instantWinAmount);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateTeams();

            if (tournamentInfo.IsEditing.Value)
            {
                globalSelection = bracketScreen.Selected.GetBoundCopy();
                globalSelection.BindValueChanged(s => Selected = s.NewValue == Match, true);
            }
        }

        private void updateTeams()
        {
            if (LoadState != LoadState.Loaded)
                return;

            // todo: teams may need to be bindable for transitions at a later point.

            // if (Match.Team1.Value == null || Match.Team2.Value == null)
            //     Match.CancelMatchStart();

            // if (Match.ConditionalMatches.Count > 0)
            // {
            //     foreach (var conditional in Match.ConditionalMatches)
            //     {
            //         bool team1Match = Match.Team1Acronym != null && conditional.Acronyms.Contains(Match.Team1Acronym);
            //         bool team2Match = Match.Team2Acronym != null && conditional.Acronyms.Contains(Match.Team2Acronym);

            //         if (team1Match && team2Match)
            //             Match.Date.Value = conditional.Date.Value;
            //     }
            // }

            // Teams has potentially changed size and therefore created/removed new bindable object
            // We need to rebind them
            foreach (var b in scoreBindables)
                b.UnbindAll();
            Match.TeamScores.ForEach(score => boundScore(score).BindValueChanged(_ => updateWinConditions()));

            Flow.Children = (from team in Match.Teams select new DrawableMatchTeam(team, Match, Match.Losers.Value)).ToArray();

            SchedulerAfterChildren.Add(() => Scheduler.Add(updateProgression));
            updateWinConditions();
        }

        protected override bool OnMouseDown(MouseDownEvent e) => e.Button == MouseButton.Left && tournamentInfo.IsEditing.Value;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (Selected && tournamentInfo.IsEditing.Value && e.Key == Key.Delete)
            {
                Remove();
                return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!tournamentInfo.IsEditing.Value || Match is ConditionalTournamentMatch || e.Button != MouseButton.Left)
                return false;

            Selected = true;
            return true;
        }

        private Vector2 positionAtStartOfDrag;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (tournamentInfo.IsEditing.Value)
            {
                positionAtStartOfDrag = Position;
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            Selected = true;

            this.MoveTo(snapToGrid(positionAtStartOfDrag + (e.MousePosition - e.MouseDownPosition)));

            Match.Position.Value = new Point((int)Position.X, (int)Position.Y);
        }

        private Vector2 snapToGrid(Vector2 pos) =>
            new Vector2(
                (int)(pos.X / BracketScreen.GRID_SPACING) * BracketScreen.GRID_SPACING,
                (int)(pos.Y / BracketScreen.GRID_SPACING) * BracketScreen.GRID_SPACING
            );

        public void Remove()
        {
            Selected = false;
            Match.Progression.Value = null;
            Match.LosersProgression.Value = null;

            // todo : Probably not needed
            if (!tournamentInfo.IsEditing.Value)
                return;

            tournamentInfo.Matches.Remove(Match);

            foreach (var m in tournamentInfo.Matches)
            {
                if (m.Progression.Value == Match)
                    m.Progression.Value = null;

                if (m.LosersProgression.Value == Match)
                    m.LosersProgression.Value = null;
            }
        }
    }
}
