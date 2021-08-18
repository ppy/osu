// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
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

        private const float height = 100;

        public GameplayChatDisplay()
            : base(leaveChannelOnDispose: false)
        {
            RelativeSizeAxes = Axes.X;

            Background.Alpha = 0.2f;
        }

        private void expandedChanged(ValueChangedEvent<bool> expanded)
        {
            if (expanded.NewValue)
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

            Expanded.BindValueChanged(expandedChanged, true);
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.FocusChatInput:
                    Textbox.TakeFocus();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }
    }
}
