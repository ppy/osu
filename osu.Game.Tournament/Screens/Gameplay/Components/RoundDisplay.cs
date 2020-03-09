// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class RoundDisplay : CompositeDrawable
    {
        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();

        private readonly TournamentSpriteText text;

        public RoundDisplay()
        {
            Width = 200;
            Height = 20;

            Masking = true;
            CornerRadius = 10;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = OsuColour.Gray(0.18f),
                    RelativeSizeAxes = Axes.Both,
                },
                text = new TournamentSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.White,
                    Font = OsuFont.Torus.With(weight: FontWeight.Regular, size: 16),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match) =>
            text.Text = match.NewValue.Round.Value?.Name.Value ?? "未知回合";
    }
}
