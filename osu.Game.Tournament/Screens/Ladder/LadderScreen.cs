// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Video;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Ladder
{
    public class LadderScreen : TournamentScreen, IProvideVideo
    {
        protected Container<DrawableMatchPairing> PairingsContainer;
        private Container<Path> paths;
        private Container headings;

        protected ScrollableContainer ScrollContent;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, Storage storage)
        {
            normalPathColour = colours.BlueDarker.Darken(2);
            losersPathColour = colours.YellowDarker.Darken(2);

            RelativeSizeAxes = Axes.Both;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new VideoSprite(storage.GetStream(@"BG Side Logo - OWC.m4v"))
                    {
                        RelativeSizeAxes = Axes.Both,
                        Loop = true,
                    },
                    ScrollContent = new ScrollableContainer
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

            foreach (var pairing in LadderInfo.Pairings)
                AddPairing(pairing);

            // todo: fix this
            Scheduler.AddDelayed(() => layout.Invalidate(), 100, true);
        }

        protected virtual void AddPairing(MatchPairing pairing)
        {
            PairingsContainer.Add(new DrawableMatchPairing(pairing));
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
                        paths.Add(new ProgressionPath(pairing, dest) { Colour = pairing.Pairing.Losers ? losersPathColour : normalPathColour });
                }
            }

            foreach (var group in LadderInfo.Groupings)
            {
                var topPairing = PairingsContainer.Where(p => !p.Pairing.Losers && p.Pairing.Grouping.Value == group).OrderBy(p => p.Y).FirstOrDefault();

                if (topPairing == null) continue;

                headings.Add(new DrawableTournamentGrouping(group)
                {
                    Position = headings.ToLocalSpace((topPairing.ScreenSpaceDrawQuad.TopLeft + topPairing.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            foreach (var group in LadderInfo.Groupings)
            {
                var topPairing = PairingsContainer.Where(p => p.Pairing.Losers && p.Pairing.Grouping.Value == group).OrderBy(p => p.Y).FirstOrDefault();

                if (topPairing == null) continue;

                headings.Add(new DrawableTournamentGrouping(group, true)
                {
                    Position = headings.ToLocalSpace((topPairing.ScreenSpaceDrawQuad.TopLeft + topPairing.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            layout.Validate();
        }

        // todo: remove after ppy/osu-framework#1980 is merged.
        public override bool HandlePositionalInput => true;
    }
}
