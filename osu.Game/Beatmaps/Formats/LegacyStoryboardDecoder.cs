// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Extensions;
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
            AddDecoder<Storyboard>(@"[Events]", _ => new LegacyStoryboardDecoder());
            SetFallbackDecoder<Storyboard>(() => new LegacyStoryboardDecoder());
        }

        protected override void ParseStreamInto(LineBufferedReader stream, Storyboard storyboard)
        {
            this.storyboard = storyboard;
            base.ParseStreamInto(stream, storyboard);
        }

        protected override void ParseLine(Storyboard storyboard, Section section, ReadOnlySpan<char> line)
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

        private void handleGeneral(Storyboard storyboard, ReadOnlySpan<char> line)
        {
            var pair = SplitKeyVal(line);

            switch (pair.Key)
            {
                case "UseSkinSprites":
                    storyboard.UseSkinSprites = pair.Value.SequenceEqual("1");
                    break;
            }
        }

        private void handleEvents(ReadOnlySpan<char> line)
        {
            decodeVariables(ref line);

            int depth = 0;

            // TODO: Use IndexOfAnyExcept
            foreach (char c in line)
            {
                if (c == ' ' || c == '_')
                    depth++;
                else
                    break;
            }

            line = line[depth..];

            var split = line.Split(',');

            if (depth == 0)
            {
                storyboardSprite = null;

                split.MoveNext(); // split[0]
                if (!Enum.TryParse(split.Current, out LegacyEventType type))
                    throw new InvalidDataException($@"Unknown event type: {split.Current}");

                switch (type)
                {
                    case LegacyEventType.Video:
                    {
                        split.MoveNext(); // split[1];
                        int offset = Parsing.ParseInt(split.Current);
                        split.MoveNext(); // split[2];
                        string path = CleanFilename(split.Current);

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
                        split.MoveNext(); // split[1]
                        string layer = parseLayer(split.Current);
                        split.MoveNext(); // split[2]
                        var origin = parseOrigin(split.Current);
                        split.MoveNext(); // split[3]
                        string path = CleanFilename(split.Current);
                        split.MoveNext(); // split[4]
                        float x = Parsing.ParseFloat(split.Current, Parsing.MAX_COORDINATE_VALUE);
                        split.MoveNext(); // split[5]
                        float y = Parsing.ParseFloat(split.Current, Parsing.MAX_COORDINATE_VALUE);
                        storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Animation:
                    {
                        split.MoveNext(); // split[1]
                        string layer = parseLayer(split.Current);
                        split.MoveNext(); // split[2]
                        var origin = parseOrigin(split.Current);
                        split.MoveNext(); // split[3]
                        string path = CleanFilename(split.Current);
                        split.MoveNext(); // split[4]
                        float x = Parsing.ParseFloat(split.Current, Parsing.MAX_COORDINATE_VALUE);
                        split.MoveNext(); // split[5]
                        float y = Parsing.ParseFloat(split.Current, Parsing.MAX_COORDINATE_VALUE);
                        split.MoveNext(); // split[6]
                        int frameCount = Parsing.ParseInt(split.Current);
                        split.MoveNext(); // split[7]
                        double frameDelay = Parsing.ParseDouble(split.Current);

                        if (FormatVersion < 6)
                            // this is random as hell but taken straight from osu-stable.
                            frameDelay = Math.Round(0.015 * frameDelay) * 1.186 * (1000 / 60f);

                        var loopType = split.MoveNext() ? parseAnimationLoopType(split.Current) : AnimationLoopType.LoopForever; // split[8]
                        storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Sample:
                    {
                        split.MoveNext(); // split[1]
                        double time = Parsing.ParseDouble(split.Current);
                        split.MoveNext(); // split[2]
                        string layer = parseLayer(split.Current);
                        split.MoveNext(); // split[3]
                        string path = CleanFilename(split.Current);
                        float volume = split.MoveNext() ? Parsing.ParseFloat(split.Current) : 100; // split[4]
                        storyboard.GetLayer(layer).Add(new StoryboardSampleInfo(path, time, (int)volume));
                        break;
                    }
                }
            }
            else
            {
                if (depth < 2)
                    timelineGroup = storyboardSprite?.TimelineGroup;

                split.MoveNext(); // split[0]
                ReadOnlySpan<char> commandType = split.Current;

                switch (commandType)
                {
                    case "T":
                    {
                        split.MoveNext(); // split[1]
                        string triggerName = split.Current.ToString();
                        double startTime = split.MoveNext() ? Parsing.ParseDouble(split.Current) : double.MinValue; // split[2]
                        double endTime = split.MoveNext() ? Parsing.ParseDouble(split.Current) : double.MaxValue; // split[3]
                        int groupNumber = split.MoveNext() ? Parsing.ParseInt(split.Current) : 0; // split[4]
                        timelineGroup = storyboardSprite?.AddTrigger(triggerName, startTime, endTime, groupNumber);
                        break;
                    }

                    case "L":
                    {
                        split.MoveNext(); // split[1]
                        double startTime = Parsing.ParseDouble(split.Current);
                        split.MoveNext(); // split[2]
                        int repeatCount = Parsing.ParseInt(split.Current);
                        timelineGroup = storyboardSprite?.AddLoop(startTime, Math.Max(0, repeatCount - 1));
                        break;
                    }

                    default:
                    {
                        split.MoveNext(); // split[1]
                        var easing = (Easing)Parsing.ParseInt(split.Current);
                        split.MoveNext(); // split[2]
                        double startTime = Parsing.ParseDouble(split.Current);
                        split.MoveNext(); // split[3]
                        double endTime = split.Current.IsEmpty ? startTime : Parsing.ParseDouble(split.Current);

                        switch (commandType)
                        {
                            case "F":
                            {
                                split.MoveNext(); // split[4]
                                float startValue = Parsing.ParseFloat(split.Current);
                                float endValue = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startValue; // split[5]
                                timelineGroup?.Alpha.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "S":
                            {
                                split.MoveNext(); // split[4]
                                float startValue = Parsing.ParseFloat(split.Current);
                                float endValue = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startValue; // split[5]
                                timelineGroup?.Scale.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "V":
                            {
                                split.MoveNext(); // split[4]
                                float startX = Parsing.ParseFloat(split.Current);
                                split.MoveNext(); // split[5]
                                float startY = Parsing.ParseFloat(split.Current);
                                float endX = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startX; // split[6]
                                float endY = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startY; // split[7]
                                timelineGroup?.VectorScale.Add(easing, startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY));
                                break;
                            }

                            case "R":
                            {
                                split.MoveNext(); // split[4]
                                float startValue = Parsing.ParseFloat(split.Current);
                                float endValue = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startValue; // split[5]
                                timelineGroup?.Rotation.Add(easing, startTime, endTime, MathUtils.RadiansToDegrees(startValue), MathUtils.RadiansToDegrees(endValue));
                                break;
                            }

                            case "M":
                            {
                                split.MoveNext(); // split[4]
                                float startX = Parsing.ParseFloat(split.Current);
                                split.MoveNext(); // split[5]
                                float startY = Parsing.ParseFloat(split.Current);
                                float endX = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startX; // split[6]
                                float endY = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startY; // split[7]
                                timelineGroup?.X.Add(easing, startTime, endTime, startX, endX);
                                timelineGroup?.Y.Add(easing, startTime, endTime, startY, endY);
                                break;
                            }

                            case "MX":
                            {
                                split.MoveNext(); // split[4]
                                float startValue = Parsing.ParseFloat(split.Current);
                                float endValue = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startValue; // split[5]
                                timelineGroup?.X.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "MY":
                            {
                                split.MoveNext(); // split[4]
                                float startValue = Parsing.ParseFloat(split.Current);
                                float endValue = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startValue; // split[5]
                                timelineGroup?.Y.Add(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "C":
                            {
                                split.MoveNext(); // split[4]
                                float startRed = Parsing.ParseFloat(split.Current);
                                split.MoveNext(); // split[5]
                                float startGreen = Parsing.ParseFloat(split.Current);
                                split.MoveNext(); // split[6]
                                float startBlue = Parsing.ParseFloat(split.Current);
                                float endRed = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startRed; // split[7]
                                float endGreen = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startGreen; // split[8]
                                float endBlue = split.MoveNext() ? Parsing.ParseFloat(split.Current) : startBlue; // split[9]
                                timelineGroup?.Colour.Add(easing, startTime, endTime,
                                    new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                                    new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1));
                                break;
                            }

                            case "P":
                            {
                                split.MoveNext(); // split[4]
                                switch (split.Current)
                                {
                                    case "A":
                                        timelineGroup?.BlendingParameters.Add(easing, startTime, endTime, BlendingParameters.Additive,
                                            startTime == endTime ? BlendingParameters.Additive : BlendingParameters.Inherit);
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

        private void handleVariables(ReadOnlySpan<char> line)
        {
            var pair = SplitKeyVal(line, '=', false);
            variables[pair.Key.ToString()] = pair.Value.ToString();
        }

        /// <summary>
        /// Decodes any beatmap variables present in a line into their real values.
        /// </summary>
        /// <param name="line">The line which may contains variables.</param>
        private void decodeVariables(ref ReadOnlySpan<char> line)
        {
            while (line.Contains('$'))
            {
                ReadOnlySpan<char> origLine = line;

                foreach (var v in variables)
                    line = line.ToString().Replace(v.Key, v.Value);

                if (line.SequenceEqual(origLine))
                    break;
            }
        }
    }
}
