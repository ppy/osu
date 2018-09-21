using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.States;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder
{
    public class LadderEditorInfo
    {
        public readonly BindableBool EditingEnabled = new BindableBool();
        public List<TournamentTeam> Teams = new List<TournamentTeam>();
        public readonly Bindable<MatchPairing> Selected = new Bindable<MatchPairing>();
    }

    public class LadderManager : CompositeDrawable, IHasContextMenu
    {
        public readonly List<TournamentTeam> Teams;
        private readonly Container<DrawableMatchPairing> pairingsContainer;
        private readonly Container<Path> paths;

        [Cached]
        private readonly LadderEditorInfo editorInfo = new LadderEditorInfo();

        public LadderManager(LadderInfo info, List<TournamentTeam> teams)
        {
            editorInfo.Teams = Teams = teams;

            RelativeSizeAxes = Axes.Both;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    paths = new Container<Path> { RelativeSizeAxes = Axes.Both },
                    pairingsContainer = new Container<DrawableMatchPairing> { RelativeSizeAxes = Axes.Both },
                    new LadderEditorSettings
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Margin = new MarginPadding(5)
                    }
                }
            };

            foreach (var pair in info.Progressions)
            {
                var src = info.Pairings.FirstOrDefault(p => p.ID == pair.Item1);
                var dest = info.Pairings.FirstOrDefault(p => p.ID == pair.Item2);

                if (src == null) throw new InvalidOperationException();

                if (dest != null)
                    src.Progression.Value = dest;
            }

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

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!editorInfo.EditingEnabled)
                    return new MenuItem[0];

                return new MenuItem[]
                {
                    new OsuMenuItem("Create new match", MenuItemType.Highlighted, () =>
                    {
                        var pos = ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position);
                        addPairing(new MatchPairing { Position = new Point((int)pos.X, (int)pos.Y) });
                    }),
                };
            }
        }

        protected override void Update()
        {
            base.Update();

            paths.Clear();

            int id = 1;
            foreach (var pairing in pairingsContainer.OrderBy(d => d.Y).ThenBy(d => d.X))
            {
                pairing.Pairing.ID = id++;

                if (pairing.Pairing.Progression.Value != null)
                {
                    var dest = pairingsContainer.FirstOrDefault(p => p.Pairing == pairing.Pairing.Progression.Value);

                    if (dest == null)
                        // clean up outdated progressions.
                        pairing.Pairing.Progression.Value = null;
                    else
                        paths.Add(new ProgressionPath(pairing, dest));
                }
            }
        }

        public void RequestJoin(MatchPairing pairing) => AddInternal(new JoinRequestHandler(pairingsContainer, pairing));

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
                    if (found.Pairing != Source)
                        Source.Progression.Value = found.Pairing;
                    Expire();
                    return true;
                }

                return false;
            }
        }

        public void Remove(MatchPairing pairing) => pairingsContainer.FirstOrDefault(p => p.Pairing == pairing)?.Remove();
    }
}
