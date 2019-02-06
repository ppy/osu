// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;
using osuTK.Graphics;
using SixLabors.Primitives;

namespace osu.Game.Tournament.Screens.Ladder
{
    [Cached]
    public class LadderEditorScreen : LadderScreen, IHasContextMenu
    {
        [Cached]
        private LadderEditorInfo editorInfo = new LadderEditorInfo();

        [BackgroundDependencyLoader]
        private void load()
        {
            ((Container)InternalChild).Add(new LadderEditorSettings
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Margin = new MarginPadding(5)
            });
        }

        private void updateInfo()
        {
            LadderInfo.Pairings = PairingsContainer.Select(p => p.Pairing).ToList();
            foreach (var g in LadderInfo.Groupings)
                g.Pairings = LadderInfo.Pairings.Where(p => p.Grouping.Value == g).Select(p => p.ID).ToList();

            LadderInfo.Progressions = LadderInfo.Pairings.Where(p => p.Progression.Value != null).Select(p => new TournamentProgression(p.ID, p.Progression.Value.ID)).Concat(
                                                    LadderInfo.Pairings.Where(p => p.LosersProgression.Value != null).Select(p => new TournamentProgression(p.ID, p.LosersProgression.Value.ID, true)))
                                                .ToList();
        }

        protected override void AddPairing(MatchPairing pairing)
        {
            base.AddPairing(pairing);
            updateInfo();
        }

        protected override void UpdateLayout()
        {
            base.UpdateLayout();
            updateInfo();
        }

        public void RequestJoin(MatchPairing pairing, bool losers)
        {
            ScrollContent.Add(new JoinRequestHandler(PairingsContainer, pairing, losers, updateInfo));
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (editorInfo == null)
                    return new MenuItem[0];

                return new MenuItem[]
                {
                    new OsuMenuItem("Create new match", MenuItemType.Highlighted, () =>
                    {
                        var pos = PairingsContainer.ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position);
                        AddPairing(new MatchPairing { Position = { Value = new Point((int)pos.X, (int)pos.Y) } });
                    }),
                    new OsuMenuItem("Reset teams", MenuItemType.Destructive, () =>
                    {
                        foreach (var p in PairingsContainer)
                            p.Pairing.Reset();
                    })
                };
            }
        }

        public void Remove(MatchPairing pairing)
        {
            PairingsContainer.FirstOrDefault(p => p.Pairing == pairing)?.Remove();
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

            private DrawableMatchPairing findTarget(InputState state)
            {
                return pairingsContainer.FirstOrDefault(d => d.ReceivePositionalInputAt(state.Mouse.Position));
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
