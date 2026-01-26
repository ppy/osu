// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class FormFieldCaption : CompositeDrawable, IHasTooltip
    {
        private OsuTextFlowContainer textFlow = null!;

        private LocalisableString caption;

        public LocalisableString Caption
        {
            get => caption;
            set
            {
                caption = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        private LocalisableString tooltipText;

        public LocalisableString TooltipText
        {
            get => tooltipText;
            set
            {
                tooltipText = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = textFlow = new OsuTextFlowContainer(t => t.Font = OsuFont.Style.Caption1)
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            textFlow.Text = caption;

            if (TooltipText != default)
            {
                textFlow.AddArbitraryDrawable(new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(10),
                    Icon = FontAwesome.Solid.QuestionCircle,
                    Margin = new MarginPadding { Left = 5 },
                    Y = 1f,
                });
            }
        }
    }
}
