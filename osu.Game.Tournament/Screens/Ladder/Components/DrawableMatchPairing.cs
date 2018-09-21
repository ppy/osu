// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using OpenTK;
using OpenTK.Input;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchPairing : CompositeDrawable
    {
        public readonly MatchPairing Pairing;
        private readonly FillFlowContainer<DrawableMatchTeam> flow;
        private readonly Bindable<TournamentConditions> conditions = new Bindable<TournamentConditions>();

        public DrawableMatchPairing(MatchPairing pairing)
        {
            Pairing = pairing;

            Position = new Vector2(pairing.Position.X, pairing.Position.Y);

            AutoSizeAxes = Axes.Both;

            Margin = new MarginPadding(5);

            InternalChildren = new Drawable[]
            {
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

            pairing.Completed.BindValueChanged(_ => updateProgression());
            pairing.Progression.BindValueChanged(_ => updateProgression());

            updateTeams();
        }

        [BackgroundDependencyLoader(true)]
        private void load(Bindable<TournamentConditions> conditions)
        {
            this.conditions.BindValueChanged(_ => updateWinConditions());

            if (conditions != null)
                this.conditions.BindTo(conditions);
        }

        private void updateProgression()
        {
            var progression = Pairing.Progression?.Value;

            if (progression == null) return;

            bool progressionAbove = progression.ID < Pairing.ID;

            var destinationForWinner = progressionAbove ? progression.Team2 : progression.Team1;
            destinationForWinner.Value = Pairing.Winner;
        }

        private void updateWinConditions()
        {
            if (conditions.Value == null) return;

            Pairing.Completed.Value = Pairing.Team1Score.Value + Pairing.Team2Score.Value >= conditions.Value.BestOf;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateTeams();
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
                new DrawableMatchTeam(Pairing.Team1, Pairing),
                new DrawableMatchTeam(Pairing.Team2, Pairing)
            };

            SchedulerAfterChildren.Add(() => Scheduler.Add(updateProgression));
            updateWinConditions();
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => args.Button == MouseButton.Left;

        protected override bool OnDragStart(InputState state) => true;

        protected override bool OnDrag(InputState state)
        {
            if (base.OnDrag(state)) return true;

            this.MoveToOffset(state.Mouse.Delta);

            var pos = Position;
            Pairing.Position = new Point((int)pos.X, (int)pos.Y);
            return true;
        }

        public void Remove()
        {
            if (Pairing.ProgressionSource.Value != null)
                Pairing.ProgressionSource.Value.Progression.Value = null;

            Pairing.Progression.Value = null;
            Expire();
        }
    }
}
