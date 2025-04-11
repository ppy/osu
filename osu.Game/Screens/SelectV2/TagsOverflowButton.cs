// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class TagsOverflowButton : CompositeDrawable, IHasPopover, IHasLineBaseHeight
    {
        private readonly string[] tags;

        private Box box = null!;
        private OsuSpriteText text = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        public float LineBaseHeight => text.LineBaseHeight;

        public TagsOverflowButton(string[] tags)
        {
            this.tags = tags;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(OsuFont.Style.Caption1.Size);
            CornerRadius = 1.5f;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                box = new Box
                {
                    Colour = colourProvider.Light1,
                    RelativeSizeAxes = Axes.Both,
                },
                text = new OsuSpriteText
                {
                    Y = -2,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "...",
                    Colour = colourProvider.Background4,
                    Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            box.FadeColour(colourProvider.Content2, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            box.FadeColour(colourProvider.Light1, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            box.FlashColour(colourProvider.Content1, 300, Easing.OutQuint);
            this.ShowPopover();
            return true;
        }

        public Popover GetPopover() => new TagsOverflowPopover(tags, songSelect);
    }
}
