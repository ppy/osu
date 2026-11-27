// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public partial class DrawableBadgeTooltip : VisibilityContainer, ITooltip<DrawableBadgeTooltipData>
    {
        private OsuSpriteText description = null!;
        private OsuSpriteText date = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;
            CornerRadius = 5;
            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.9f,
                    Colour = colours.Gray2,
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(5),
                    Padding = new MarginPadding(5),
                    Children = new Drawable[]
                    {
                        description = new OsuSpriteText
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Font = OsuFont.Style.Caption1,
                        },
                        date = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.Style.Caption1,
                        },
                    },
                },
            };
        }

        public void SetContent(DrawableBadgeTooltipData content)
        {
            description.Text = content.Badge.Description;
            date.Text = content.Badge.AwardedAt.ToLocalisedMediumDate();
            date.Colour = content.ColourProvider.Light1;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;
    }

    public record DrawableBadgeTooltipData(OverlayColourProvider ColourProvider, Badge Badge);
}
