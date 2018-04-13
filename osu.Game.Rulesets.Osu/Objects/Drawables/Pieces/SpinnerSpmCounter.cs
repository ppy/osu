﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SpinnerSpmCounter : Container
    {
        private readonly OsuSpriteText spmText;

        public SpinnerSpmCounter()
        {
            Children = new Drawable[]
            {
                spmText = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"0",
                    Font = @"Venera",
                    TextSize = 24
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"SPINS PER MINUTE",
                    Font = @"Venera",
                    TextSize = 12,
                    Y = 30
                }
            };
        }

        private double spm;

        public double SpinsPerMinute
        {
            get { return spm; }
            private set
            {
                if (value == spm) return;
                spm = value;
                spmText.Text = Math.Truncate(value).ToString(@"#0");
            }
        }

        private struct RotationRecord
        {
            public float Rotation;
            public double Time;
        }

        private readonly Queue<RotationRecord> records = new Queue<RotationRecord>();
        private const double spm_count_duration = 595; // not using hundreds to avoid frame rounding issues

        public void SetRotation(float currentRotation)
        {
            // If we've gone back in time, it's fine to work with a fresh set of records for now
            if (records.Count > 0 && Time.Current < records.Last().Time)
                records.Clear();

            if (records.Count > 0)
            {
                var record = records.Peek();
                while (records.Count > 0 && Time.Current - records.Peek().Time > spm_count_duration)
                    record = records.Dequeue();
                SpinsPerMinute = (currentRotation - record.Rotation) / (Time.Current - record.Time) * 1000 * 60 / 360;
            }

            records.Enqueue(new RotationRecord { Rotation = currentRotation, Time = Time.Current });
        }
    }
}
