// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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
                Text = team?.Acronym?.ToUpperInvariant() ?? string.Empty,
                Font = @"Exo2.0-Regular"
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
