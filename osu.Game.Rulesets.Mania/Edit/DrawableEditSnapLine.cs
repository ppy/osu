// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Edit
{
    /// <summary>
    /// Visualises a <see cref="EditSnapLine"/>. Although this derives DrawableManiaHitObject,
    /// this does not handle input/sound like a normal hit object.
    /// </summary>
    public class DrawableEditSnapLine : DrawableManiaHitObject<EditSnapLine>
    {
        public DrawableEditSnapLine(EditSnapLine snapLine)
            : base(snapLine)
        {
            RelativeSizeAxes = Axes.X;
            Height = 2;

            AddInternal(new Box
            {
                Name = "Snap line",
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Colour = snapLine.Colour
            });

            bool isMajor = snapLine.BeatIndex % snapLine.BeatDivisor.Value == 0 && snapLine.BeatIndex / snapLine.BeatDivisor.Value % (int)snapLine.ControlPoint.TimeSignature == 0;

            if (isMajor)
            {
                Height = 4;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
