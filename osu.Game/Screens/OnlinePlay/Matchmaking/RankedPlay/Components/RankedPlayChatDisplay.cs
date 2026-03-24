// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public class RankedPlayChatDisplay : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private ChannelManager channelManager { get; set; } = null!;

        [Resolved]
        private RealmKeyBindingStore keyBindingStore { get; set; } = null!;

        private readonly MultiplayerRoom room;

        private StandAloneChatDisplay chat = null!;
        private ChatTextBox textbox = null!;

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
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                textbox = new ChatTextBox
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.X,
                    Height = 30,
                    CornerRadius = 10,
                    ReleaseFocusOnCommit = false,
                    HoldFocus = false,
                    Focus = () => textbox.PlaceholderText = ChatStrings.InputPlaceholder,
                    FocusLost = resetPlaceholderText
                }
            };

            resetPlaceholderText();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            channel = channelManager.JoinChannel(new Channel { Id = room.ChannelID, Type = ChannelType.Multiplayer, Name = $"#lazermp_{room.RoomID}" });
            channel.NewMessagesArrived += onNewMessagesArrived;

            chat.Channel.Value = channel;
        }

        private void onNewMessagesArrived(IEnumerable<Message> obj)
        {
        }

        private void resetPlaceholderText()
        {
            textbox.PlaceholderText = Localisation.ChatStrings.InGameInputPlaceholder(keyBindingStore.GetBindingsStringFor(GlobalAction.ToggleChatFocus));
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (textbox.HasFocus)
                    {
                        Schedule(() => textbox.KillFocus());
                        return true;
                    }

                    break;

                case GlobalAction.ToggleChatFocus:
                    if (textbox.HasFocus)
                    {
                        Schedule(() => textbox.KillFocus());
                    }
                    else
                    {
                        Schedule(() => textbox.TakeFocus());
                    }

                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
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

        private class ChatTextBox : StandAloneChatDisplay.ChatTextBox
        {
            protected override void LoadComplete()
            {
                base.LoadComplete();

                BackgroundFocused = Colour4.FromHex("222228");
                BackgroundUnfocused = BackgroundFocused.Opacity(0.7f);
                Placeholder.Colour = Color4.White;
            }
        }
    }
}
