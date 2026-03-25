// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users.Drawables
{
    [LongRunningLoad]
    public partial class DrawableTeamFlag : CompositeDrawable
    {
        private readonly APITeam? team;

        private readonly Sprite sprite;

        /// <summary>
        /// A simple, non-interactable flag sprite for the specified user.
        /// </summary>
        /// <param name="team">The team. A null value will show a placeholder background.</param>
        public DrawableTeamFlag(APITeam? team)
        {
            this.team = team;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.FromHex("333"),
                },
                sprite = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(LargeTextureStore textures)
        {
            if (team != null)
                sprite.Texture = textures.Get(team.FlagUrl);
        }
    }
}
