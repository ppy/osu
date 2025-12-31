// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormFieldCaption : CompositeDrawable, IHasTooltip
    {
        private OsuSpriteText captionText = null!;
        private SpriteIcon questionIcon = null!;

        private LocalisableString caption;

        public LocalisableString Caption
        {
            get => caption;
            set
            {
                caption = value;

                if (captionText.IsNotNull())
                    captionText.Text = value;
            }
        }

        private LocalisableString tooltipText;

        public LocalisableString TooltipText
        {
            get => tooltipText;
            set
            {
                tooltipText = value;

                if (questionIcon.IsNotNull())
                    questionIcon.Alpha = value == default ? 0 : 1;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    captionText = new OsuSpriteText
                    {
                        Text = caption,
                        Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold),
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    questionIcon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Alpha = TooltipText == default ? 0 : 1,
                        Size = new Vector2(10),
                        Icon = FontAwesome.Solid.QuestionCircle,
                        Margin = new MarginPadding { Top = 1 },
                    }
                },
            };
        }
    }
}
