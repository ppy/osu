// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Ladder
{
    public class LadderScreen : TournamentScreen, IProvideVideo
    {
        protected Container<DrawableMatchPairing> PairingsContainer;
        private Container<Path> paths;
        private Container headings;

        protected LadderDragContainer ScrollContent;

        protected Container Content;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, Storage storage)
        {
            normalPathColour = colours.BlueDarker.Darken(2);
            losersPathColour = colours.YellowDarker.Darken(2);

            RelativeSizeAxes = Axes.Both;

            InternalChild = Content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new TourneyVideo(storage.GetStream(@"BG Side Logo - OWC.m4v"))
                    {
                        RelativeSizeAxes = Axes.Both,
                        Loop = true,
                    },
                    ScrollContent = new LadderDragContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            paths = new Container<Path> { RelativeSizeAxes = Axes.Both },
                            headings = new Container { RelativeSizeAxes = Axes.Both },
                            PairingsContainer = new Container<DrawableMatchPairing> { RelativeSizeAxes = Axes.Both },
                        }
                    },
                }
            };

            void addPairing(MatchPairing pairing) =>
                PairingsContainer.Add(new DrawableMatchPairing(pairing, this is LadderEditorScreen)
                {
                    Changed = () => layout.Invalidate()
                });

            foreach (var pairing in LadderInfo.Pairings)
                addPairing(pairing);

            LadderInfo.Rounds.ItemsAdded += _ => layout.Invalidate();
            LadderInfo.Rounds.ItemsRemoved += _ => layout.Invalidate();

            LadderInfo.Pairings.ItemsAdded += pairings =>
            {
                foreach (var p in pairings)
                    addPairing(p);
                layout.Invalidate();
            };

            LadderInfo.Pairings.ItemsRemoved += pairings =>
            {
                foreach (var p in pairings)
                foreach (var d in PairingsContainer.Where(d => d.Pairing == p))
                    d.Expire();

                layout.Invalidate();
            };
        }

        private Cached layout = new Cached();

        protected override void Update()
        {
            base.Update();

            if (!layout.IsValid)
                UpdateLayout();
        }

        private Color4 normalPathColour;
        private Color4 losersPathColour;

        protected virtual bool DrawLoserPaths => false;

        protected virtual void UpdateLayout()
        {
            paths.Clear();
            headings.Clear();

            int id = 1;

            foreach (var pairing in PairingsContainer.OrderBy(d => d.Y).ThenBy(d => d.X))
            {
                pairing.Pairing.ID = id++;

                if (pairing.Pairing.Progression.Value != null)
                {
                    var dest = PairingsContainer.FirstOrDefault(p => p.Pairing == pairing.Pairing.Progression.Value);

                    if (dest == null)
                        // clean up outdated progressions.
                        pairing.Pairing.Progression.Value = null;
                    else
                        paths.Add(new ProgressionPath(pairing, dest) { Colour = pairing.Pairing.Losers.Value ? losersPathColour : normalPathColour });
                }

                if (DrawLoserPaths)
                {
                    if (pairing.Pairing.LosersProgression.Value != null)
                    {
                        var dest = PairingsContainer.FirstOrDefault(p => p.Pairing == pairing.Pairing.LosersProgression.Value);

                        if (dest == null)
                            // clean up outdated progressions.
                            pairing.Pairing.LosersProgression.Value = null;
                        else
                            paths.Add(new ProgressionPath(pairing, dest) { Colour = losersPathColour.Opacity(0.1f) });
                    }
                }
            }

            foreach (var round in LadderInfo.Rounds)
            {
                var topPairing = PairingsContainer.Where(p => !p.Pairing.Losers.Value && p.Pairing.Round.Value == round).OrderBy(p => p.Y).FirstOrDefault();

                if (topPairing == null) continue;

                headings.Add(new DrawableTournamentRound(round)
                {
                    Position = headings.ToLocalSpace((topPairing.ScreenSpaceDrawQuad.TopLeft + topPairing.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            foreach (var round in LadderInfo.Rounds)
            {
                var topPairing = PairingsContainer.Where(p => p.Pairing.Losers.Value && p.Pairing.Round.Value == round).OrderBy(p => p.Y).FirstOrDefault();

                if (topPairing == null) continue;

                headings.Add(new DrawableTournamentRound(round, true)
                {
                    Position = headings.ToLocalSpace((topPairing.ScreenSpaceDrawQuad.TopLeft + topPairing.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            layout.Validate();
        }
    }
}
