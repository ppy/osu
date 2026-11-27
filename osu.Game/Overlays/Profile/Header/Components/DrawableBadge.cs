// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Online;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    [LongRunningLoad]
    public partial class DrawableBadge : OsuClickableContainer, IHasCustomTooltip<DrawableBadgeTooltipData>
    {
        public static readonly Vector2 DRAWABLE_BADGE_SIZE = new Vector2(86, 40);

        private readonly Badge badge;

        public DrawableBadgeTooltipData? TooltipContent { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public DrawableBadge(Badge badge)
        {
            this.badge = badge;
            Size = DRAWABLE_BADGE_SIZE;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures, ILinkHandler? linkHandler)
        {
            Child = new Sprite
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fit,
                RelativeSizeAxes = Axes.Both,
                Texture = textures.Get(badge.ImageUrl),
            };

            if (!string.IsNullOrEmpty(badge.Url))
                Action = () => linkHandler?.HandleLink(badge.Url);

            TooltipContent = new DrawableBadgeTooltipData(colourProvider, badge);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(200);
        }

        public override LocalisableString TooltipText => string.Empty;

        public ITooltip<DrawableBadgeTooltipData> GetCustomTooltip() => new DrawableBadgeTooltip();
    }
}
