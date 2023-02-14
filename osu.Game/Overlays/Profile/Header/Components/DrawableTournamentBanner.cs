// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header.Components
{
    [LongRunningLoad]
    public partial class DrawableTournamentBanner : OsuClickableContainer
    {
        private readonly TournamentBanner banner;

        public DrawableTournamentBanner(TournamentBanner banner)
        {
            this.banner = banner;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures, OsuGame? game, IAPIProvider api)
        {
            Child = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Texture = textures.Get(banner.Image),
            };

            Action = () => game?.OpenUrlExternally($@"{api.WebsiteRootUrl}/community/tournaments/{banner.TournamentId}");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeInFromZero(200);
        }

        public override LocalisableString TooltipText => "view in browser";
    }
}
