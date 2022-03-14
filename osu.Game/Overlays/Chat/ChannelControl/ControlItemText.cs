// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat.ChannelControl
{
    public class ControlItemText : Container
    {
        private readonly Channel channel;

        private OsuSpriteText? text;

        [Resolved]
        private BindableBool unread { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public ControlItemText(Channel channel)
        {
            this.channel = channel;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Child = text = new OsuSpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Text = channel.Type == ChannelType.Public ? $"# {channel.Name.Substring(1)}" : channel.Name,
                Font = OsuFont.Torus.With(size: 17, weight: FontWeight.SemiBold),
                Colour = colourProvider.Light3,
                Margin = new MarginPadding { Bottom = 2 },
                RelativeSizeAxes = Axes.X,
                Truncate = true,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            unread.BindValueChanged(change =>
            {
                text!.Colour = change.NewValue ? colourProvider.Content1 : colourProvider.Light3;
            }, true);
        }
    }
}
