﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.IO.File;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyStoryboardDecoder : LegacyDecoder<Storyboard>
    {
        private StoryboardSprite storyboardSprite;
        private CommandTimelineGroup timelineGroup;

        private Storyboard storyboard;

        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        public LegacyStoryboardDecoder()
            : base(0)
        {
        }

        public static void Register()
        {
            // note that this isn't completely correct
            AddDecoder<Storyboard>(@"osu file format v", m => new LegacyStoryboardDecoder());
            AddDecoder<Storyboard>(@"[Events]", m => new LegacyStoryboardDecoder());
        }

        protected override void ParseStreamInto(StreamReader stream, Storyboard storyboard)
        {
            this.storyboard = storyboard;
            base.ParseStreamInto(stream, storyboard);
        }

        protected override void ParseLine(Storyboard storyboard, Section section, string line)
        {
            switch (section)
            {
                case Section.Events:
                    handleEvents(line);
                    return;
                case Section.Variables:
                    handleVariables(line);
                    return;
            }

            base.ParseLine(storyboard, section, line);
        }

        private void handleEvents(string line)
        {
            var depth = 0;
            while (line.StartsWith(" ") || line.StartsWith("_"))
            {
                ++depth;
                line = line.Substring(1);
            }

            decodeVariables(ref line);

            string[] split = line.Split(',');

            if (depth == 0)
            {
                storyboardSprite = null;

                EventType type;
                if (!Enum.TryParse(split[0], out type))
                    throw new InvalidDataException($@"Unknown event type {split[0]}");

                switch (type)
                {
                    case EventType.Sprite:
                    {
                        var layer = parseLayer(split[1]);
                        var origin = parseOrigin(split[2]);
                        var path = cleanFilename(split[3]);
                        var x = float.Parse(split[4], NumberFormatInfo.InvariantInfo);
                        var y = float.Parse(split[5], NumberFormatInfo.InvariantInfo);
                        storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                    }
                        break;
                    case EventType.Animation:
                    {
                        var layer = parseLayer(split[1]);
                        var origin = parseOrigin(split[2]);
                        var path = cleanFilename(split[3]);
                        var x = float.Parse(split[4], NumberFormatInfo.InvariantInfo);
                        var y = float.Parse(split[5], NumberFormatInfo.InvariantInfo);
                        var frameCount = int.Parse(split[6]);
                        var frameDelay = double.Parse(split[7], NumberFormatInfo.InvariantInfo);
                        var loopType = split.Length > 8 ? (AnimationLoopType)Enum.Parse(typeof(AnimationLoopType), split[8]) : AnimationLoopType.LoopForever;
                        storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                    }
                        break;
                    case EventType.Sample:
                    {
                        var time = double.Parse(split[1], CultureInfo.InvariantCulture);
                        var layer = parseLayer(split[2]);
                        var path = cleanFilename(split[3]);
                        var volume = split.Length > 4 ? float.Parse(split[4], CultureInfo.InvariantCulture) : 100;
                        storyboard.GetLayer(layer).Add(new StoryboardSample(path, time, volume));
                    }
                        break;
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
                        var startTime = split.Length > 2 ? double.Parse(split[2], CultureInfo.InvariantCulture) : double.MinValue;
                        var endTime = split.Length > 3 ? double.Parse(split[3], CultureInfo.InvariantCulture) : double.MaxValue;
                        var groupNumber = split.Length > 4 ? int.Parse(split[4]) : 0;
                        timelineGroup = storyboardSprite?.AddTrigger(triggerName, startTime, endTime, groupNumber);
                    }
                        break;
                    case "L":
                    {
                        var startTime = double.Parse(split[1], CultureInfo.InvariantCulture);
                        var loopCount = int.Parse(split[2]);
                        timelineGroup = storyboardSprite?.AddLoop(startTime, loopCount);
                    }
                        break;
                    default:
                    {
                        if (string.IsNullOrEmpty(split[3]))
                            split[3] = split[2];

                        var easing = (Easing)int.Parse(split[1]);
                        var startTime = double.Parse(split[2], CultureInfo.InvariantCulture);
                        var endTime = double.Parse(split[3], CultureInfo.InvariantCulture);

                        switch (commandType)
                        {
                            case "F":
                            {
                                var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Alpha.Add(easing, startTime, endTime, startValue, endValue);
                            }
                                break;
                            case "S":
                            {
                                var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Scale.Add(easing, startTime, endTime, new Vector2(startValue), new Vector2(endValue));
                            }
                                break;
                            case "V":
                            {
                                var startX = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var startY = float.Parse(split[5], CultureInfo.InvariantCulture);
                                var endX = split.Length > 6 ? float.Parse(split[6], CultureInfo.InvariantCulture) : startX;
                                var endY = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startY;
                                timelineGroup?.Scale.Add(easing, startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY));
                            }
                                break;
                            case "R":
                            {
                                var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Rotation.Add(easing, startTime, endTime, MathHelper.RadiansToDegrees(startValue), MathHelper.RadiansToDegrees(endValue));
                            }
                                break;
                            case "M":
                            {
                                var startX = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var startY = float.Parse(split[5], CultureInfo.InvariantCulture);
                                var endX = split.Length > 6 ? float.Parse(split[6], CultureInfo.InvariantCulture) : startX;
                                var endY = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startY;
                                timelineGroup?.X.Add(easing, startTime, endTime, startX, endX);
                                timelineGroup?.Y.Add(easing, startTime, endTime, startY, endY);
                            }
                                break;
                            case "MX":
                            {
                                var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.X.Add(easing, startTime, endTime, startValue, endValue);
                            }
                                break;
                            case "MY":
                            {
                                var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Y.Add(easing, startTime, endTime, startValue, endValue);
                            }
                                break;
                            case "C":
                            {
                                var startRed = float.Parse(split[4], CultureInfo.InvariantCulture);
                                var startGreen = float.Parse(split[5], CultureInfo.InvariantCulture);
                                var startBlue = float.Parse(split[6], CultureInfo.InvariantCulture);
                                var endRed = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startRed;
                                var endGreen = split.Length > 8 ? float.Parse(split[8], CultureInfo.InvariantCulture) : startGreen;
                                var endBlue = split.Length > 9 ? float.Parse(split[9], CultureInfo.InvariantCulture) : startBlue;
                                timelineGroup?.Colour.Add(easing, startTime, endTime,
                                    new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                                    new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1));
                            }
                                break;
                            case "P":
                            {
                                var type = split[4];
                                switch (type)
                                {
                                    case "A":
                                        timelineGroup?.BlendingMode.Add(easing, startTime, endTime, BlendingMode.Additive, startTime == endTime ? BlendingMode.Additive : BlendingMode.Inherit);
                                        break;
                                    case "H":
                                        timelineGroup?.FlipH.Add(easing, startTime, endTime, true, startTime == endTime);
                                        break;
                                    case "V":
                                        timelineGroup?.FlipV.Add(easing, startTime, endTime, true, startTime == endTime);
                                        break;
                                }
                            }
                                break;
                            default:
                                throw new InvalidDataException($@"Unknown command type: {commandType}");
                        }
                    }
                        break;
                }
            }
        }

        private string parseLayer(string value) => Enum.Parse(typeof(StoryLayer), value).ToString();

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
            }

            throw new InvalidDataException($@"Unknown origin: {value}");
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
            while (line.IndexOf('$') >= 0)
            {
                string origLine = line;
                string[] split = line.Split(',');
                for (int i = 0; i < split.Length; i++)
                {
                    var item = split[i];
                    if (item.StartsWith("$") && variables.ContainsKey(item))
                        split[i] = variables[item];
                }

                line = string.Join(",", split);
                if (line == origLine)
                    break;
            }
        }

        private string cleanFilename(string path) => FileSafety.PathStandardise(FileSafety.PathSanitise(path.Trim('\"')));
    }
}
