// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints.Components
{
    public partial class EditNotePiece : CompositeDrawable
    {
        public EditNotePiece()
        {
            Masking = true;
            BorderThickness = 9; // organoleptically chosen to look good enough for all default skins
            BorderColour = Color4.White;
            Height = DefaultNotePiece.NOTE_HEIGHT;

            InternalChild = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Alpha = 0,
                AlwaysPresent = true,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            // from anecdotal experience, there are generally two types of user skins:
            // one type uses rectangles for notes, and the other uses circles / squarish sprites (various stepmania-likes).
            // this is a crude heuristic that attempts to choose the best of both worlds based on aspect ratio alone.
            float aspectRatio = DrawWidth / DrawHeight;
            bool isSquarish = aspectRatio > 4f / 5 && aspectRatio < 5f / 4;
            CornerRadius = isSquarish ? Math.Min(DrawWidth, DrawHeight) / 2 : 5;
        }
    }
}
