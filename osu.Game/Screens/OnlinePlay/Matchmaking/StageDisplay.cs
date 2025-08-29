// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class StageDisplay : CompositeDrawable
    {
        private OsuScrollContainer scroll = null!;
        private FillFlowContainer flow = null!;

        public StageDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // TODO: get this from somewhere?
            const int round_count = 5;

            InternalChildren = new Drawable[]
            {
                scroll = new OsuScrollContainer(Direction.Horizontal)
                {
                    ScrollbarOverlapsContent = false,
                    ScrollbarVisible = false,
                    RelativeSizeAxes = Axes.X,
                    Height = 36,
                    Child = flow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                    },
                },
                new StageText
                {
                    Margin = new MarginPadding { Top = 36 + 5, Bottom = 5 },
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                }
            };

            flow.Add(new StageBubble(null, MatchmakingStage.WaitingForClientsJoin, "Waiting for other users"));

            for (int i = 1; i <= round_count; i++)
            {
                flow.Add(new StageBubble(i, MatchmakingStage.RoundWarmupTime, "Next Round"));
                flow.Add(new StageBubble(i, MatchmakingStage.UserBeatmapSelect, "Beatmap Selection"));
                flow.Add(new StageBubble(i, MatchmakingStage.GameplayWarmupTime, "Get Ready"));
                flow.Add(new StageBubble(i, MatchmakingStage.ResultsDisplaying, "Results"));
            }

            flow.Add(new StageBubble(null, MatchmakingStage.Ended, "Match End"));
        }

        protected override void Update()
        {
            base.Update();
            var drawable = flow.FirstOrDefault(d => d is StageBubble b && b.Active);
            if (drawable != null)
                scroll.ScrollTo(drawable);
        }
    }
}
