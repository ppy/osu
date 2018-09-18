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

                    const float padding = 10;

                    var q1 = pairing.ScreenSpaceDrawQuad;
                    var q2 = progression.ScreenSpaceDrawQuad;

                    bool progressionToRight = q2.TopLeft.X > q1.TopLeft.X;

                    if (!progressionToRight)
                    {
                        var temp = q2;
                        q2 = q1;
                        q1 = temp;
                    }

                    var c1 = getCenteredVector(q1.TopRight, q1.BottomRight) + new Vector2(padding, 0);
                    var c2 = getCenteredVector(q2.TopLeft, q2.BottomLeft) - new Vector2(padding, 0);

                    var p1 = c1;
                    var p2 = p1 + new Vector2(padding, 0);

                    if (p2.X > c2.X)
                    {
                        c2 = getCenteredVector(q2.TopRight, q2.BottomRight) + new Vector2(padding, 0);
                        p2.X = c2.X + padding;
                    }

                    var p3 = new Vector2(p2.X, c2.Y);
                    var p4 = new Vector2(c2.X, p3.Y);

                    path.Positions = new[] { p1, p2, p3, p4 }.Select(p => path.ToLocalSpace(p)).ToList();
                }
            }
        }

        public void JoinRequest(MatchPairing pairing)
        {
            AddInternal(new JoinRequestHandler(pairingsContainer, pairing));
        }

        private class JoinRequestHandler : CompositeDrawable
        {
            private readonly Container<DrawableMatchPairing> pairingsContainer;
            public readonly MatchPairing Source;

            public JoinRequestHandler(Container<DrawableMatchPairing> pairingsContainer, MatchPairing source)
            {
                this.pairingsContainer = pairingsContainer;
                Source = source;
                RelativeSizeAxes = Axes.Both;
            }

            protected override bool OnClick(InputState state)
            {
                var found = pairingsContainer.FirstOrDefault(d => d.ReceiveMouseInputAt(state.Mouse.NativeState.Position));

                if (found != null)
                {
                    Source.Progression.Value = found.Pairing;
                    Expire();
                    return true;
                }

                return false;
            }
        }
    }
}
