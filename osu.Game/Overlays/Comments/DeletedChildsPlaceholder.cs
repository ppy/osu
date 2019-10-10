// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Bindables;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public class DeletedChildsPlaceholder : FillFlowContainer
    {
        private const int deleted_placeholder_margin = 80;
        private const int margin = 10;

        public readonly BindableBool ShowDeleted = new BindableBool();

        private readonly bool canBeVisible;

        public DeletedChildsPlaceholder(int count)
        {
            canBeVisible = count != 0;

            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(3, 0);
            Margin = new MarginPadding { Vertical = margin, Left = deleted_placeholder_margin };
            Alpha = 0;
            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.Trash,
                    Size = new Vector2(14),
                },
                new SpriteText
                {
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                    Text = $@"{count} deleted comment{(count.ToString().ToCharArray().Last() == '1' ? "" : "s")}"
                }
            };
        }

        protected override void LoadComplete()
        {
            ShowDeleted.BindValueChanged(onShowDeletedChanged, true);
            base.LoadComplete();
        }

        private void onShowDeletedChanged(ValueChangedEvent<bool> showDeleted)
        {
            if (canBeVisible)
                this.FadeTo(showDeleted.NewValue ? 0 : 1);
        }
    }
}
