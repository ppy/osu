// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Legacy;
using osu.Game.IO;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Commands;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyStoryboardDecoder : LegacyDecoder<Storyboard>
    {
        private StoryboardSprite? storyboardSprite;
        private StoryboardCommandGroup? currentCommandsGroup;

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
                    storyboard.UseSkinSprites = pair.Value is "1";
                    break;
            }
        }

        private void handleEvents(ReadOnlySpan<char> line)
        {
            line = decodeVariables(line);

            int depth = line.IndexOfAnyExcept(' ', '_');

            line = line.Slice(depth);

            Span<Range> ranges = stackalloc Range[11];
            int splitCount = line.Split(ranges, ',');

            if (depth == 0)
            {
                storyboardSprite = null;

                if (!Enum.TryParse(line[ranges[0]], out LegacyEventType type))
                    throw new InvalidDataException($@"Unknown event type: {line[ranges[0]]}");

                switch (type)
                {
                    case LegacyEventType.Video:
                    {
                        int offset = Parsing.ParseInt(line[ranges[1]]);
                        string path = CleanFilename(line[ranges[2]]);

                        // See handling in LegacyBeatmapDecoder for the special case where a video type is used but
                        // the file extension is not a valid video.
                        //
                        // This avoids potential weird crashes when ffmpeg attempts to parse an image file as a video
                        // (see https://github.com/ppy/osu/issues/22829#issuecomment-1465552451).
                        if (!OsuGameBase.VIDEO_EXTENSIONS.Contains(Path.GetExtension(path).ToLowerInvariant()))
                            break;

                        storyboard.GetLayer("Video").Add(storyboardSprite = new StoryboardVideo(path, offset));
                        break;
                    }

                    case LegacyEventType.Sprite:
                    {
                        string layer = parseLayer(line[ranges[1]]);
                        var origin = parseOrigin(line[ranges[2]]);
                        string path = CleanFilename(line[ranges[3]]);
                        float x = Parsing.ParseFloat(line[ranges[4]], Parsing.MAX_COORDINATE_VALUE);
                        float y = Parsing.ParseFloat(line[ranges[5]], Parsing.MAX_COORDINATE_VALUE);
                        storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Animation:
                    {
                        string layer = parseLayer(line[ranges[1]]);
                        var origin = parseOrigin(line[ranges[2]]);
                        string path = CleanFilename(line[ranges[3]]);
                        float x = Parsing.ParseFloat(line[ranges[4]], Parsing.MAX_COORDINATE_VALUE);
                        float y = Parsing.ParseFloat(line[ranges[5]], Parsing.MAX_COORDINATE_VALUE);
                        int frameCount = Parsing.ParseInt(line[ranges[6]]);
                        double frameDelay = Parsing.ParseDouble(line[ranges[7]]);

                        if (FormatVersion < 6)
                            // this is random as hell but taken straight from osu-stable.
                            frameDelay = Math.Round(0.015 * frameDelay) * 1.186 * (1000 / 60f);

                        var loopType = splitCount > 8 ? parseAnimationLoopType(line[ranges[8]]) : AnimationLoopType.LoopForever;
                        storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Sample:
                    {
                        double time = Parsing.ParseDouble(line[ranges[1]]);
                        string layer = parseLayer(line[ranges[2]]);
                        string path = CleanFilename(line[ranges[3]]);
                        float volume = splitCount > 4 ? Parsing.ParseFloat(line[ranges[4]]) : 100;
                        storyboard.GetLayer(layer).Add(new StoryboardSampleInfo(path, time, (int)volume));
                        break;
                    }
                }
            }
            else
            {
                if (depth < 2)
                    currentCommandsGroup = storyboardSprite?.Commands;

                ReadOnlySpan<char> commandType = line[ranges[0]];

                switch (commandType)
                {
                    case "T":
                    {
                        string triggerName = line[ranges[1]].ToString();
                        double startTime = splitCount > 2 ? Parsing.ParseDouble(line[ranges[2]]) : double.MinValue;
                        double endTime = splitCount > 3 ? Parsing.ParseDouble(line[ranges[3]]) : double.MaxValue;
                        int groupNumber = splitCount > 4 ? Parsing.ParseInt(line[ranges[4]]) : 0;
                        currentCommandsGroup = storyboardSprite?.AddTriggerGroup(triggerName, startTime, endTime, groupNumber);
                        break;
                    }

                    case "L":
                    {
                        double startTime = Parsing.ParseDouble(line[ranges[1]]);
                        int repeatCount = Parsing.ParseInt(line[ranges[2]]);
                        currentCommandsGroup = storyboardSprite?.AddLoopingGroup(startTime, Math.Max(0, repeatCount - 1));
                        break;
                    }

                    default:
                    {
                        if (line[ranges[3]].IsEmpty)
                            ranges[3] = ranges[2];

                        var easing = (Easing)Parsing.ParseInt(line[ranges[1]]);
                        double startTime = Parsing.ParseDouble(line[ranges[2]]);
                        double endTime = Parsing.ParseDouble(line[ranges[3]]);

                        switch (commandType)
                        {
                            case "F":
                            {
                                float startValue = Parsing.ParseFloat(line[ranges[4]]);
                                float endValue = splitCount > 5 ? Parsing.ParseFloat(line[ranges[5]]) : startValue;
                                currentCommandsGroup?.AddAlpha(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "S":
                            {
                                float startValue = Parsing.ParseFloat(line[ranges[4]]);
                                float endValue = splitCount > 5 ? Parsing.ParseFloat(line[ranges[5]]) : startValue;
                                currentCommandsGroup?.AddScale(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "V":
                            {
                                float startX = Parsing.ParseFloat(line[ranges[4]]);
                                float startY = Parsing.ParseFloat(line[ranges[5]]);
                                float endX = splitCount > 6 ? Parsing.ParseFloat(line[ranges[6]]) : startX;
                                float endY = splitCount > 7 ? Parsing.ParseFloat(line[ranges[7]]) : startY;
                                currentCommandsGroup?.AddVectorScale(easing, startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY));
                                break;
                            }

                            case "R":
                            {
                                float startValue = Parsing.ParseFloat(line[ranges[4]]);
                                float endValue = splitCount > 5 ? Parsing.ParseFloat(line[ranges[5]]) : startValue;
                                currentCommandsGroup?.AddRotation(easing, startTime, endTime, float.RadiansToDegrees(startValue), float.RadiansToDegrees(endValue));
                                break;
                            }

                            case "M":
                            {
                                float startX = Parsing.ParseFloat(line[ranges[4]]);
                                float startY = Parsing.ParseFloat(line[ranges[5]]);
                                float endX = splitCount > 6 ? Parsing.ParseFloat(line[ranges[6]]) : startX;
                                float endY = splitCount > 7 ? Parsing.ParseFloat(line[ranges[7]]) : startY;
                                currentCommandsGroup?.AddX(easing, startTime, endTime, startX, endX);
                                currentCommandsGroup?.AddY(easing, startTime, endTime, startY, endY);
                                break;
                            }

                            case "MX":
                            {
                                float startValue = Parsing.ParseFloat(line[ranges[4]]);
                                float endValue = splitCount > 5 ? Parsing.ParseFloat(line[ranges[5]]) : startValue;
                                currentCommandsGroup?.AddX(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "MY":
                            {
                                float startValue = Parsing.ParseFloat(line[ranges[4]]);
                                float endValue = splitCount > 5 ? Parsing.ParseFloat(line[ranges[5]]) : startValue;
                                currentCommandsGroup?.AddY(easing, startTime, endTime, startValue, endValue);
                                break;
                            }

                            case "C":
                            {
                                float startRed = Parsing.ParseFloat(line[ranges[4]]);
                                float startGreen = Parsing.ParseFloat(line[ranges[5]]);
                                float startBlue = Parsing.ParseFloat(line[ranges[6]]);
                                float endRed = splitCount > 7 ? Parsing.ParseFloat(line[ranges[7]]) : startRed;
                                float endGreen = splitCount > 8 ? Parsing.ParseFloat(line[ranges[8]]) : startGreen;
                                float endBlue = splitCount > 9 ? Parsing.ParseFloat(line[ranges[9]]) : startBlue;
                                currentCommandsGroup?.AddColour(easing, startTime, endTime,
                                    new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                                    new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1));
                                break;
                            }

                            case "P":
                            {
                                ReadOnlySpan<char> type = line[ranges[4]];

                                switch (type)
                                {
                                    case "A":
                                        currentCommandsGroup?.AddBlendingParameters(easing, startTime, endTime, BlendingParameters.Additive,
                                            startTime == endTime ? BlendingParameters.Additive : BlendingParameters.Inherit);
                                        break;

                                    case "H":
                                        currentCommandsGroup?.AddFlipH(easing, startTime, endTime, true, startTime == endTime);
                                        break;

                                    case "V":
                                        currentCommandsGroup?.AddFlipV(easing, startTime, endTime, true, startTime == endTime);
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
        private ReadOnlySpan<char> decodeVariables(ReadOnlySpan<char> line)
        {
            while (line.Contains('$'))
            {
                ReadOnlySpan<char> origLine = line.ToString();

                foreach (var v in variables)
                    line = line.ToString().Replace(v.Key, v.Value);

                if (line.SequenceEqual(origLine))
                    break;
            }

            return line;
        }
    }
}
