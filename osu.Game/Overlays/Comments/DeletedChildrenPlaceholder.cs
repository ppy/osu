// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Bindables;
using Humanizer;

namespace osu.Game.Overlays.Comments
{
    public class DeletedChildrenPlaceholder : FillFlowContainer
    {
        public readonly BindableBool ShowDeleted = new BindableBool();
        public readonly BindableInt DeletedCount = new BindableInt();

        private readonly SpriteText countText;

        public DeletedChildrenPlaceholder()
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
            DeletedCount.BindValueChanged(_ => updateDisplay(), true);
            ShowDeleted.BindValueChanged(_ => updateDisplay(), true);
            base.LoadComplete();
        }

        private void updateDisplay()
        {
            if (DeletedCount.Value != 0)
            {
                countText.Text = @"deleted comment".ToQuantity(DeletedCount.Value);
                this.FadeTo(ShowDeleted.Value ? 0 : 1);
            }
            else
            {
                Hide();
            }
        }
    }
}
