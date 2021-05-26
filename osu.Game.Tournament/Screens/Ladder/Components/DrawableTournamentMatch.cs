// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableTournamentMatch : CompositeDrawable
    {
        public readonly TournamentMatch Match;
        private readonly bool editor;
        protected readonly FillFlowContainer<DrawableMatchTeam> Flow;
        private readonly Drawable selectionBox;
        protected readonly Drawable CurrentMatchSelectionBox;
        private Bindable<TournamentMatch> globalSelection;

        [Resolved(CanBeNull = true)]
        private LadderEditorInfo editorInfo { get; set; }

        [Resolved(CanBeNull = true)]
        private LadderInfo ladderInfo { get; set; }

        public DrawableTournamentMatch(TournamentMatch match, bool editor = false)
        {
            Match = match;
            this.editor = editor;

            AutoSizeAxes = Axes.Both;

            Margin = new MarginPadding(5);

            InternalChildren = new[]
            {
                selectionBox = new Container
                {
                    Scale = new Vector2(1.1f),
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Colour = Color4.YellowGreen,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                CurrentMatchSelectionBox = new Container
                {
                    Scale = new Vector2(1.05f, 1.1f),
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Colour = Color4.White,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                Flow = new FillFlowContainer<DrawableMatchTeam>
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2)
                }
            };

            boundReference(match.Team1).BindValueChanged(_ => updateTeams());
            boundReference(match.Team2).BindValueChanged(_ => updateTeams());
            boundReference(match.Team1Score).BindValueChanged(_ => updateWinConditions());
            boundReference(match.Team2Score).BindValueChanged(_ => updateWinConditions());
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
        }

        /// <summary>
        /// Fired when somethign changed that requires a ladder redraw.
        /// </summary>
        public Action Changed;

        private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

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
        }

        private void updateCurrentMatch()
        {
            if (Match.Current.Value)
                CurrentMatchSelectionBox.Show();
            else
                CurrentMatchSelectionBox.Hide();
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
                    if (editor && editorInfo != null)
                        editorInfo.Selected.Value = Match;
                    else if (ladderInfo != null)
                        ladderInfo.CurrentMatch.Value = Match;
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
                if (Match.Progression.Value != null && Match.Progression.Value.Team1.Value == Match.Team1.Value)
                    Match.Progression.Value.Team1.Value = null;

                if (Match.Progression.Value != null && Match.Progression.Value.Team2.Value == Match.Team2.Value)
                    Match.Progression.Value.Team2.Value = null;

                if (Match.LosersProgression.Value != null && Match.LosersProgression.Value.Team1.Value == Match.Team1.Value)
                    Match.LosersProgression.Value.Team1.Value = null;

                if (Match.LosersProgression.Value != null && Match.LosersProgression.Value.Team2.Value == Match.Team2.Value)
                    Match.LosersProgression.Value.Team2.Value = null;
            }
            else
            {
                transferProgression(Match.Progression?.Value, Match.Winner);
                transferProgression(Match.LosersProgression?.Value, Match.Loser);
            }

            Changed?.Invoke();
        }

        private void transferProgression(TournamentMatch destination, TournamentTeam team)
        {
            if (destination == null) return;

            bool progressionAbove = destination.ID < Match.ID;

            Bindable<TournamentTeam> destinationTeam;

            // check for the case where we have already transferred out value
            if (destination.Team1.Value == team)
                destinationTeam = destination.Team1;
            else if (destination.Team2.Value == team)
                destinationTeam = destination.Team2;
            else
            {
                destinationTeam = progressionAbove ? destination.Team2 : destination.Team1;
                if (destinationTeam.Value != null)
                    destinationTeam = progressionAbove ? destination.Team1 : destination.Team2;
            }

            destinationTeam.Value = team;
        }

        private void updateWinConditions()
        {
            if (Match.Round.Value == null) return;

            var instaWinAmount = Match.Round.Value.BestOf.Value / 2;

            Match.Completed.Value = Match.Round.Value.BestOf.Value > 0
                                    && (Match.Team1Score.Value + Match.Team2Score.Value >= Match.Round.Value.BestOf.Value || Match.Team1Score.Value > instaWinAmount || Match.Team2Score.Value > instaWinAmount);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateTeams();

            if (editorInfo != null)
            {
                globalSelection = editorInfo.Selected.GetBoundCopy();
                globalSelection.BindValueChanged(s =>
                {
                    if (s.NewValue != Match) Selected = false;
                });
            }
        }

        private void updateTeams()
        {
            if (LoadState != LoadState.Loaded)
                return;

            // todo: teams may need to be bindable for transitions at a later point.

            if (Match.Team1.Value == null || Match.Team2.Value == null)
                Match.CancelMatchStart();

            if (Match.ConditionalMatches.Count > 0)
            {
                foreach (var conditional in Match.ConditionalMatches)
                {
                    var team1Match = conditional.Acronyms.Contains(Match.Team1Acronym);
                    var team2Match = conditional.Acronyms.Contains(Match.Team2Acronym);

                    if (team1Match && team2Match)
                        Match.Date.Value = conditional.Date.Value;
                }
            }

            Flow.Children = new[]
            {
                new DrawableMatchTeam(Match.Team1.Value, Match, Match.Losers.Value),
                new DrawableMatchTeam(Match.Team2.Value, Match, Match.Losers.Value)
            };

            SchedulerAfterChildren.Add(() => Scheduler.Add(updateProgression));
            updateWinConditions();
        }

        protected override bool OnMouseDown(MouseDownEvent e) => e.Button == MouseButton.Left && editorInfo != null;

        protected override bool OnDragStart(DragStartEvent e) => editorInfo != null;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (Selected && editorInfo != null && e.Key == Key.Delete)
            {
                Remove();
                return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (editorInfo == null || Match is ConditionalTournamentMatch)
                return false;

            Selected = true;
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            Selected = true;
            this.MoveToOffset(e.Delta);

            var pos = Position;
            Match.Position.Value = new Point((int)pos.X, (int)pos.Y);
        }

        public void Remove()
        {
            Selected = false;
            Match.Progression.Value = null;
            Match.LosersProgression.Value = null;

            ladderInfo.Matches.Remove(Match);
        }
    }
}
