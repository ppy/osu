// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Framework.MathUtils;
using OpenTK;
using OpenTK.Input;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableMatchPairing : CompositeDrawable
    {
        public readonly MatchPairing Pairing;
        private readonly FillFlowContainer<DrawableMatchTeam> flow;
        private DrawableMatchPairing progression;

        private readonly Bindable<TournamentConditions> conditions = new Bindable<TournamentConditions>();

        private readonly Path path;

        public DrawableMatchPairing Progression
        {
            get => progression;
            set
            {
                if (progression == value) return;
                progression = value;

                if (LoadState == LoadState.Loaded)
                    updateProgression();

                path.FadeInFromZero(200);
            }
        }

        private Vector2 progressionStart;
        private Vector2 progressionEnd;

        private const float line_width = 2;

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
                },
                path = new Path
                {
                    Alpha = 0,
                    BypassAutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    PathWidth = line_width,
                },
            };

            pairing.Team1.BindValueChanged(_ => updateTeams());
            pairing.Team2.BindValueChanged(_ => updateTeams());

            pairing.Team1Score.BindValueChanged(_ => updateWinConditions());
            pairing.Team2Score.BindValueChanged(_ => updateWinConditions());

            pairing.Completed.BindValueChanged(_ => updateProgression());

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
            if (progression == null)
            {
                path.Positions = new List<Vector2>();
                return;
            }

            Vector2 getCenteredVector(Vector2 top, Vector2 bottom) => new Vector2(top.X, top.Y + (bottom.Y - top.Y) / 2);

            const float padding = 5;

            var start = getCenteredVector(ScreenSpaceDrawQuad.TopRight, ScreenSpaceDrawQuad.BottomRight);
            var end = getCenteredVector(progression.ScreenSpaceDrawQuad.TopLeft, progression.ScreenSpaceDrawQuad.BottomLeft);

            bool progressionAbove = progression.ScreenSpaceDrawQuad.TopLeft.Y < ScreenSpaceDrawQuad.TopLeft.Y;
            bool progressionToRight = progression.ScreenSpaceDrawQuad.TopLeft.X > ScreenSpaceDrawQuad.TopLeft.X;

            if (!Precision.AlmostEquals(progressionStart, start) || !Precision.AlmostEquals(progressionEnd, end))
            {
                progressionStart = start;
                progressionEnd = end;

                path.Origin = progressionAbove ? Anchor.y2 : Anchor.y0;
                path.Y = progressionAbove ? line_width : -line_width;

                path.Origin |= progressionToRight ? Anchor.x0 : Anchor.x2;
                //path.X = progressionToRight ? line_width : -line_width;

                Vector2 startPosition = path.ToLocalSpace(start) + new Vector2(padding, 0);
                Vector2 endPosition = path.ToLocalSpace(end) + new Vector2(-padding, 0);
                Vector2 intermediate1 = startPosition + new Vector2(padding, 0);
                Vector2 intermediate2 = new Vector2(intermediate1.X, endPosition.Y);

                path.Positions = new List<Vector2>
                {
                    startPosition,
                    intermediate1,
                    intermediate2,
                    endPosition
                };
            }

            var destinationForWinner = progressionAbove ? progression.Pairing.Team2 : progression.Pairing.Team1;

            destinationForWinner.Value = Pairing.Winner;
        }

        protected override void UpdateAfterAutoSize()
        {
            base.UpdateAfterAutoSize();
            updateProgression();
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
    }
}
