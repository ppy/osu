// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class RoomSpecialCategoryPill : OnlinePlayComposite
    {
        private SpriteText text;
        private PillContainer pill;

        [Resolved]
        private OsuColour colours { get; set; }

        public RoomSpecialCategoryPill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = pill = new PillContainer
            {
                Background =
                {
                    Colour = colours.Pink,
                    Alpha = 1
                },
                Child = text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12),
                    Colour = Color4.Black
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Category.BindValueChanged(c =>
            {
                text.Text = c.NewValue.GetLocalisableDescription();

                var backgroundColour = colours.ForRoomCategory(Category.Value);
                if (backgroundColour != null)
                    pill.Background.Colour = backgroundColour.Value;
            }, true);
        }
    }
}
