// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    public partial class CatchEditorPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public CatchEditorPlayfieldAdjustmentContainer()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            Size = new Vector2(0.8f, 0.9f);

            InternalChild = new ScalingContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Child = content = new Container { RelativeSizeAxes = Axes.Both },
            };
        }

        private partial class ScalingContainer : Container
        {
            public ScalingContainer()
            {
                RelativeSizeAxes = Axes.Y;
                Width = CatchPlayfield.WIDTH;
            }

            protected override void Update()
            {
                base.Update();

                Scale = new Vector2(Math.Min(Parent!.ChildSize.X / CatchPlayfield.WIDTH, Parent!.ChildSize.Y / CatchPlayfield.HEIGHT));
                Height = 1 / Scale.Y;
            }
        }
    }
}
