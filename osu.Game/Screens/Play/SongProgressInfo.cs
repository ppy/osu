// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using System;

namespace osu.Game.Screens.Play
{
    public class SongProgressInfo : Container
    {
        private OsuSpriteText timeCurrent;
        private OsuSpriteText timeLeft;
        private OsuSpriteText progress;

        private double startTime;
        private double endTime;

        private int? previousPercent;
        private int? previousSecond;

        private double songLength => endTime - startTime;

        private const int margin = 10;

        public double StartTime
        {
            set => startTime = value;
        }

        public double EndTime
        {
            set => endTime = value;
        }

        private GameplayClock gameplayClock;
        private SongProgress songProgressParent;

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, GameplayClock clock, SongProgress parent)
        {
            if (clock != null)
                gameplayClock = clock;

            if (parent != null)
                songProgressParent = parent;

            Children = new Drawable[]
            {
                timeCurrent = new OsuSpriteText
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Colour = colours.BlueLighter,
                    Font = OsuFont.Numeric,
                    Margin = new MarginPadding
                    {
                        Left = margin,
                    },
                },
                progress = new OsuSpriteText
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Colour = colours.BlueLighter,
                    Font = OsuFont.Numeric,
                },
                timeLeft = new OsuSpriteText
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    Colour = colours.BlueLighter,
                    Font = OsuFont.Numeric,
                    Margin = new MarginPadding
                    {
                        Right = margin,
                    },
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            double time = gameplayClock?.CurrentTime ?? Time.Current;

            double songCurrentTime = time - startTime;
            int currentPercent = Math.Max(0, Math.Min(100, (int)(songCurrentTime / songLength * 100)));
            int currentSecond = (int)Math.Floor(songCurrentTime / 1000.0);

            if (currentPercent != previousPercent)
            {
                progress.Text = currentPercent.ToString() + @"%";
                previousPercent = currentPercent;
            }

            if (currentSecond != previousSecond && songCurrentTime < songLength)
            {
                timeCurrent.Text = formatTime(TimeSpan.FromSeconds(currentSecond));
                timeLeft.Text = formatTime(TimeSpan.FromMilliseconds(endTime - time));

                previousSecond = currentSecond;
            }

            // Undo rotations done by the progress container
            timeCurrent.Rotation = -songProgressParent.Rotation;
            progress.Rotation = -songProgressParent.Rotation;
            timeLeft.Rotation = -songProgressParent.Rotation;

            // Undo flips done by the progress container
            Vector2 textScale = new Vector2 { X = Math.Sign(songProgressParent.Scale.X), Y = Math.Sign(songProgressParent.Scale.Y) };

            // If we are rotated 90 degrees in either direction,
            // we need to swap horizontal flips with vertical flips and vice versa.
            int parentRotationInQuarterTurns = (int)Math.Floor(songProgressParent.Rotation / 90);
            if (parentRotationInQuarterTurns % 2 != 0)
                textScale = new Vector2 { X = textScale.Y, Y = textScale.X };

            timeCurrent.Scale = textScale;
            progress.Scale = textScale;
            timeLeft.Scale = textScale;

            // Choose the correct origin based on rotation and flip
            Anchor currentTimeOriginX = Anchor.x0;
            Anchor currentTimeOriginY = Anchor.y2;

            Anchor timeLeftOriginX = Anchor.x2;
            Anchor timeLeftOriginY = Anchor.y2;

            // We use a proper maths modulus (negative % positive = positive) to make the if statements smaller
            int parentRotationInQuarterTurnsModFour = parentRotationInQuarterTurns % 4;
            if (parentRotationInQuarterTurnsModFour < 0) parentRotationInQuarterTurnsModFour += 4;

            // 90 & 180: flip current time origin vertically, flip time left origin horizontally
            if (parentRotationInQuarterTurnsModFour == 3 || parentRotationInQuarterTurnsModFour == 2)
            {
                currentTimeOriginX = flipXAnchor(currentTimeOriginX);
                timeLeftOriginY = flipYAnchor(timeLeftOriginY);
            }

            // 270 & 180: flip current time origin horizontally, flip time left origin vertically
            if (parentRotationInQuarterTurnsModFour == 1 || parentRotationInQuarterTurnsModFour == 2)
            {
                currentTimeOriginY = flipYAnchor(currentTimeOriginY);
                timeLeftOriginX = flipXAnchor(timeLeftOriginX);
            }

            // If parent is flipped horizontally: flip origins horizontally
            if (textScale.X == -1)
            {
                currentTimeOriginX = flipXAnchor(currentTimeOriginX);
                timeLeftOriginX = flipXAnchor(timeLeftOriginX);
            }

            // If parent is flipped vertically: flip origins vertically
            if (textScale.Y == -1)
            {
                currentTimeOriginY = flipYAnchor(currentTimeOriginY);
                timeLeftOriginY = flipYAnchor(timeLeftOriginY);
            }

            timeCurrent.Origin = currentTimeOriginX | currentTimeOriginY;
            timeLeft.Origin = timeLeftOriginX | timeLeftOriginY;
        }

        private string formatTime(TimeSpan timeSpan) => $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{Math.Floor(timeSpan.Duration().TotalMinutes)}:{timeSpan.Duration().Seconds:D2}";

        private Anchor flipXAnchor(Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.x0: return Anchor.x2;
                case Anchor.x1: return Anchor.x1;
                case Anchor.x2: return Anchor.x0;

                // The only place this function is called is in this class, so this should never happen, but...
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private Anchor flipYAnchor(Anchor anchor)
        {
            switch (anchor)
            {
                case Anchor.y0: return Anchor.y2;
                case Anchor.y1: return Anchor.y1;
                case Anchor.y2: return Anchor.y0;

                // The only place this function is called is in this class, so this should never happen, but...
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
