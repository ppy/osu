// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Online.Leaderboards;
using osu.Game.Online.Multiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.Ranking.Pages
{
    public class RoomLeaderboardPage : ResultsPage
    {
        private readonly Room room;

        private OsuColour colours;

        private TextFlowContainer rankText;

        public RoomLeaderboardPage(ScoreInfo score, WorkingBeatmap beatmap, Room room)
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
                    Colour = colours.Gray6,
                    RelativeSizeAxes = Axes.Both,
                },
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    BackgroundColour = colours.Gray6,
                    Child = leaderboard = CreateLeaderboard(room)
                },
                rankText = new TextFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.5f,
                    AutoSizeAxes = Axes.Y,
                    Y = 50,
                    TextAnchor = Anchor.TopCentre
                },
            };

            leaderboard.Origin = Anchor.Centre;
            leaderboard.Anchor = Anchor.Centre;
            leaderboard.RelativeSizeAxes = Axes.Both;
            leaderboard.Height = 0.8f;
            leaderboard.Y = 55;
            leaderboard.ScoresLoaded = scoresLoaded;
        }

        private void scoresLoaded(IEnumerable<APIRoomScoreInfo> scores)
        {
            Action<SpriteText> gray = s => s.Colour = colours.GrayC;
            Action<SpriteText> white = s =>
            {
                s.TextSize *= 1.4f;
                s.Colour = colours.GrayF;
            };

            rankText.AddText(room.Name + "\n", white);
            rankText.AddText("You are placed ", gray);

            int index = scores.IndexOf(new APIRoomScoreInfo { User = Score.User }, new FuncEqualityComparer<APIRoomScoreInfo>((s1, s2) => s1.User.Id.Equals(s2.User.Id)));

            rankText.AddText($"#{index + 1} ", s =>
            {
                s.Font = "Exo2.0-Bold";
                s.Colour = colours.YellowDark;
            });

            rankText.AddText("in the room!", gray);
        }

        protected virtual MatchLeaderboard CreateLeaderboard(Room room) => new ResultsMatchLeaderboard(room);

        public class ResultsMatchLeaderboard : MatchLeaderboard
        {
            public ResultsMatchLeaderboard(Room room)
            {
                Room = room;
            }

            protected override bool FadeTop => true;

            protected override LeaderboardScore CreateDrawableScore(APIRoomScoreInfo model, int index)
                => new ResultsMatchLeaderboardScore(model, index);

            protected override FillFlowContainer<LeaderboardScore> CreateScoreFlow()
            {
                var flow = base.CreateScoreFlow();
                flow.Padding = new MarginPadding
                {
                    Top = LeaderboardScore.HEIGHT * 2,
                    Bottom = LeaderboardScore.HEIGHT * 3,
                };
                return flow;
            }

            private class ResultsMatchLeaderboardScore : MatchLeaderboardScore
            {
                public ResultsMatchLeaderboardScore(APIRoomScoreInfo score, int rank)
                    : base(score, rank)
                {
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                }
            }
        }
    }
}
