// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public class RankedPlayChatDisplay : CompositeDrawable
    {
        [Resolved]
        private ChannelManager? channelManager { get; set; }

        private readonly MultiplayerRoom room;

        private StandAloneChatDisplay chat = null!;

        private Channel? channel;

        public RankedPlayChatDisplay(MultiplayerRoom room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                chat = new StandAloneChatDisplay(false)
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            channel = channelManager?.JoinChannel(new Channel { Id = room.ChannelID, Type = ChannelType.Multiplayer, Name = $"#lazermp_{room.RoomID}" });

            if (channel != null)
                channel.NewMessagesArrived += onNewMessagesArrived;

            chat.Channel.Value = channel;
        }

        private void onNewMessagesArrived(IEnumerable<Message> obj)
        {
        }

        public void Appear()
        {
            FinishTransforms();

            this.MoveToY(150f)
                .FadeOut()
                .MoveToY(0f, 240, Easing.OutCubic)
                .FadeIn(240, Easing.OutCubic);
        }

        public TransformSequence<RankedPlayChatDisplay> Disappear()
        {
            FinishTransforms();

            return this.FadeOut(240, Easing.InOutCubic)
                       .MoveToY(150f, 240, Easing.InOutCubic);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (channel != null)
                channel.NewMessagesArrived -= onNewMessagesArrived;
        }
    }
}
