// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using osuTK;
using osuTK.Graphics;
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
            line = StripComments(line);

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
            int depth = 0;
            while (line.StartsWith(" ", StringComparison.Ordinal) || line.StartsWith("_", StringComparison.Ordinal))
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
                        string layer = parseLayer(split[1]);
                        Anchor origin = parseOrigin(split[2]);
                        string path = cleanFilename(split[3]);
                        float x = float.Parse(split[4], NumberFormatInfo.InvariantInfo);
                        float y = float.Parse(split[5], NumberFormatInfo.InvariantInfo);
                        storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                    }
                        break;
                    case EventType.Animation:
                    {
                        string layer = parseLayer(split[1]);
                        Anchor origin = parseOrigin(split[2]);
                        string path = cleanFilename(split[3]);
                        float x = float.Parse(split[4], NumberFormatInfo.InvariantInfo);
                        float y = float.Parse(split[5], NumberFormatInfo.InvariantInfo);
                        int frameCount = int.Parse(split[6]);
                        double frameDelay = double.Parse(split[7], NumberFormatInfo.InvariantInfo);
                        AnimationLoopType loopType = split.Length > 8 ? (AnimationLoopType)Enum.Parse(typeof(AnimationLoopType), split[8]) : AnimationLoopType.LoopForever;
                        storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                    }
                        break;
                    case EventType.Sample:
                    {
                        double time = double.Parse(split[1], CultureInfo.InvariantCulture);
                        string layer = parseLayer(split[2]);
                        string path = cleanFilename(split[3]);
                        float volume = split.Length > 4 ? float.Parse(split[4], CultureInfo.InvariantCulture) : 100;
                        storyboard.GetLayer(layer).Add(new StoryboardSample(path, time, volume));
                    }
                        break;
                }
            }
            else
            {
                if (depth < 2)
                    timelineGroup = storyboardSprite?.TimelineGroup;

                string commandType = split[0];
                switch (commandType)
                {
                    case "T":
                    {
                        string triggerName = split[1];
                        double startTime = split.Length > 2 ? double.Parse(split[2], CultureInfo.InvariantCulture) : double.MinValue;
                        double endTime = split.Length > 3 ? double.Parse(split[3], CultureInfo.InvariantCulture) : double.MaxValue;
                        int groupNumber = split.Length > 4 ? int.Parse(split[4]) : 0;
                        timelineGroup = storyboardSprite?.AddTrigger(triggerName, startTime, endTime, groupNumber);
                    }
                        break;
                    case "L":
                    {
                        double startTime = double.Parse(split[1], CultureInfo.InvariantCulture);
                        int loopCount = int.Parse(split[2]);
                        timelineGroup = storyboardSprite?.AddLoop(startTime, loopCount);
                    }
                        break;
                    default:
                    {
                        if (string.IsNullOrEmpty(split[3]))
                            split[3] = split[2];

                        var easing = (Easing)int.Parse(split[1]);
                        double startTime = double.Parse(split[2], CultureInfo.InvariantCulture);
                        double endTime = double.Parse(split[3], CultureInfo.InvariantCulture);

                        switch (commandType)
                        {
                            case "F":
                            {
                                float startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Alpha.Add(easing, startTime, endTime, startValue, endValue);
                            }
                                break;
                            case "S":
                            {
                                float startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Scale.Add(easing, startTime, endTime, new Vector2(startValue), new Vector2(endValue));
                            }
                                break;
                            case "V":
                            {
                                float startX = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float startY = float.Parse(split[5], CultureInfo.InvariantCulture);
                                float endX = split.Length > 6 ? float.Parse(split[6], CultureInfo.InvariantCulture) : startX;
                                float endY = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startY;
                                timelineGroup?.Scale.Add(easing, startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY));
                            }
                                break;
                            case "R":
                            {
                                float startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Rotation.Add(easing, startTime, endTime, MathHelper.RadiansToDegrees(startValue), MathHelper.RadiansToDegrees(endValue));
                            }
                                break;
                            case "M":
                            {
                                float startX = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float startY = float.Parse(split[5], CultureInfo.InvariantCulture);
                                float endX = split.Length > 6 ? float.Parse(split[6], CultureInfo.InvariantCulture) : startX;
                                float endY = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startY;
                                timelineGroup?.X.Add(easing, startTime, endTime, startX, endX);
                                timelineGroup?.Y.Add(easing, startTime, endTime, startY, endY);
                            }
                                break;
                            case "MX":
                            {
                                float startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.X.Add(easing, startTime, endTime, startValue, endValue);
                            }
                                break;
                            case "MY":
                            {
                                float startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                timelineGroup?.Y.Add(easing, startTime, endTime, startValue, endValue);
                            }
                                break;
                            case "C":
                            {
                                float startRed = float.Parse(split[4], CultureInfo.InvariantCulture);
                                float startGreen = float.Parse(split[5], CultureInfo.InvariantCulture);
                                float startBlue = float.Parse(split[6], CultureInfo.InvariantCulture);
                                float endRed = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startRed;
                                float endGreen = split.Length > 8 ? float.Parse(split[8], CultureInfo.InvariantCulture) : startGreen;
                                float endBlue = split.Length > 9 ? float.Parse(split[9], CultureInfo.InvariantCulture) : startBlue;
                                timelineGroup?.Colour.Add(easing, startTime, endTime,
                                    new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                                    new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1));
                            }
                                break;
                            case "P":
                            {
                                string type = split[4];
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
                default:
                    return Anchor.TopLeft;
            }
        }

        private void handleVariables(string line)
        {
            KeyValuePair<string, string> pair = SplitKeyVal(line, '=');
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

                foreach (KeyValuePair<string, string> v in variables)
                    line = line.Replace(v.Key, v.Value);

                if (line == origLine)
                    break;
            }
        }

        private string cleanFilename(string path) => FileSafety.PathStandardise(path.Trim('"'));
    }
}
