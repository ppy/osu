// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class GameplayChatDisplay : MatchChatDisplay, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private ILocalUserPlayInfo localUserInfo { get; set; }

        private IBindable<bool> localUserPlaying = new Bindable<bool>();

        public override bool PropagatePositionalInputSubTree => !localUserPlaying.Value;

        public Bindable<bool> Expanded = new Bindable<bool>();

        private readonly Bindable<bool> expandedFromTextboxFocus = new Bindable<bool>();

        private const float height = 100;

        public override bool PropagateNonPositionalInputSubTree => true;

        public GameplayChatDisplay(Room room)
            : base(room, leaveChannelOnDispose: false)
        {
            RelativeSizeAxes = Axes.X;

            Background.Alpha = 0.2f;

            Textbox.FocusLost = () => expandedFromTextboxFocus.Value = false;
        }

        protected override bool OnHover(HoverEvent e) => true; // use UI mouse cursor.

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localUserPlaying = localUserInfo.IsPlaying.GetBoundCopy();
            localUserPlaying.BindValueChanged(playing =>
            {
                // for now let's never hold focus. this avoid misdirected gameplay keys entering chat.
                // note that this is done within this callback as it triggers an un-focus as well.
                Textbox.HoldFocus = false;

                // only hold focus (after sending a message) during breaks
                Textbox.ReleaseFocusOnCommit = playing.NewValue;
            }, true);

            Expanded.BindValueChanged(_ => updateExpandedState(), true);
            expandedFromTextboxFocus.BindValueChanged(focus =>
            {
                if (focus.NewValue)
                    updateExpandedState();
                else
                {
                    // on finishing typing a message there should be a brief delay before hiding.
                    using (BeginDelayedSequence(600))
                        updateExpandedState();
                }
            }, true);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (Textbox.HasFocus)
                    {
                        Schedule(() => Textbox.KillFocus());
                        return true;
                    }

                    break;

                case GlobalAction.ToggleChatFocus:
                    if (Textbox.HasFocus)
                    {
                        Schedule(() => Textbox.KillFocus());
                    }
                    else
                    {
                        expandedFromTextboxFocus.Value = true;

                        // schedule required to ensure the textbox has become present from above bindable update.
                        Schedule(() => Textbox.TakeFocus());
                    }

                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void updateExpandedState()
        {
            if (Expanded.Value || expandedFromTextboxFocus.Value)
            {
                this.FadeIn(300, Easing.OutQuint);
                this.ResizeHeightTo(height, 500, Easing.OutQuint);
            }
            else
            {
                this.FadeOut(300, Easing.OutQuint);
                this.ResizeHeightTo(0, 500, Easing.OutQuint);
            }
        }
    }
}
