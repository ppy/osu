using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Input.States;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder
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
                    paths.Add(new ProgressionPath(pairing, pairingsContainer.Single(p => p.Pairing == pairing.Pairing.Progression.Value)));
            }
        }

        public void JoinRequest(MatchPairing pairing) => AddInternal(new JoinRequestHandler(pairingsContainer, pairing));

        private class JoinRequestHandler : CompositeDrawable
        {
            private readonly Container<DrawableMatchPairing> pairingsContainer;
            public readonly MatchPairing Source;

            private ProgressionPath path;

            public JoinRequestHandler(Container<DrawableMatchPairing> pairingsContainer, MatchPairing source)
            {
                this.pairingsContainer = pairingsContainer;
                RelativeSizeAxes = Axes.Both;

                Source = source;
                Source.Progression.Value = null;
            }

            private DrawableMatchPairing findTarget(InputState state) => pairingsContainer.FirstOrDefault(d => d.ReceiveMouseInputAt(state.Mouse.NativeState.Position));

            protected override bool OnMouseMove(InputState state)
            {
                var found = findTarget(state);

                if (found == path?.Destination)
                    return false;

                path?.Expire();
                path = null;

                if (found == null)
                    return false;

                AddInternal(path = new ProgressionPath(pairingsContainer.First(c => c.Pairing == Source), found) { Alpha = 0.4f });

                return base.OnMouseMove(state);
            }

            protected override bool OnClick(InputState state)
            {
                var found = findTarget(state);

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
