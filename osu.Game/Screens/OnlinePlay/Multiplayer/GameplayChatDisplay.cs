// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
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
    public partial class GameplayChatDisplay : MatchChatDisplay, IKeyBindingHandler<GlobalAction>
    {
        [Resolved(CanBeNull = true)]
        [CanBeNull]
        private ILocalUserPlayInfo localUserInfo { get; set; }

        private readonly IBindable<bool> localUserPlaying = new Bindable<bool>();

        public override bool PropagatePositionalInputSubTree => !localUserPlaying.Value;

        public Bindable<bool> Expanded = new Bindable<bool>();

        private readonly Bindable<bool> expandedFromTextBoxFocus = new Bindable<bool>();

        private const float height = 100;

        public override bool PropagateNonPositionalInputSubTree => true;

        public GameplayChatDisplay(Room room)
            : base(room, leaveChannelOnDispose: false)
        {
            RelativeSizeAxes = Axes.X;

            Background.Alpha = 0.2f;

            TextBox.FocusLost = () => expandedFromTextBoxFocus.Value = false;
        }

        protected override bool OnHover(HoverEvent e) => true; // use UI mouse cursor.

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (localUserInfo != null)
                localUserPlaying.BindTo(localUserInfo.IsPlaying);

            localUserPlaying.BindValueChanged(playing =>
            {
                // for now let's never hold focus. this avoid misdirected gameplay keys entering chat.
                // note that this is done within this callback as it triggers an un-focus as well.
                TextBox.HoldFocus = false;

                // only hold focus (after sending a message) during breaks
                TextBox.ReleaseFocusOnCommit = playing.NewValue;
            }, true);

            Expanded.BindValueChanged(_ => updateExpandedState(), true);
            expandedFromTextBoxFocus.BindValueChanged(focus =>
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
                        expandedFromTextBoxFocus.Value = true;

                        // schedule required to ensure the textbox has become present from above bindable update.
                        Schedule(() => TextBox.TakeFocus());
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
            if (Expanded.Value || expandedFromTextBoxFocus.Value)
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
