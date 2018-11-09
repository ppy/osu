// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK;
using OpenTK.Graphics;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder
{
    [Cached]
    public class LadderManager : CompositeDrawable, IHasContextMenu
    {
        private Container<DrawableMatchPairing> pairingsContainer;
        private Container<Path> paths;
        private Container headings;

        private ScrollableContainer scrollContent;

        [Cached]
        private LadderEditorInfo editorInfo = new LadderEditorInfo();

        [Resolved]
        private LadderInfo ladderInfo { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            normalPathColour = colours.BlueDarker.Darken(2);
            losersPathColour = colours.YellowDarker.Darken(2);

            RelativeSizeAxes = Axes.Both;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    scrollContent = new ScrollableContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            paths = new Container<Path> { RelativeSizeAxes = Axes.Both },
                            headings = new Container { RelativeSizeAxes = Axes.Both },
                            pairingsContainer = new Container<DrawableMatchPairing> { RelativeSizeAxes = Axes.Both },
                        }
                    },
                    new LadderEditorSettings
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Margin = new MarginPadding(5)
                    }
                }
            };

            foreach (var pairing in ladderInfo.Pairings)
                addPairing(pairing);

            // todo: fix this
            Scheduler.AddDelayed(() => layout.Invalidate(), 100, true);
        }

        private void updateInfo()
        {
            ladderInfo.Pairings = pairingsContainer.Select(p => p.Pairing).ToList();
            foreach (var g in ladderInfo.Groupings)
                g.Pairings = ladderInfo.Pairings.Where(p => p.Grouping.Value == g).Select(p => p.ID).ToList();

            ladderInfo.Progressions = ladderInfo.Pairings.Where(p => p.Progression.Value != null).Select(p => new TournamentProgression(p.ID, p.Progression.Value.ID)).Concat(
                                        ladderInfo.Pairings.Where(p => p.LosersProgression.Value != null).Select(p => new TournamentProgression(p.ID, p.LosersProgression.Value.ID, true)))
                                       .ToList();
        }

        private void addPairing(MatchPairing pairing)
        {
            pairingsContainer.Add(new DrawableMatchPairing(pairing));
            updateInfo();
        }

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
                        var pos = pairingsContainer.ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position);
                        addPairing(new MatchPairing { Position = new Point((int)pos.X, (int)pos.Y) });
                    }),
                };
            }
        }

        private Cached layout = new Cached();

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
                updateLayout();
        }

        private Color4 normalPathColour;
        private Color4 losersPathColour;

        private void updateLayout()
        {
            paths.Clear();
            headings.Clear();

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
                        paths.Add(new ProgressionPath(pairing, dest) { Colour = pairing.Pairing.Losers ? losersPathColour : normalPathColour });
                }
            }

            foreach (var group in ladderInfo.Groupings)
            {
                var topPairing = pairingsContainer.Where(p => !p.Pairing.Losers && p.Pairing.Grouping.Value == group).OrderBy(p => p.Y).FirstOrDefault();

                if (topPairing == null) continue;

                headings.Add(new DrawableTournamentGrouping(group)
                {
                    Position = headings.ToLocalSpace((topPairing.ScreenSpaceDrawQuad.TopLeft + topPairing.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            foreach (var group in ladderInfo.Groupings)
            {
                var topPairing = pairingsContainer.Where(p => p.Pairing.Losers && p.Pairing.Grouping.Value == group).OrderBy(p => p.Y).FirstOrDefault();

                if (topPairing == null) continue;

                headings.Add(new DrawableTournamentGrouping(group, true)
                {
                    Position = headings.ToLocalSpace((topPairing.ScreenSpaceDrawQuad.TopLeft + topPairing.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            layout.Validate();
            updateInfo();
        }

        public void RequestJoin(MatchPairing pairing, bool losers) => scrollContent.Add(new JoinRequestHandler(pairingsContainer, pairing, losers, updateInfo));

        // todo: remove after ppy/osu-framework#1980 is merged.
        public override bool HandlePositionalInput => true;

        public void Remove(MatchPairing pairing) => pairingsContainer.FirstOrDefault(p => p.Pairing == pairing)?.Remove();

        public void SetCurrent(MatchPairing pairing)
        {
            if (ladderInfo.CurrentMatch.Value != null)
                ladderInfo.CurrentMatch.Value.Current.Value = false;

            ladderInfo.CurrentMatch.Value = pairing;
            ladderInfo.CurrentMatch.Value.Current.Value = true;
        }

        private class JoinRequestHandler : CompositeDrawable
        {
            private readonly Container<DrawableMatchPairing> pairingsContainer;
            public readonly MatchPairing Source;
            private readonly bool losers;
            private readonly Action complete;

            private ProgressionPath path;

            public JoinRequestHandler(Container<DrawableMatchPairing> pairingsContainer, MatchPairing source, bool losers, Action complete)
            {
                this.pairingsContainer = pairingsContainer;
                RelativeSizeAxes = Axes.Both;

                Source = source;
                this.losers = losers;
                this.complete = complete;
                if (losers)
                    Source.LosersProgression.Value = null;
                else
                    Source.Progression.Value = null;
            }

            private DrawableMatchPairing findTarget(InputState state) => pairingsContainer.FirstOrDefault(d => d.ReceivePositionalInputAt(state.Mouse.Position));

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var found = findTarget(e.CurrentState);

                if (found == path?.Destination)
                    return false;

                path?.Expire();
                path = null;

                if (found == null)
                    return false;

                AddInternal(path = new ProgressionPath(pairingsContainer.First(c => c.Pairing == Source), found)
                {
                    Colour = Color4.Yellow,
                });

                return base.OnMouseMove(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                var found = findTarget(e.CurrentState);

                if (found != null)
                {
                    if (found.Pairing != Source)
                    {
                        if (losers)
                            Source.LosersProgression.Value = found.Pairing;
                        else
                            Source.Progression.Value = found.Pairing;
                    }

                    complete?.Invoke();
                    Expire();
                    return true;
                }

                return false;
            }
        }
    }
}
