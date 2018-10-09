// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using System.Collections.Generic;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDeflate : Mod, IApplicableToDrawableHitObjects
    {
        public override string Name => "Deflate";
        public override string ShortenedName => "DF";
        public override FontAwesome Icon => FontAwesome.fa_compress;
        public override ModType Type => ModType.Fun;
        public override string Description => "Become one with the approach circle...";
        public override double ScoreMultiplier => 1;

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables)
                drawable.ApplyCustomUpdateState += drawableOnApplyCustomUpdateState;
        }

        private void drawableOnApplyCustomUpdateState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject d))
                return;

            var h = d.HitObject;

            switch (drawable)
            {
                case DrawableHitCircle c:
                    c.ApproachCircle.Hide();
                    using (d.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                    {
                        var origScale = d.Scale;
                        d.ScaleTo(1.1f, 1)
                            .Then()
                            .ScaleTo(origScale, h.TimePreempt);
                    }
                    switch (state)
                    {
                        case ArmedState.Miss:
                            d.FadeOut(100);
                            break;
                        case ArmedState.Hit:
                            d.FadeOut(800)
                                .ScaleTo(d.Scale * 1.5f, 400, Easing.OutQuad);
                            break;
                    }
                    break;
                case DrawableSlider s:
                    using (d.BeginAbsoluteSequence(h.StartTime - h.TimePreempt + 1, true))
                    {
                        float origPathWidth = s.Body.PathWidth;
                        var origBodySize = s.Body.Size;
                        var origBodyDrawPos = s.Body.DrawPosition;

                        s.Body.MoveTo(origBodyDrawPos - new Vector2(origPathWidth), 1)
                            .Then()
                            .MoveTo(origBodyDrawPos, h.TimePreempt);

                        s.Body.ResizeTo(origBodySize * 2, 1)
                            .Then()
                            .ResizeTo(origBodySize, h.TimePreempt);

                        s.Body.TransformTo("PathWidth", origPathWidth * 2, 1)
                            .Then()
                            .TransformTo("PathWidth", origPathWidth, h.TimePreempt);
                    }
                    break;
            }
        }
    }
}
