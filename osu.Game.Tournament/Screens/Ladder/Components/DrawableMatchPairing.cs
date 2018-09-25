// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchPairing : CompositeDrawable
    {
        public readonly MatchPairing Pairing;
        private readonly FillFlowContainer<DrawableMatchTeam> flow;
        private readonly Drawable selectionBox;
        private Bindable<MatchPairing> globalSelection;

        [Resolved(CanBeNull = true)]
        private LadderEditorInfo editorInfo { get; set; } = null;

        public DrawableMatchPairing(MatchPairing pairing)
        {
            Pairing = pairing;

            Position = new Vector2(pairing.Position.X, pairing.Position.Y);

            AutoSizeAxes = Axes.Both;

            Margin = new MarginPadding(5);

            InternalChildren = new[]
            {
                selectionBox = new Container
                {
                    CornerRadius = 5,
                    Masking = true,
                    Scale = new Vector2(1.05f),
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Colour = Color4.YellowGreen,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                flow = new FillFlowContainer<DrawableMatchTeam>
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2)
                }
            };
            pairing.Team1.BindValueChanged(_ => updateTeams());
            pairing.Team2.BindValueChanged(_ => updateTeams());
            pairing.Team1Score.BindValueChanged(_ => updateWinConditions());
            pairing.Team2Score.BindValueChanged(_ => updateWinConditions());
            pairing.Grouping.BindValueChanged(_ => updateWinConditions());
            pairing.Completed.BindValueChanged(_ => updateProgression());
            pairing.Progression.BindValueChanged(_ => updateProgression());
            pairing.LosersProgression.BindValueChanged(_ => updateProgression());
            pairing.Losers.BindValueChanged(_ => updateTeams());

            updateTeams();
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
                    editorInfo.Selected.Value = Pairing;
                }
                else
                    selectionBox.Hide();
            }
        }

        private void updateProgression()
        {
            var progression = Pairing.Progression?.Value;

            if (progression != null)
            {
                bool progressionAbove = progression.ID < Pairing.ID;

                var destinationForWinner = progressionAbove || progression.Team1.Value != null && progression.Team1.Value != Pairing.Team1.Value && progression.Team1.Value != Pairing.Team2.Value ? progression.Team2 : progression.Team1;
                destinationForWinner.Value = Pairing.Winner;
            }

            if ((progression = Pairing.LosersProgression?.Value) != null)
            {
                bool progressionAbove = progression.ID < Pairing.ID;

                var destinationForLoser = progressionAbove || progression.Team1.Value != null && progression.Team1.Value != Pairing.Team1.Value && progression.Team1.Value != Pairing.Team2.Value ? progression.Team2 : progression.Team1;
                destinationForLoser.Value = Pairing.Loser;
            }
        }

        private void updateWinConditions()
        {
            if (Pairing.Grouping.Value == null) return;

            var instaWinAmount = Pairing.Grouping.Value.BestOf / 2;

            Pairing.Completed.Value = Pairing.Grouping.Value.BestOf > 0 && (Pairing.Team1Score + Pairing.Team2Score >= Pairing.Grouping.Value.BestOf || Pairing.Team1Score > instaWinAmount || Pairing.Team2Score > instaWinAmount);
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
                    if (s != Pairing) Selected = false;
                });
            }
        }

        private void updateTeams()
        {
            if (LoadState != LoadState.Loaded)
                return;

            // todo: teams may need to be bindable for transitions at a later point.

            if (Pairing.Team1.Value == null || Pairing.Team2.Value == null)
                Pairing.CancelMatchStart();

            flow.Children = new[]
            {
                new DrawableMatchTeam(Pairing.Team1, Pairing, Pairing.Losers),
                new DrawableMatchTeam(Pairing.Team2, Pairing, Pairing.Losers)
            };

            SchedulerAfterChildren.Add(() => Scheduler.Add(updateProgression));
            updateWinConditions();
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => args.Button == MouseButton.Left && editorInfo.EditingEnabled;

        protected override bool OnDragStart(InputState state) => editorInfo.EditingEnabled;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (Selected && editorInfo.EditingEnabled && args.Key == Key.Delete)
            {
                Remove();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override bool OnClick(InputState state)
        {
            if (!editorInfo.EditingEnabled)
                return false;

            Selected = true;
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            if (base.OnDrag(state)) return true;

            Selected = true;
            this.MoveToOffset(state.Mouse.Delta);

            var pos = Position;
            Pairing.Position = new Point((int)pos.X, (int)pos.Y);
            return true;
        }

        public void Remove()
        {
            Selected = false;
            Pairing.Progression.Value = null;
            Pairing.LosersProgression.Value = null;

            Expire();
        }
    }
}
