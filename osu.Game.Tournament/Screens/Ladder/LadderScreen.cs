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
        protected Container<DrawableTournamentMatch> MatchesContainer;
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
                            MatchesContainer = new Container<DrawableTournamentMatch> { RelativeSizeAxes = Axes.Both },
                        }
                    },
                }
            };

            void addMatch(TournamentMatch match) =>
                MatchesContainer.Add(new DrawableTournamentMatch(match, this is LadderEditorScreen)
                {
                    Changed = () => layout.Invalidate()
                });

            foreach (var match in LadderInfo.Matches)
                addMatch(match);

            LadderInfo.Rounds.ItemsAdded += _ => layout.Invalidate();
            LadderInfo.Rounds.ItemsRemoved += _ => layout.Invalidate();

            LadderInfo.Matches.ItemsAdded += matches =>
            {
                foreach (var p in matches)
                    addMatch(p);
                layout.Invalidate();
            };

            LadderInfo.Matches.ItemsRemoved += matches =>
            {
                foreach (var p in matches)
                foreach (var d in MatchesContainer.Where(d => d.Match == p))
                    d.Expire();

                layout.Invalidate();
            };
        }

        private readonly Cached layout = new Cached();

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

            foreach (var round in LadderInfo.Rounds)
            {
                var topMatch = MatchesContainer.Where(p => !p.Match.Losers.Value && p.Match.Round.Value == round).OrderBy(p => p.Y).FirstOrDefault();

                if (topMatch == null) continue;

                headings.Add(new DrawableTournamentRound(round)
                {
                    Position = headings.ToLocalSpace((topMatch.ScreenSpaceDrawQuad.TopLeft + topMatch.ScreenSpaceDrawQuad.TopRight) / 2),
                    Margin = new MarginPadding { Bottom = 10 },
                    Origin = Anchor.BottomCentre,
                });
            }

            foreach (var round in LadderInfo.Rounds)
            {
                var topMatch = MatchesContainer.Where(p => p.Match.Losers.Value && p.Match.Round.Value == round).OrderBy(p => p.Y).FirstOrDefault();

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
    }
}
