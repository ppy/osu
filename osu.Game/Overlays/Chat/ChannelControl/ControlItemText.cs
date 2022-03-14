// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat.ChannelControl
{
    public class ControlItemText : Container
    {
        private bool hasUnread;

        public bool HasUnread
        {
            get => hasUnread;
            set
            {
                if (hasUnread == value)
                    return;

                hasUnread = value;
                updateText();
            }
        }

        private readonly Channel channel;

        private OsuSpriteText? text;

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

        private void updateText()
        {
            if (!IsLoaded)
                return;

            if (HasUnread)
                text!.Colour = colourProvider.Content1;
            else
                text!.Colour = colourProvider.Light3;
        }
    }
}
