// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Multi.Components
{
    public class ParticipantsList : MultiplayerComposite
    {
        public const float TILE_SIZE = 35;

        public override Axes RelativeSizeAxes
        {
            get => base.RelativeSizeAxes;
            set
            {
                base.RelativeSizeAxes = value;

                if (tiles != null)
                    tiles.RelativeSizeAxes = value;
            }
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set
            {
                base.AutoSizeAxes = value;

                if (tiles != null)
                    tiles.AutoSizeAxes = value;
            }
        }

        private FillDirection direction = FillDirection.Full;

        public FillDirection Direction
        {
            get => direction;
            set
            {
                direction = value;

                if (tiles != null)
                    tiles.Direction = value;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RecentParticipants.CollectionChanged += (_, __) => updateParticipants();
            updateParticipants();
        }

        private ScheduledDelegate scheduledUpdate;
        private FillFlowContainer<UserTile> tiles;

        private void updateParticipants()
        {
            scheduledUpdate?.Cancel();
            scheduledUpdate = Schedule(() =>
            {
                tiles?.FadeOut(250, Easing.Out).Expire();

                tiles = new FillFlowContainer<UserTile>
                {
                    Alpha = 0,
                    Direction = Direction,
                    AutoSizeAxes = AutoSizeAxes,
                    RelativeSizeAxes = RelativeSizeAxes,
                    Spacing = new Vector2(10)
                };

                for (int i = 0; i < RecentParticipants.Count; i++)
                    tiles.Add(new UserTile { User = RecentParticipants[i] });

                AddInternal(tiles);

                tiles.Delay(250).FadeIn(250, Easing.OutQuint);
            });
        }

        private class UserTile : CompositeDrawable, IHasTooltip
        {
            public User User
            {
                get => avatar.User;
                set => avatar.User = value;
            }

            public string TooltipText => User?.Username ?? string.Empty;

            private readonly UpdateableAvatar avatar;

            public UserTile()
            {
                Size = new Vector2(TILE_SIZE);
                CornerRadius = 5f;
                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex(@"27252d"),
                    },
                    avatar = new UpdateableAvatar { RelativeSizeAxes = Axes.Both },
                };
            }
        }
    }
}
