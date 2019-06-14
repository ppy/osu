// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class DrawableTournamentGrouping : CompositeDrawable
    {
        [UsedImplicitly]
        private readonly Bindable<string> name;

        [UsedImplicitly]
        private readonly Bindable<string> description;

        public DrawableTournamentGrouping(TournamentGrouping grouping, bool losers = false)
        {
            OsuSpriteText textName;
            OsuSpriteText textDescription;

            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    textDescription = new OsuSpriteText
                    {
                        Colour = Color4.Black,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                    textName = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Bold),
                        Colour = Color4.Black,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre
                    },
                }
            };

            name = grouping.Name.GetBoundCopy();
            name.BindValueChanged(n => textName.Text = ((losers ? "Losers " : "") + grouping.Name).ToUpper(), true);

            description = grouping.Name.GetBoundCopy();
            description.BindValueChanged(n => textDescription.Text = grouping.Description.Value.ToUpper(), true);
        }
    }
}
