// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.OnlinePlay.Match.Components;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    public partial class MatchmakingChatDisplay : MatchChatDisplay, IKeyBindingHandler<GlobalAction>
    {
        protected new ChatTextBox TextBox => base.TextBox!;

        public MatchmakingChatDisplay(Room room, bool leaveChannelOnDispose = true)
            : base(room, leaveChannelOnDispose)
        {
        }

        [BackgroundDependencyLoader]
        private void load(RealmKeyBindingStore keyBindingStore)
        {
            resetPlaceholderText();

            TextBox.HoldFocus = false;
            TextBox.ReleaseFocusOnCommit = true;
            TextBox.Focus = () => TextBox.PlaceholderText = ChatStrings.InputPlaceholder;
            TextBox.FocusLost = resetPlaceholderText;

            void resetPlaceholderText() => TextBox.PlaceholderText = Localisation.ChatStrings.InGameInputPlaceholder(keyBindingStore.GetBindingsStringFor(GlobalAction.ToggleChatFocus));
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (TextBox.HasFocus)
                    {
                        Schedule(() => TextBox.KillFocus());
                        return true;
                    }

                    break;

                case GlobalAction.ToggleChatFocus:
                    if (TextBox.HasFocus)
                    {
                        Schedule(() => TextBox.KillFocus());
                    }
                    else
                    {
                        Schedule(() => TextBox.TakeFocus());
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

        public TransformSequence<MatchmakingChatDisplay> Disappear()
        {
            FinishTransforms();

            return this.FadeOut(240, Easing.InOutCubic)
                       .MoveToY(150f, 240, Easing.InOutCubic);
        }
    }
}
