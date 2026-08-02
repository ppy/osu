// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RankRangePill : CompositeDrawable
    {
        private OsuTextFlowContainer rankFlow = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            updateState();
        }

        private void onRoomUpdated() => Scheduler.AddOnce(updateState);

        private void updateState()
        {
            rankFlow.Clear();

            if (client.Room == null || client.Room.Users.All(u => u.User == null))
            {
                rankFlow.AddText("-");
                return;
            }

            int minRank = client.Room.Users.Select(u => u.User?.Statistics.GlobalRank ?? 0).DefaultIfEmpty(0).Min();
            int maxRank = client.Room.Users.Select(u => u.User?.Statistics.GlobalRank ?? 0).DefaultIfEmpty(0).Max();

            rankFlow.AddText("#");
            rankFlow.AddText(minRank.ToString("#,0"), s => s.Font = s.Font.With(weight: FontWeight.Bold));

            rankFlow.AddText(" - ");

            rankFlow.AddText("#");
            rankFlow.AddText(maxRank.ToString("#,0"), s => s.Font = s.Font.With(weight: FontWeight.Bold));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
        }
    }
}
