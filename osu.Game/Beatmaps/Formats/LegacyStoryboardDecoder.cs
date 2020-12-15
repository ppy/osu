// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Legacy;
using osu.Game.IO;
using osu.Game.Storyboards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyStoryboardDecoder : LegacyDecoder<Storyboard>
    {
        private StoryboardSprite storyboardSprite;
        private CommandTimelineGroup timelineGroup;

        private Storyboard storyboard;

        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        public LegacyStoryboardDecoder(int version = LATEST_VERSION)
            : base(version)
        {
        }

        public static void Register()
        {
            // note that this isn't completely correct
            AddDecoder<Storyboard>(@"osu file format v", m => new LegacyStoryboardDecoder(Parsing.ParseInt(m.Split('v').Last())));
            AddDecoder<Storyboard>(@"[Events]", m => new LegacyStoryboardDecoder());
            SetFallbackDecoder<Storyboard>(() => new LegacyStoryboardDecoder());
        }

        protected override void ParseStreamInto(LineBufferedReader stream, Storyboard storyboard)
        {
            this.storyboard = storyboard;
            base.ParseStreamInto(stream, storyboard);
        }

        protected override void ParseLine(Storyboard storyboard, Section section, string line)
        {
            line = StripComments(line);

            switch (section)
            {
                case Section.General:
                    handleGeneral(storyboard, line);
                    return;

                case Section.Events:
                    handleEvents(line);
                    return;

                case Section.Variables:
                    handleVariables(line);
                    return;
            }

            base.ParseLine(storyboard, section, line);
        }

        private void handleGeneral(Storyboard storyboard, string line)
        {
            var pair = SplitKeyVal(line);

            switch (pair.Key)
            {
                case "UseSkinSprites":
                    storyboard.UseSkinSprites = pair.Value == "1";
                    break;
            }
        }

        private void handleEvents(string line)
        {
            var depth = 0;

            foreach (char c in line)
            {
                if (c == ' ' || c == '_')
                    depth++;
                else
                    break;
            }

            line = line.Substring(depth);

            decodeVariables(ref line);

            string[] split = line.Split(',');

            if (depth == 0)
            {
                storyboardSprite = null;

                if (!Enum.TryParse(split[0], out LegacyEventType type))
                    throw new InvalidDataException($@"Unknown event type: {split[0]}");

                switch (type)
                {
                    case LegacyEventType.Video:
                    {
                        var offset = Parsing.ParseInt(split[1]);
                        var path = CleanFilename(split[2]);

                        storyboard.GetLayer("Video").Add(new StoryboardVideo(path, offset));
                        break;
                    }

                    case LegacyEventType.Sprite:
                    {
                        var layer = parseLayer(split[1]);
                        var origin = parseOrigin(split[2]);
                        var path = CleanFilename(split[3]);
                        var x = Parsing.ParseFloat(split[4], Parsing.MAX_COORDINATE_VALUE);
                        var y = Parsing.ParseFloat(split[5], Parsing.MAX_COORDINATE_VALUE);
                        storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Animation:
                    {
                        var layer = parseLayer(split[1]);
                        var origin = parseOrigin(split[2]);
                        var path = CleanFilename(split[3]);
                        var x = Parsing.ParseFloat(split[4], Parsing.MAX_COORDINATE_VALUE);
                        var y = Parsing.ParseFloat(split[5], Parsing.MAX_COORDINATE_VALUE);
                        var frameCount = Parsing.ParseInt(split[6]);
                        var frameDelay = Parsing.ParseDouble(split[7]);

                        if (FormatVersion < 6)
                            // this is random as hell but taken straight from osu-stable.
                            frameDelay = Math.Round(0.015 * frameDelay) * 1.186 * (1000 / 60f);

                        var loopType = split.Length > 8 ? (AnimationLoopType)Enum.Parse(typeof(AnimationLoopType), split[8]) : AnimationLoopType.LoopForever;
                        storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Sample:
                    {
                        var time = Parsing.ParseDouble(split[1]);
                        var layer = parseLayer(split[2]);
                        var path = CleanFilename(split[3]);
                        var volume = split.Length > 4 ? Parsing.ParseFloat(split[4]) : 100;
                        storyboard.GetLayer(layer).Add(new StoryboardSampleInfo(path, time, (int)volume));
                        break;
                    }
                }
            }
            else
            {
                if (depth < 2)
                    timelineGroup = storyboardSprite?.TimelineGroup;

                var commandType = split[0];

                switch (commandType)
                {
                    case "T":
                    {
                        var triggerName = split[1];
                        var startTime = split.Length > 2 ? Parsing.ParseDouble(split[2]) : double.MinValue;
                        var endTime = split.Length > 3 ? Parsing.ParseDouble(split[3]) : double.MaxValue;
                        var groupNumber = split.Length > 4 ? Parsing.ParseInt(split[4]) : 0;
                        timelineGroup = storyboardSprite?.AddTrigger(triggerName, startTime, endTime, groupNumber);
                        break;
                    }

                    case "L":
                    {
                        var startTime = Parsing.ParseDouble(split[1]);
                        var loopCount = Parsing.ParseInt(split[2]);
                        timelineGroup = storyboardSprite?.AddLoop(startTime, loopCount);
                        break;
                    }

                    default:
                    {
                        if (string.IsNullOrEmpty(split[3]))
                            split[3] = split[2];

                        var easing = (Easing)Parsing.ParseInt(split[1]);
                        var startTime = Parsing.ParseDouble(split[2]);
                        var endTime = Parsing.ParseDouble(split[3]);

                        switch (commandType)
                        {
                            case "F":
                            {
                                var startValue = Parsing.ParseFloat(split[4]);
                                var endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                timelineGroup?.Alpha.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "S":
                            {
                                var startValue = Parsing.ParseFloat(split[4]);
                                var endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                timelineGroup?.Scale.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "V":
                            {
                                var startX = Parsing.ParseFloat(split[4]);
                                var startY = Parsing.ParseFloat(split[5]);
                                var endX = split.Length > 6 ? Parsing.ParseFloat(split[6]) : startX;
                                var endY = split.Length > 7 ? Parsing.ParseFloat(split[7]) : startY;
                                timelineGroup?.VectorScale.Add(easing, startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY));
                                break;
                            }

                            case "R":
                            {
                                var startValue = Parsing.ParseFloat(split[4]);
                                var endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                timelineGroup?.Rotation.Add(easing, startTime, endTime, MathUtils.RadiansToDegrees(startValue), MathUtils.RadiansToDegrees(endValue));
                                break;
                            }

                            case "M":
                            {
                                var startX = Parsing.ParseFloat(split[4]);
                                var startY = Parsing.ParseFloat(split[5]);
                                var endX = split.Length > 6 ? Parsing.ParseFloat(split[6]) : startX;
                                var endY = split.Length > 7 ? Parsing.ParseFloat(split[7]) : startY;
                                timelineGroup?.X.Add(easing, startTime, endTime, startX, endX);
                                timelineGroup?.Y.Add(easing, startTime, endTime, startY, endY);
                                break;
                            }

                            case "MX":
                            {
                                var startValue = Parsing.ParseFloat(split[4]);
                                var endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                timelineGroup?.X.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "MY":
                            {
                                var startValue = Parsing.ParseFloat(split[4]);
                                var endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                timelineGroup?.Y.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "C":
                            {
                                var startRed = Parsing.ParseFloat(split[4]);
                                var startGreen = Parsing.ParseFloat(split[5]);
                                var startBlue = Parsing.ParseFloat(split[6]);
                                var endRed = split.Length > 7 ? Parsing.ParseFloat(split[7]) : startRed;
                                var endGreen = split.Length > 8 ? Parsing.ParseFloat(split[8]) : startGreen;
                                var endBlue = split.Length > 9 ? Parsing.ParseFloat(split[9]) : startBlue;
                                timelineGroup?.Colour.Add(easing, startTime, endTime,
                                    new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                                    new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1));
                                break;
                            }

                            case "P":
                            {
                                var type = split[4];

                                switch (type)
                                {
                                    case "A":
                                        timelineGroup?.BlendingParameters.Add(easing, startTime, endTime, BlendingParameters.Additive, startTime == endTime ? BlendingParameters.Additive : BlendingParameters.Inherit);
                                        break;

                                    case "H":
                                        timelineGroup?.FlipH.Add(easing, startTime, endTime, true, startTime == endTime);
                                        break;

                                    case "V":
                                        timelineGroup?.FlipV.Add(easing, startTime, endTime, true, startTime == endTime);
                                        break;
                                }

                                break;
                            }

                            default:
                                throw new InvalidDataException($@"Unknown command type: {commandType}");
                        }

                        break;
                    }
                }
            }
        }

        private string parseLayer(string value) => Enum.Parse(typeof(LegacyStoryLayer), value).ToString();

        private Anchor parseOrigin(string value)
        {
            var origin = (LegacyOrigins)Enum.Parse(typeof(LegacyOrigins), value);

            switch (origin)
            {
                case LegacyOrigins.TopLeft:
                    return Anchor.TopLeft;

                case LegacyOrigins.TopCentre:
                    return Anchor.TopCentre;

                case LegacyOrigins.TopRight:
                    return Anchor.TopRight;

                case LegacyOrigins.CentreLeft:
                    return Anchor.CentreLeft;

                case LegacyOrigins.Centre:
                    return Anchor.Centre;

                case LegacyOrigins.CentreRight:
                    return Anchor.CentreRight;

                case LegacyOrigins.BottomLeft:
                    return Anchor.BottomLeft;

                case LegacyOrigins.BottomCentre:
                    return Anchor.BottomCentre;

                case LegacyOrigins.BottomRight:
                    return Anchor.BottomRight;

                default:
                    return Anchor.TopLeft;
            }
        }

        private void handleVariables(string line)
        {
            var pair = SplitKeyVal(line, '=');
            variables[pair.Key] = pair.Value;
        }

        /// <summary>
        /// Decodes any beatmap variables present in a line into their real values.
        /// </summary>
        /// <param name="line">The line which may contains variables.</param>
        private void decodeVariables(ref string line)
        {
            while (line.Contains('$'))
            {
                string origLine = line;

                foreach (var v in variables)
                    line = line.Replace(v.Key, v.Value);

                if (line == origLine)
                    break;
            }
        }
    }
}
