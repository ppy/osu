// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results.Components;
using osuTK;

using System;
using System.Drawing;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Overlays;
using osuTK.Graphics;
using osu.Game.Screens.OnlinePlay.Tournaments.Components;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results
{
    [Cached]
    public partial class BracketScreen : CompositeDrawable, IHasContextMenu
    {
        protected Container<DrawableTournamentMatch> MatchesContainer = null!;
        private Container<Path> paths = null!;
        private Container headings = null!;

        protected BracketDragContainer ScrollContent = null!;

        protected Container Content = null!;

        [Resolved]
        private TournamentInfo tournamentInfo { get; set; } = null!;

        public readonly Bindable<TournamentMatch> Selected = new Bindable<TournamentMatch>();

        public const float GRID_SPACING = 10;

        private RectangularPositionSnapGrid grid = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        protected bool DrawLoserPaths => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            normalPathColour = Color4Extensions.FromHex("#66D1FF");
            losersPathColour = Color4Extensions.FromHex("#FFC700");

            RelativeSizeAxes = Axes.Both;

            InternalChild = Content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    new DrawableTournamentHeaderText
                    {
                        Y = 100,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    },
                    ScrollContent = new BracketDragContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            paths = new Container<Path> { RelativeSizeAxes = Axes.Both },
                            headings = new Container { RelativeSizeAxes = Axes.Both },
                            MatchesContainer = new Container<DrawableTournamentMatch>
                            {
                                AutoSizeAxes = Axes.Both
                            },
                        }
                    },
                }
            };

            void addMatch(TournamentMatch match) =>
                // MatchesContainer.Add(new DrawableTournamentMatch(match, this is LadderEditorScreen)
                MatchesContainer.Add(new DrawableTournamentMatch(match, true)
                {
                    Changed = () => layout.Invalidate()
                });

            foreach (var match in tournamentInfo.Matches)
                addMatch(match);

            tournamentInfo.Rounds.CollectionChanged += (_, _) => layout.Invalidate();
            tournamentInfo.Matches.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(args.NewItems != null);

                        foreach (var p in args.NewItems.Cast<TournamentMatch>())
                            addMatch(p);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(args.OldItems != null);

                        foreach (var p in args.OldItems.Cast<TournamentMatch>())
                        {
                            foreach (var d in MatchesContainer.Where(d => d.Match == p))
                                d.Expire();
                        }

                        break;
                }

                layout.Invalidate();
            };

            // AddInternal(new ControlPanel
            // {
            //     Child = new LadderEditorSettings(),
            // });

            ScrollContent.Add(grid = new RectangularPositionSnapGrid
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                BypassAutoSizeAxes = Axes.Both,
                Depth = float.MaxValue
            });

            grid.Spacing.Value = new Vector2(GRID_SPACING);
        }

        private readonly Cached layout = new Cached();

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
                UpdateLayout();

            // Expand grid with the content to allow going beyond the bounds of the screen.
            grid.Size = ScrollContent.Size + new Vector2(GRID_SPACING * 2);
        }

        private Color4 normalPathColour;
        private Color4 losersPathColour;

        protected virtual void UpdateLayout()
        {
            paths.Clear();
            headings.Clear();

            int id = 1;

            foreach (var match in MatchesContainer.OrderBy(d => d.Y).ThenBy(d => d.X))
            {
                match.Match.ID = id++;

                if (match.Match.Progression.Value != null)
                {
                    var dest = MatchesContainer.FirstOrDefault(p => p.Match == match.Match.Progression.Value);

                    if (dest == null)
                        // clean up outdated progressions.
                        match.Match.Progression.Value = null;
                    else
                        paths.Add(new ProgressionPath(match, dest) { Colour = match.Match.Losers.Value ? losersPathColour : normalPathColour });
                }

                if (DrawLoserPaths)
                {
                    if (match.Match.LosersProgression.Value != null)
                    {
                        var dest = MatchesContainer.FirstOrDefault(p => p.Match == match.Match.LosersProgression.Value);

                        if (dest == null)
                            // clean up outdated progressions.
                            match.Match.LosersProgression.Value = null;
                        else
                            paths.Add(new ProgressionPath(match, dest) { Colour = losersPathColour.Opacity(0.1f) });
                    }
                }
            }

            foreach (var round in tournamentInfo.Rounds)
            {
                var topMatch = MatchesContainer.Where(p => !p.Match.Losers.Value && p.Match.Round.Value == round).MinBy(p => p.Y);

                if (topMatch == null) continue;

                headings.Add(new DrawableTournamentRound(round)
                {
                    Position = headings.ToLocalSpace((topMatch.ScreenSpaceDrawQuad.TopLeft + topMatch.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            foreach (var round in tournamentInfo.Rounds)
            {
                var topMatch = MatchesContainer.Where(p => p.Match.Losers.Value && p.Match.Round.Value == round).MinBy(p => p.Y);

                if (topMatch == null) continue;

                headings.Add(new DrawableTournamentRound(round, true)
                {
                    Position = headings.ToLocalSpace((topMatch.ScreenSpaceDrawQuad.TopLeft + topMatch.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            layout.Validate();
        }

        private Vector2 lastMatchesContainerMouseDownPosition;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            lastMatchesContainerMouseDownPosition = MatchesContainer.ToLocalSpace(e.ScreenSpaceMouseDownPosition);
            return base.OnMouseDown(e);
        }

        public void BeginJoin(TournamentMatch match, bool losers)
        {
            ScrollContent.Add(new JoinVisualiser(MatchesContainer, match, losers, UpdateLayout));
        }

        public MenuItem[] ContextMenuItems => !tournamentInfo.IsEditing.Value ? Array.Empty<MenuItem>() : new MenuItem[]
                {
                    new OsuMenuItem("Create new match", MenuItemType.Highlighted, () =>
                    {
                        Vector2 pos = MatchesContainer.Count == 0 ? Vector2.Zero : lastMatchesContainerMouseDownPosition;

                        TournamentMatch newMatch = new TournamentMatch { Position = { Value = new Point((int)pos.X, (int)pos.Y) } };

                        tournamentInfo.Matches.Add(newMatch);

                        Selected.Value = newMatch;
                    }),
                    new OsuMenuItem("Reset teams", MenuItemType.Destructive, () =>
                    {
                        dialogOverlay?.Push(new BracketResetTeamsDialog(() =>
                        {
                            foreach (var p in MatchesContainer)
                                p.Match.Reset();
                        }));
                    })
                };

        public void Remove(TournamentMatch match)
        {
            MatchesContainer.FirstOrDefault(p => p.Match == match)?.Remove();
        }

        private partial class JoinVisualiser : CompositeDrawable
        {
            private readonly Container<DrawableTournamentMatch> matchesContainer;
            public readonly TournamentMatch Source;
            private readonly bool losers;
            private readonly Action? complete;

            private ProgressionPath? path;

            public JoinVisualiser(Container<DrawableTournamentMatch> matchesContainer, TournamentMatch source, bool losers, Action? complete)
            {
                this.matchesContainer = matchesContainer;
                RelativeSizeAxes = Axes.Both;

                Source = source;
                this.losers = losers;
                this.complete = complete;
                if (losers)
                    Source.LosersProgression.Value = null;
                else
                    Source.Progression.Value = null;
            }

            private DrawableTournamentMatch? findTarget(InputState state)
            {
                return matchesContainer.FirstOrDefault(d => d.ReceivePositionalInputAt(state.Mouse.Position));
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                return true;
            }

            protected override bool OnMouseMove(MouseMoveEvent e)
            {
                var found = findTarget(e.CurrentState);

                if (found == path?.Destination)
                    return false;

                path?.Expire();
                path = null;

                if (found == null)
                    return false;

                AddInternal(path = new ProgressionPath(matchesContainer.First(c => c.Match == Source), found)
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
                    if (found.Match != Source)
                    {
                        if (losers)
                            Source.LosersProgression.Value = found.Match;
                        else
                            Source.Progression.Value = found.Match;
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
