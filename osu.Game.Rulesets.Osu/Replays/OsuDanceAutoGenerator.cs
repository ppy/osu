// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays.Movers;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using static osu.Game.Rulesets.Osu.Configuration.OsuDanceMover;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuDanceAutoGenerator : OsuAutoGeneratorBase
    {
        public static BaseDanceMover GetMover(OsuDanceMover mover) =>
            mover switch
            {
                HalfCircle => new HalfCircleMover(),
                Flower => new FlowerMover(),
                Momentum => new MomentumMover(),
                _ => new MomentumMover()
            };

        public new OsuBeatmap Beatmap => (OsuBeatmap)base.Beatmap;
        private readonly bool[] ObjectsDuring;
        private readonly BaseDanceMover mover;
        private readonly OsuRulesetConfigManager config;
        private int buttonIndex;

        private readonly double frameTime;

        private OsuAction getAction(OsuHitObject h, OsuHitObject last)
        {
            double timeDifference = ApplyModsToTimeDelta(h.StartTime, last.StartTime);

            if (timeDifference > 0 && // Sanity checks
                ((last.StackedPosition - h.StackedPosition).Length > h.Radius * (1.5 + 100.0 / timeDifference) || // Either the distance is big enough
                 timeDifference >= 266)) // ... or the beats are slow enough to tap anyway.
            {
                buttonIndex = 0;
            }
            else
            {
                buttonIndex++;
            }

            return buttonIndex % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton;
        }

        public OsuDanceAutoGenerator(IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(beatmap, mods)
        {
            config = OsuRulesetConfigManager.Instance;
            frameTime = 1000.0 / config.Get<float>(OsuRulesetSetting.ReplayFramerate);
            mover = GetMover(config.Get<OsuDanceMover>(OsuRulesetSetting.DanceMover));

            mover.Beatmap = Beatmap;

            var objectsDuring = new bool[Beatmap.HitObjects.Count];

            for (int i = 0; i < Beatmap.HitObjects.Count - 1; ++i)
            {
                var e = Beatmap.HitObjects[i].GetEndTime();
                objectsDuring[i] = false;

                for (int j = i + 1; j < Beatmap.HitObjects.Count; ++j)
                {
                    if (Beatmap.HitObjects[j].StartTime + 1 > e) continue;

                    objectsDuring[i] = true;
                    break;
                }
            }

            ObjectsDuring = objectsDuring;
            mover.ObjectsDuring = objectsDuring;
        }

        private void objectGenerate(OsuHitObject o, int idx)
        {
            OsuHitObject last = Beatmap.HitObjects[idx == 0 ? idx : idx - 1];
            OsuAction action = getAction(o, last);
            var endFrame = new OsuReplayFrame(o.GetEndTime() + KEY_UP_DELAY, o.StackedEndPosition);
            var FrameDelay = GetFrameDelay(endFrame.Time);

            switch (o)
            {
                case Slider slider:
                    for (double j = GetFrameDelay(endFrame.Time); j < slider.Duration; j += (endFrame.Time))
                    {
                        Vector2 pos = slider.StackedPositionAt(j / slider.Duration);
                        AddFrameToReplay(new OsuReplayFrame(o.StartTime + j, pos, action));
                    }

                    AddFrameToReplay(new OsuReplayFrame(slider.EndTime, slider.StackedEndPosition, action));

                    break;

                case Spinner s:
                    if (s.SpinsRequired == 0) return;

                    Vector2 difference = s.StackedPosition - SPINNER_CENTRE;

                    float radius = difference.Length;
                    float angle = radius == 0 ? 0 : MathF.Atan2(difference.Y, difference.X);

                    double t;

                    for (double j = s.StartTime + FrameDelay; j < s.EndTime; j += FrameDelay)
                    {
                        t = ApplyModsToTimeDelta(s.StartTime, j) * -1;

                        Vector2 pos = SPINNER_CENTRE + CirclePosition(t / 20 + angle, SPIN_RADIUS);
                        AddFrameToReplay(new OsuReplayFrame((int)j, pos, action));
                    }

                    t = ApplyModsToTimeDelta(s.StartTime, s.EndTime) * -1;
                    Vector2 endPosition = SPINNER_CENTRE + CirclePosition(t / 20 + angle, SPIN_RADIUS);

                    AddFrameToReplay(new OsuReplayFrame(s.EndTime, endPosition, action));

                    break;

                case HitCircle c:
                    AddFrameToReplay(new OsuReplayFrame(c.StartTime, c.StackedPosition, action));
                    break;

                default: return;
            }

            if (Frames[^1].Time <= endFrame.Time)
                AddFrameToReplay(endFrame);
        }

        public override Replay Generate()
        {
            var o = Beatmap.HitObjects[0];
            AddFrameToReplay(new OsuReplayFrame(-100000, o.Position));
            AddFrameToReplay(new OsuReplayFrame(o.StartTime - 1500, o.Position));
            AddFrameToReplay(new OsuReplayFrame(o.StartTime - 1500, o.Position));

            var bs = OsuPlayfield.BASE_SIZE;

            var xf = bs.X / 0.8f * (4f / 3f);
            var x0 = (bs.X - xf) / 2f;
            var x1 = xf + x0;

            var yf = bs.Y / 0.8f;
            var y0 = (bs.Y - yf) / 2f;
            var y1 = yf + y0;

            for (int i = 0; i < Beatmap.HitObjects.Count - 1; i++)
            {
                o = Beatmap.HitObjects[i];
                objectGenerate(o, i);

                mover.ObjectIndex = i;
                mover.OnObjChange();

                for (double t = (ObjectsDuring[i] ? o.StartTime : o.GetEndTime()) + frameTime; t < mover.End.StartTime; t += frameTime)
                {
                    var v = mover.Update(t);

                    if (config.Get<bool>(OsuRulesetSetting.BorderBounce))
                    {
                        if (v.X < x0) v.X = x0 - (v.X - x0);
                        if (v.Y < y0) v.Y = y0 - (v.Y - y0);

                        if (v.X > x1)
                        {
                            var x = v.X - x0;
                            var m = (int)(x / xf);
                            x %= xf;
                            x = m % 2 == 0 ? x : xf - x;
                            v.X = x + x0;
                        }

                        if (v.Y > y1)
                        {
                            var y = v.Y - y0;
                            var m = (int)(y / yf);
                            y %= yf;
                            y = m % 2 == 0 ? y : yf - y;
                            v.Y = y + y0;
                        }
                    }

                    AddFrameToReplay(new OsuReplayFrame(t, v));
                }
            }

            objectGenerate(Beatmap.HitObjects[^1], Beatmap.HitObjects.Count - 1);

            return Replay;
        }
    }

    public abstract class BaseDanceMover
    {
        protected double StartTime => Start.GetEndTime();
        protected double EndTime => End.StartTime;
        protected double Duration => EndTime - StartTime;

        protected Vector2 StartPos => ObjectsDuring[ObjectIndex] ? Start.StackedPosition : Start.StackedEndPosition;
        protected Vector2 EndPos => End.StackedPosition;
        protected float StartX => StartPos.X;
        protected float StartY => StartPos.Y;
        protected float EndX => EndPos.X;
        protected float EndY => EndPos.Y;

        protected float T(double time) => (float)((time - StartTime) / Duration);

        public bool[] ObjectsDuring { set; protected get; }

        public int ObjectIndex { set; protected get; }
        public OsuBeatmap Beatmap { set; protected get; }

        public OsuHitObject Start
        {
            get
            {
                if (Beatmap.HitObjects[ObjectIndex] is Spinner spinner && spinner.SpinsRequired == 0) return Beatmap.HitObjects[ObjectIndex == 0 ? ObjectIndex : ObjectIndex - 1];

                return Beatmap.HitObjects[ObjectIndex];
            }
        }

        public OsuHitObject End
        {
            get
            {
                if (Beatmap.HitObjects[ObjectIndex + 1] is Spinner spinner && spinner.SpinsRequired == 0) return Beatmap.HitObjects[ObjectIndex == Beatmap.HitObjects.Count ? ObjectIndex - 1 : ObjectIndex];

                return Beatmap.HitObjects[ObjectIndex + 1];
            }
        }

        public virtual void OnObjChange() { }
        public abstract Vector2 Update(double time);
    }
}
