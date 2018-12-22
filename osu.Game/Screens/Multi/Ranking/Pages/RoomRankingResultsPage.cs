// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Ranking.Pages
{
    public class RoomRankingResultsPage : ResultsPage
    {
        private readonly Room room;

        private OsuColour colours;

        private TextFlowContainer rankText;

        public RoomRankingResultsPage(ScoreInfo score, WorkingBeatmap beatmap, Room room)
            : base(score, beatmap)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            MatchLeaderboard leaderboard;

            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.GrayE,
                    RelativeSizeAxes = Axes.Both,
                },
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundColour = colours.GrayE,
                    Child = leaderboard = CreateLeaderboard(room)
                },
                rankText = new TextFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.5f,
                    AutoSizeAxes = Axes.Y,
                    Y = 75,
                    TextAnchor = Anchor.TopCentre
                },
            };

            leaderboard.Origin = Anchor.Centre;
            leaderboard.Anchor = Anchor.Centre;
            leaderboard.RelativeSizeAxes = Axes.Both;
            leaderboard.Height = 0.8f;
            leaderboard.Y = 95;
            leaderboard.ScoresLoaded = scoresLoaded;
        }

        private void scoresLoaded(IEnumerable<APIRoomScoreInfo> scores)
        {
            Action<SpriteText> gray = s => s.Colour = colours.Gray8;

            rankText.AddText("You are placed ", gray);

            int index = scores.IndexOf(new APIRoomScoreInfo { User = Score.User }, new FuncEqualityComparer<APIRoomScoreInfo>((s1, s2) => s1.User.Id.Equals(s2.User.Id)));

            rankText.AddText($"#{index + 1} ", s =>
            {
                s.Font = "Exo2.0-Bold";
                s.Colour = colours.YellowDark;
            });

            rankText.AddText("in the room!", gray);
        }

        protected virtual MatchLeaderboard CreateLeaderboard(Room room) => new MatchLeaderboard(room);
    }
}
