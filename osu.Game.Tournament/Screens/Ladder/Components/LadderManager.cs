using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Input.States;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Components;
using OpenTK;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderManager : CompositeDrawable
    {
        public readonly List<TournamentTeam> Teams;
        private readonly Container<DrawableMatchPairing> pairingsContainer;
        private readonly Container<Path> paths;

        public LadderManager(LadderInfo info, List<TournamentTeam> teams)
        {
            Teams = teams;

            RelativeSizeAxes = Axes.Both;

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    pairingsContainer = new Container<DrawableMatchPairing> { RelativeSizeAxes = Axes.Both },
                    paths = new Container<Path> { RelativeSizeAxes = Axes.Both },
                }
            };

            foreach (var pair in info.Progressions)
                info.Pairings.Single(p => p.ID == pair.Item1).Progression.Value = info.Pairings.Single(p => p.ID == pair.Item2);

            foreach (var pairing in info.Pairings)
                addPairing(pairing);
        }

        public LadderInfo CreateInfo()
        {
            var pairings = pairingsContainer.Select(p => p.Pairing).ToList();

            return new LadderInfo
            {
                Pairings = pairings,
                Progressions = pairings
                               .Where(p => p.Progression.Value != null)
                               .Select(p => (p.ID, p.Progression.Value.ID))
                               .ToList()
            };
        }

        private void addPairing(MatchPairing pairing) => pairingsContainer.Add(new DrawableMatchPairing(pairing));

        protected override bool OnClick(InputState state)
        {
            addPairing(new MatchPairing
            {
                Position = new Point((int)state.Mouse.Position.X, (int)state.Mouse.Position.Y)
            });

            return true;
        }

        protected override void Update()
        {
            base.Update();

            paths.Clear();

            int id = 0;
            foreach (var pairing in pairingsContainer.OrderBy(d => d.Y).ThenBy(d => d.X))
            {
                pairing.Pairing.ID = id++;

                if (pairing.Pairing.Progression.Value != null)
                {
                    var progression = pairingsContainer.Single(p => p.Pairing == pairing.Pairing.Progression.Value);

                    const float line_width = 2;

                    var path = new Path
                    {
                        BypassAutoSizeAxes = Axes.Both,
                        PathWidth = line_width,
                    };

                    paths.Add(path);

                    Vector2 getCenteredVector(Vector2 top, Vector2 bottom) => new Vector2(top.X, top.Y + (bottom.Y - top.Y) / 2);

                    const float padding = 5;

                    var start = getCenteredVector(pairing.ScreenSpaceDrawQuad.TopRight, pairing.ScreenSpaceDrawQuad.BottomRight);
                    var end = getCenteredVector(progression.ScreenSpaceDrawQuad.TopLeft, progression.ScreenSpaceDrawQuad.BottomLeft);

                    bool progressionAbove = progression.ScreenSpaceDrawQuad.TopLeft.Y < pairing.ScreenSpaceDrawQuad.TopLeft.Y;
                    bool progressionToRight = progression.ScreenSpaceDrawQuad.TopLeft.X > pairing.ScreenSpaceDrawQuad.TopLeft.X;

                    //if (!Precision.AlmostEquals(progressionStart, start) || !Precision.AlmostEquals(progressionEnd, end))
                    {
                        // var progressionStart = start;
                        // var progressionEnd = end;

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
                }
            }
        }

        public void JoinRequest(MatchPairing pairing)
        {
            AddInternal(new JoinRequestHandler(pairing, handleProgression));
        }

        private bool handleProgression(JoinRequestHandler handler, InputState state)
        {
            var found = pairingsContainer.FirstOrDefault(d => d.ReceiveMouseInputAt(state.Mouse.NativeState.Position));

            if (found != null)
            {
                handler.Source.Progression.Value = found.Pairing;
                return true;
            }

            return false;
        }

        private class JoinRequestHandler : CompositeDrawable
        {
            public readonly MatchPairing Source;
            private readonly Func<JoinRequestHandler, InputState, bool> onClick;

            public JoinRequestHandler(MatchPairing source, Func<JoinRequestHandler, InputState, bool> onClick)
            {
                Source = source;
                this.onClick = onClick;
                RelativeSizeAxes = Axes.Both;
            }

            protected override bool OnClick(InputState state)
            {
                if (onClick(this, state))
                    Expire();

                return true;
            }
        }
    }
}
