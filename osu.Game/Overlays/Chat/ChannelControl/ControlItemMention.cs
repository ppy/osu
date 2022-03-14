// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Chat.ChannelControl
{
    public class ControlItemMention : CircularContainer
    {
        private int mentionCount;

        public int MentionCount
        {
            get => mentionCount;
            set
            {
                if (value == mentionCount)
                    return;

                mentionCount = value;
                updateText();
            }
        }

        private OsuSpriteText? countText;

        [Resolved]
        private OsuColour osuColour { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            Size = new Vector2(20, 12);
            Alpha = 0f;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = osuColour.YellowLight,
                },
                countText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Torus.With(size: 11, weight: FontWeight.Bold),
                    Margin = new MarginPadding { Bottom = 1 },
                    Colour = colourProvider.Background5,
                },
            };

            updateText();
        }

        private void updateText()
        {
            countText!.Text = MentionCount > 99 ? "99+" : MentionCount.ToString();

            if (mentionCount > 0)
                Show();
            else
                Hide();
        }
    }
}
