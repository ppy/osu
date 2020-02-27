// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
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
                fill.RelativeSizeAxes = value;
            }
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set
            {
                base.AutoSizeAxes = value;
                fill.AutoSizeAxes = value;
            }
        }

        public FillDirection Direction
        {
            get => fill.Direction;
            set => fill.Direction = value;
        }

        private readonly FillFlowContainer<UserTile> fill;

        public ParticipantsList()
        {
            InternalChild = fill = new FillFlowContainer<UserTile> { Spacing = new Vector2(10) };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Participants.CollectionChanged += (_, __) => updateParticipants();
            updateParticipants();
        }

        private ScheduledDelegate scheduledUpdate;

        private void updateParticipants()
        {
            scheduledUpdate?.Cancel();
            scheduledUpdate = Schedule(() =>
            {
                // Remove all extra tiles with a nice, progressive fade
                int time = 500;

                for (int i = Participants.Count; i < fill.Count; i++)
                {
                    var tile = fill[i];

                    tile.Delay(500 - time).FadeOut(time, Easing.Out);
                    time = Math.Max(20, time - 20);
                    tile.Expire();
                }

                // Add new tiles for all new players
                for (int i = fill.Count; i < Participants.Count; i++)
                {
                    var tile = new UserTile();
                    fill.Add(tile);

                    tile.ClearTransforms();
                    tile.LifetimeEnd = double.MaxValue;
                    tile.FadeInFromZero(250, Easing.OutQuint);
                }

                for (int i = 0; i < Participants.Count; i++)
                    fill[i].User = Participants[i];
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
                        Colour = OsuColour.FromHex(@"27252d"),
                    },
                    avatar = new UpdateableAvatar { RelativeSizeAxes = Axes.Both },
                };
            }
        }
    }
}
