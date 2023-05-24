// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RankRangePill : MultiplayerRoomComposite
    {
        private OsuTextFlowContainer rankFlow;

        public RankRangePill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new PillContainer
            {
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(4),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(8),
                            Icon = FontAwesome.Solid.User
                        },
                        rankFlow = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12))
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                        }
                    }
                }
            };
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            rankFlow.Clear();

            if (Room == null || Room.Users.All(u => u.User == null))
            {
                rankFlow.AddText("-");
                return;
            }

            int minRank = Room.Users.Select(u => u.User?.Statistics.GlobalRank ?? 0).DefaultIfEmpty(0).Min();
            int maxRank = Room.Users.Select(u => u.User?.Statistics.GlobalRank ?? 0).DefaultIfEmpty(0).Max();

            rankFlow.AddText("#");
            rankFlow.AddText(minRank.ToString("#,0"), s => s.Font = s.Font.With(weight: FontWeight.Bold));

            rankFlow.AddText(" - ");

            rankFlow.AddText("#");
            rankFlow.AddText(maxRank.ToString("#,0"), s => s.Font = s.Font.With(weight: FontWeight.Bold));
        }
    }
}
