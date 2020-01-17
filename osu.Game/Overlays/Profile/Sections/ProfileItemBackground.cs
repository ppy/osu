// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections
{
    public class ProfileItemBackground : OsuHoverContainer
    {
        protected override IEnumerable<Drawable> EffectTargets => new[] { background };
        protected override Container<Drawable> Content => content;

        private readonly Box background;
        private readonly Container content;

        public ProfileItemBackground()
        {
            RelativeSizeAxes = Axes.Both;
            Enabled.Value = true; //manually enabled, because we have no action
            Masking = true;
            CornerRadius = 6;

            base.Content.AddRange(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.GreySeafoam;
            HoverColour = colours.GreySeafoamLight;
        }
    }
}
