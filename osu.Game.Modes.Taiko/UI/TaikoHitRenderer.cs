﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables.BarLines;
using osu.Game.Modes.Taiko.Objects.Drawables.Bashes;
using osu.Game.Modes.Taiko.Objects.Drawables.DrumRolls;
using osu.Game.Modes.Taiko.Objects.Drawables.Hits;
using osu.Game.Modes.UI;
using OpenTK;

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

        protected override Playfield<TaikoHitObject> CreatePlayfield() => new TaikoPlayfield
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

                if ((h.Type & TaikoHitType.RimHit) > 0)
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
            //todo: this function should not be here.
            foreach (BarLine line in new TaikoConverter().ConvertBarLines(beatmap))
                ((TaikoPlayfield)Playfield).AddBarLine(line.IsMajor ? new DrawableMajorBarLine(line) : new DrawableBarLine(line));
        }
    }
}
