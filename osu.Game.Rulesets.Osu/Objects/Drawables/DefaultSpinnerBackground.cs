// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DefaultSpinnerBackground : SpinnerFill
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, DrawableHitObject drawableHitObject)
        {
            Disc.Alpha = 0;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            drawableHitObject.State.BindValueChanged(val =>
            {
                Color4 colour;

                switch (val.NewValue)
                {
                    default:
                        colour = colours.BlueDark;
                        break;

                    case ArmedState.Hit:
                        colour = colours.YellowLight;
                        break;
                }

                this.FadeAccent(colour.Darken(1), 200);
            }, true);

            FinishTransforms(true);
        }
    }
}
