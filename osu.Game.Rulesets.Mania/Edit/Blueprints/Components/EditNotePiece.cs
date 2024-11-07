// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints.Components
{
    public partial class EditNotePiece : CompositeDrawable
    {
        [Resolved]
        private Column? column { get; set; }

        public EditNotePiece()
        {
            Masking = true;
            CornerRadius = 5;
            BorderThickness = 6;
            BorderColour = ColourInfo.GradientVertical(Colour4.White.Opacity(0.5f), Colour4.White);
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

            if (column != null)
                Scale = new Vector2(1, column.ScrollingInfo.Direction.Value == ScrollingDirection.Down ? 1 : -1);
        }
    }
}
