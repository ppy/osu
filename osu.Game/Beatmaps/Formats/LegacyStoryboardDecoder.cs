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
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Storyboards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyStoryboardDecoder : LegacyDecoder<Storyboard>
    {
        private StoryboardSprite? storyboardSprite;
        private CommandTimelineGroup? timelineGroup;

        private Storyboard storyboard = null!;

        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        public LegacyStoryboardDecoder(int version = LATEST_VERSION)
            : base(version)
        {
        }

        public static void Register()
        {
            // note that this isn't completely correct
            AddDecoder<Storyboard>(@"osu file format v", m => new LegacyStoryboardDecoder(Parsing.ParseInt(m.Split('v').Last())));
            AddDecoder<Storyboard>(@"[Events]", _ => new LegacyStoryboardDecoder());
            SetFallbackDecoder<Storyboard>(() => new LegacyStoryboardDecoder());
        }

        protected override void ParseStreamInto(LineBufferedReader stream, Storyboard storyboard)
        {
            this.storyboard = storyboard;
            base.ParseStreamInto(stream, storyboard);
        }

        protected override void ParseLine(Storyboard storyboard, Section section, string line)
        {
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

        private const char comma = ',';

        private void handleEvents(string line)
        {
            decodeVariables(ref line);

            int depth = 0;

            foreach (char c in line)
            {
                if (c == ' ' || c == '_')
                    depth++;
                else
                    break;
            }

            var span = line.AsSpan(depth);

            Span<Range> ranges = stackalloc Range[span.GetSplitCount(comma)];
            span.Split(ranges, comma);

            if (depth == 0)
            {
                storyboardSprite = null;

                if (!Enum.TryParse(span.Slice(ranges[0]), out LegacyEventType type))
                    throw new InvalidDataException($@"Unknown event type: {span.Slice(ranges[0])}");

                switch (type)
                {
                    case LegacyEventType.Video:
                    {
                        int offset = Parsing.ParseInt(span.Slice(ranges[1]));
                        string path = CleanFilename(span.Slice(ranges[2]).ToString());

                        // See handling in LegacyBeatmapDecoder for the special case where a video type is used but
                        // the file extension is not a valid video.
                        //
                        // This avoids potential weird crashes when ffmpeg attempts to parse an image file as a video
                        // (see https://github.com/ppy/osu/issues/22829#issuecomment-1465552451).
                        if (!OsuGameBase.VIDEO_EXTENSIONS.Contains(Path.GetExtension(path).ToLowerInvariant()))
                            break;

                        storyboard.GetLayer("Video").Add(new StoryboardVideo(path, offset));
                        break;
                    }

                    case LegacyEventType.Sprite:
                    {
                        string layer = parseLayer(span.Slice(ranges[1]));
                        var origin = parseOrigin(span.Slice(ranges[2]));
                        string path = CleanFilename(span.Slice(ranges[3]).ToString());
                        float x = Parsing.ParseFloat(span.Slice(ranges[4]), Parsing.MAX_COORDINATE_VALUE);
                        float y = Parsing.ParseFloat(span.Slice(ranges[5]), Parsing.MAX_COORDINATE_VALUE);
                        storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Animation:
                    {
                        string layer = parseLayer(span.Slice(ranges[1]));
                        var origin = parseOrigin(span.Slice(ranges[2]));
                        string path = CleanFilename(span.Slice(ranges[3]).ToString());
                        float x = Parsing.ParseFloat(span.Slice(ranges[4]), Parsing.MAX_COORDINATE_VALUE);
                        float y = Parsing.ParseFloat(span.Slice(ranges[5]), Parsing.MAX_COORDINATE_VALUE);
                        int frameCount = Parsing.ParseInt(span.Slice(ranges[6]));
                        double frameDelay = Parsing.ParseDouble(span.Slice(ranges[7]));

                        if (FormatVersion < 6)
                            // this is random as hell but taken straight from osu-stable.
                            frameDelay = Math.Round(0.015 * frameDelay) * 1.186 * (1000 / 60f);

                        var loopType = ranges.Length > 8 ? parseAnimationLoopType(span.Slice(ranges[8])) : AnimationLoopType.LoopForever;
                        storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Sample:
                    {
                        double time = Parsing.ParseDouble(span.Slice(ranges[1]));
                        string layer = parseLayer(span.Slice(ranges[2]));
                        string path = CleanFilename(span.Slice(ranges[3]).ToString());
                        float volume = ranges.Length > 4 ? Parsing.ParseFloat(span.Slice(ranges[4])) : 100;
                        storyboard.GetLayer(layer).Add(new StoryboardSampleInfo(path, time, (int)volume));
                        break;
                    }
                }
            }
            else
            {
                if (depth < 2)
                    timelineGroup = storyboardSprite?.TimelineGroup;

                var commandType = span.Slice(ranges[0]);

                if (commandType.SequenceEqual("T"))
                {
                    double startTime = ranges.Length > 2 ? Parsing.ParseDouble(span.Slice(ranges[2])) : double.MinValue;
                    double endTime = ranges.Length > 3 ? Parsing.ParseDouble(span.Slice(ranges[3])) : double.MaxValue;
                    int groupNumber = ranges.Length > 4 ? Parsing.ParseInt(span.Slice(ranges[4])) : 0;
                    timelineGroup = storyboardSprite?.AddTrigger(span.Slice(ranges[1]).ToString(), startTime, endTime, groupNumber);
                }
                else if (commandType.SequenceEqual("L"))
                {
                    double startTime = Parsing.ParseDouble(span.Slice(ranges[1]));
                    int repeatCount = Parsing.ParseInt(span.Slice(ranges[2]));
                    timelineGroup = storyboardSprite?.AddLoop(startTime, Math.Max(0, repeatCount - 1));
                }
                else
                {
                    if (ranges[3].Length() == 0)
                        ranges[3] = ranges[2];

                    var easing = (Easing)Parsing.ParseInt(span.Slice(ranges[1]));
                    double startTime = Parsing.ParseDouble(span.Slice(ranges[2]));
                    double endTime = Parsing.ParseDouble(span.Slice(ranges[3]));

                    if (commandType.SequenceEqual("F"))
                    {
                        float startValue = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float endValue = ranges.Length > 5 ? Parsing.ParseFloat(span.Slice(ranges[5])) : startValue;
                        timelineGroup?.Alpha.Add(easing, startTime, endTime, startValue, endValue);
                    }
                    else if (commandType.SequenceEqual("S"))
                    {
                        float startValue = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float endValue = ranges.Length > 5 ? Parsing.ParseFloat(span.Slice(ranges[5])) : startValue;
                        timelineGroup?.Scale.Add(easing, startTime, endTime, startValue, endValue);
                    }
                    else if (commandType.SequenceEqual("V"))
                    {
                        float startX = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float startY = Parsing.ParseFloat(span.Slice(ranges[5]));
                        float endX = ranges.Length > 6 ? Parsing.ParseFloat(span.Slice(ranges[6])) : startX;
                        float endY = ranges.Length > 7 ? Parsing.ParseFloat(span.Slice(ranges[7])) : startY;
                        timelineGroup?.VectorScale.Add(easing, startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY));
                    }
                    else if (commandType.SequenceEqual("R"))
                    {
                        float startValue = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float endValue = ranges.Length > 5 ? Parsing.ParseFloat(span.Slice(ranges[5])) : startValue;
                        timelineGroup?.Rotation.Add(easing, startTime, endTime, MathUtils.RadiansToDegrees(startValue), MathUtils.RadiansToDegrees(endValue));
                    }
                    else if (commandType.SequenceEqual("M"))
                    {
                        float startX = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float startY = Parsing.ParseFloat(span.Slice(ranges[5]));
                        float endX = ranges.Length > 6 ? Parsing.ParseFloat(span.Slice(ranges[6])) : startX;
                        float endY = ranges.Length > 7 ? Parsing.ParseFloat(span.Slice(ranges[7])) : startY;
                        timelineGroup?.X.Add(easing, startTime, endTime, startX, endX);
                        timelineGroup?.Y.Add(easing, startTime, endTime, startY, endY);
                    }
                    else if (commandType.SequenceEqual("MX"))
                    {
                        float startValue = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float endValue = ranges.Length > 5 ? Parsing.ParseFloat(span.Slice(ranges[5])) : startValue;
                        timelineGroup?.X.Add(easing, startTime, endTime, startValue, endValue);
                    }
                    else if (commandType.SequenceEqual("MY"))
                    {
                        float startValue = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float endValue = ranges.Length > 5 ? Parsing.ParseFloat(span.Slice(ranges[5])) : startValue;
                        timelineGroup?.Y.Add(easing, startTime, endTime, startValue, endValue);
                    }
                    else if (commandType.SequenceEqual("C"))
                    {
                        float startRed = Parsing.ParseFloat(span.Slice(ranges[4]));
                        float startGreen = Parsing.ParseFloat(span.Slice(ranges[5]));
                        float startBlue = Parsing.ParseFloat(span.Slice(ranges[6]));
                        float endRed = ranges.Length > 7 ? Parsing.ParseFloat(span.Slice(ranges[7])) : startRed;
                        float endGreen = ranges.Length > 8 ? Parsing.ParseFloat(span.Slice(ranges[8])) : startGreen;
                        float endBlue = ranges.Length > 9 ? Parsing.ParseFloat(span.Slice(ranges[9])) : startBlue;
                        timelineGroup?.Colour.Add(easing, startTime, endTime,
                            new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                            new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1));
                    }
                    else if (commandType.SequenceEqual("P"))
                    {
                        var type = span.Slice(ranges[4]);

                        if (type.SequenceEqual(@"A"))
                        {
                            timelineGroup?.BlendingParameters.Add(easing, startTime, endTime, BlendingParameters.Additive,
                                startTime == endTime ? BlendingParameters.Additive : BlendingParameters.Inherit);
                        }
                        else if (type.SequenceEqual(@"H"))
                        {
                            timelineGroup?.FlipH.Add(easing, startTime, endTime, true, startTime == endTime);
                        }
                        else if (type.SequenceEqual(@"V"))
                        {
                            timelineGroup?.FlipV.Add(easing, startTime, endTime, true, startTime == endTime);
                        }
                    }
                    else
                    {
                        throw new InvalidDataException($@"Unknown command type: {commandType}");
                    }
                }
            }
        }

        private string parseLayer(ReadOnlySpan<char> value) => Enum.Parse<LegacyStoryLayer>(value).ToString();

        private Anchor parseOrigin(ReadOnlySpan<char> value)
        {
            var origin = Enum.Parse<LegacyOrigins>(value);

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

        private AnimationLoopType parseAnimationLoopType(ReadOnlySpan<char> value)
        {
            var parsed = Enum.Parse<AnimationLoopType>(value);
            return Enum.IsDefined(parsed) ? parsed : AnimationLoopType.LoopForever;
        }

        private void handleVariables(string line)
        {
            var pair = SplitKeyVal(line, '=', false);
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
