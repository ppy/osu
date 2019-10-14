// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Comments
{
    public class DeletedChildsPlaceholder : FillFlowContainer
    {
        public readonly BindableBool ShowDeleted = new BindableBool();
        public readonly BindableInt DeletedCount = new BindableInt();

        private bool canBeShown;

        private readonly SpriteText countText;

        public DeletedChildsPlaceholder()
        {
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Spacing = new Vector2(3, 0);
            Margin = new MarginPadding { Vertical = 10, Left = 80 };
            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    Icon = FontAwesome.Solid.Trash,
                    Size = new Vector2(14),
                },
                countText = new SpriteText
                {
                    Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                }
            };
        }

        protected override void LoadComplete()
        {
            DeletedCount.BindValueChanged(onCountChanged, true);
            ShowDeleted.BindValueChanged(onShowDeletedChanged, true);
            base.LoadComplete();
        }

        private void onShowDeletedChanged(ValueChangedEvent<bool> showDeleted)
        {
            if (canBeShown)
                this.FadeTo(showDeleted.NewValue ? 0 : 1);
        }

        private void onCountChanged(ValueChangedEvent<int> count)
        {
            canBeShown = count.NewValue != 0;

            if (!canBeShown)
            {
                Hide();
                return;
            }

            string str = $@"{count.NewValue} deleted comment";

            if (!(count.NewValue.ToString().EndsWith("1") && !count.NewValue.ToString().EndsWith("11")))
                str += "s";

            countText.Text = str;
            Show();
        }
    }
}
