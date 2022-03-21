// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Chat.ChannelList
{
    public class ChannelListItemMentionPill : CircularContainer
    {
        public readonly BindableInt Mentions = new BindableInt();

        private OsuSpriteText countText = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour osuColour, OverlayColourProvider colourProvider)
        {
            Masking = true;
            Size = new Vector2(20, 12);
            Alpha = 0f;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = osuColour.Orange1,
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Mentions.BindValueChanged(change =>
            {
                int mentionCount = change.NewValue;

                countText.Text = mentionCount > 99 ? "99+" : mentionCount.ToString();

                if (mentionCount > 0)
                    Show();
                else
                    Hide();
            }, true);
        }
    }
}
