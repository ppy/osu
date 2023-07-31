// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Bindables;
using Humanizer;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Comments
{
    public partial class DeletedCommentsCounter : CompositeDrawable
    {
        public readonly BindableBool ShowDeleted = new BindableBool();

        public readonly BindableInt Count = new BindableInt();

        private readonly SpriteText countText;

        public DeletedCommentsCounter()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(3, 0),
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Regular.TrashAlt,
                        Size = new Vector2(14),
                    },
                    countText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Count.BindValueChanged(_ => updateDisplay(), true);
            ShowDeleted.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            if (!ShowDeleted.Value && Count.Value != 0)
            {
                countText.Text = @"deleted comment".ToQuantity(Count.Value);
                Show();
            }
            else
                Hide();
        }
    }
}
