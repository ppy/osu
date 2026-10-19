// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;

namespace osu.Game.Overlays
{
    public partial class ChatTicker : VisibilityContainer
    {
        private readonly BindableBool showChatTicker = new BindableBool();

        private TickerLine? tickerLine;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Height = 18;
            RelativeSizeAxes = Axes.X;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.1f),
            });

            config.BindWith(OsuSetting.ChatTicker, showChatTicker);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            showChatTicker.BindValueChanged(showTicker =>
            {
                PostMessage(null);
                State.Value = showTicker.NewValue ? Visibility.Visible : Visibility.Hidden;
            }, true);
        }

        public void PostMessage(Message? message)
        {
            if (message == null || !showChatTicker.Value)
            {
                PopOut();
                return;
            }

            if (tickerLine != null)
                Remove(tickerLine, false);

            Add(tickerLine = new TickerLine(message));

            this.FadeOutFromOne(10000);
        }

        protected override void PopIn()
        {
        }

        protected override void PopOut() => this.FadeOut(100);

        protected partial class TickerLine : ChatLine
        {
            protected override float Spacing => 5;
            protected override float UsernameWidth => 90;

            public TickerLine(Message message)
                : base(message)
            {
                UsernameIsClickable = false;
            }
        }
    }
}
