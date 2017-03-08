// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.UI;
using osu.Framework.Graphics;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Framework.Allocation;
using osu.Game.Beatmaps.Timing;
using System.Collections.Generic;
using osu.Game.Modes.Taiko.Objects.Drawables.Hits;
using osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls;
using osu.Game.Modes.Taiko.Objects.Drawables.Bashes;
using osu.Game.Modes.Taiko.Objects.Drawables.BarLines;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoHitRenderer : HitRenderer<TaikoHitObject>
    {
        protected override HitObjectConverter<TaikoHitObject> Converter => new TaikoConverter();

        private Beatmap beatmap;

        public TaikoHitRenderer(Beatmap beatmap)
            : base(beatmap)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            loadBarLines();
        }

        protected override Playfield<TaikoHitObject> CreatePlayfield() => new TaikoPlayfield()
        {
            RelativePositionAxes = Axes.Y,
            Position = new Vector2(0, 0.4f)
        };

        protected override DrawableHitObject<TaikoHitObject> GetVisualRepresentation(TaikoHitObject h)
        {
            if ((h.Type & TaikoHitType.Hit) > 0)
            {
                if ((h.Type & TaikoHitType.CentreHit) > 0)
                {
                    if ((h.Type & TaikoHitType.Finisher) > 0)
                        return new DrawableCentreHitFinisher(h);
                    return new DrawableCentreHit(h);
                }
                else if ((h.Type & TaikoHitType.RimHit) > 0)
                {
                    if ((h.Type & TaikoHitType.Finisher) > 0)
                        return new DrawableRimHitFinisher(h);
                    return new DrawableRimHit(h);
                }
            }
            else if ((h.Type & TaikoHitType.DrumRoll) > 0)
            {
                if ((h.Type & TaikoHitType.Finisher) > 0)
                    return new DrawableDrumRollFinisher(h as DrumRoll);
                return new DrawableDrumRoll(h as DrumRoll);
            }
            else if ((h.Type & TaikoHitType.Bash) > 0)
                return new DrawableBash(h as Bash);

            return null;
        }

        private void loadBarLines()
        {
            foreach (BarLine line in new TaikoConverter().ConvertBarLines(beatmap))
            {
                TaikoPlayfield tp = Playfield as TaikoPlayfield;
                if (line.IsMajor)
                    tp.AddBarLine(new DrawableMajorBarLine(line));
                else
                    tp.AddBarLine(new DrawableBarLine(line));
            }
        }
    }
}
