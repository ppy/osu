// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;

namespace osu.Game.Graphics.Cursor
{
    public partial class OsuTooltipContainer : TooltipContainer
    {
        protected override ITooltip CreateTooltip() => new OsuTooltip();

        public OsuTooltipContainer(CursorContainer cursor)
            : base(cursor)
        {
        }

        protected override double AppearDelay => (1 - CurrentTooltip.Alpha) * base.AppearDelay; // reduce appear delay if the tooltip is already partly visible.

        public partial class OsuTooltip : Tooltip
        {
            public const float MAX_WIDTH = 500;
            private const float padding = 5;

            private readonly Box background;
            private readonly TextFlowContainer text;
            private bool instantMovement = true;

            private LocalisableString lastContent;

            [Resolved]
            private OsuColour colour { get; set; } = null!;

            public override void SetContent(LocalisableString content)
            {
                if (content.Equals(lastContent))
                    return;

                text.Text = content;

                if (IsPresent)
                {
                    AutoSizeDuration = 250;
                    background.FlashColour(colour.Gray3, 1000, Easing.OutQuint);
                }
                else
                    AutoSizeDuration = 0;

                lastContent = content;
            }

            public OsuTooltip()
            {
                AutoSizeEasing = Easing.OutPow10;

                CornerRadius = 5;
                Masking = true;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.15f),
                    Radius = 5,
                };
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.9f,
                    },
                    text = new TextFlowContainer(f =>
                    {
                        f.Font = OsuFont.Style.Caption1;
                    })
                    {
                        Margin = new MarginPadding(padding),
                        ParagraphSpacing = -0.1f,
                        AutoSizeAxes = Axes.Both,
                        MaximumSize = new Vector2(MAX_WIDTH - 2 * padding, float.PositiveInfinity),
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                background.Colour = colour.Gray2;
            }

            protected override void PopIn()
            {
                instantMovement |= !IsPresent;
                this.FadeIn(300, Easing.OutQuint);
            }

            protected override void PopOut() => this.Delay(150).FadeOut(300, Easing.OutQuint);

            public override void Move(Vector2 pos)
            {
                if (instantMovement)
                {
                    Position = pos;
                    instantMovement = false;
                }
                else
                {
                    // This method is called every frame so we can do this safely here.
                    Position = Interpolation.ValueAt(Time.Elapsed, Position, pos, 0, 120, Easing.OutQuint);
                }
            }
        }
    }
}
