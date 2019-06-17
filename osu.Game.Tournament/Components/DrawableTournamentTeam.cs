// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Tournament.Components
{
    public abstract class DrawableTournamentTeam : CompositeDrawable
    {
        public readonly TournamentTeam Team;

        protected readonly Sprite Flag;
        protected readonly OsuSpriteText AcronymText;

        protected DrawableTournamentTeam(TournamentTeam team)
        {
            Team = team;

            Flag = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit
            };

            AcronymText = new OsuSpriteText
            {
                Text = team?.Acronym.Value?.ToUpperInvariant() ?? string.Empty,
                Font = OsuFont.GetFont(weight: FontWeight.Regular),
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            if (Team != null)
                Flag.Texture = textures.Get($@"Flags/{Team.FlagName}");
        }
    }
}
