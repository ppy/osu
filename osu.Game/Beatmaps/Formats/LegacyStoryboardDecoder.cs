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
using osu.Game.Storyboards.Commands;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyStoryboardDecoder : LegacyDecoder<Storyboard>
    {
        private StoryboardSprite? storyboardSprite;
        private StoryboardCommandGroup? currentGroup;

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

            line = line.Substring(depth);

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
                        int offset = Parsing.ParseInt(split[1]);
                        string path = CleanFilename(split[2]);

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
                        string layer = parseLayer(split[1]);
                        var origin = parseOrigin(split[2]);
                        string path = CleanFilename(split[3]);
                        float x = Parsing.ParseFloat(split[4], Parsing.MAX_COORDINATE_VALUE);
                        float y = Parsing.ParseFloat(split[5], Parsing.MAX_COORDINATE_VALUE);
                        storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Animation:
                    {
                        string layer = parseLayer(split[1]);
                        var origin = parseOrigin(split[2]);
                        string path = CleanFilename(split[3]);
                        float x = Parsing.ParseFloat(split[4], Parsing.MAX_COORDINATE_VALUE);
                        float y = Parsing.ParseFloat(split[5], Parsing.MAX_COORDINATE_VALUE);
                        int frameCount = Parsing.ParseInt(split[6]);
                        double frameDelay = Parsing.ParseDouble(split[7]);

                        if (FormatVersion < 6)
                            // this is random as hell but taken straight from osu-stable.
                            frameDelay = Math.Round(0.015 * frameDelay) * 1.186 * (1000 / 60f);

                        var loopType = split.Length > 8 ? parseAnimationLoopType(split[8]) : AnimationLoopType.LoopForever;
                        storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                        storyboard.GetLayer(layer).Add(storyboardSprite);
                        break;
                    }

                    case LegacyEventType.Sample:
                    {
                        double time = Parsing.ParseDouble(split[1]);
                        string layer = parseLayer(split[2]);
                        string path = CleanFilename(split[3]);
                        float volume = split.Length > 4 ? Parsing.ParseFloat(split[4]) : 100;
                        storyboard.GetLayer(layer).Add(new StoryboardSampleInfo(path, time, (int)volume));
                        break;
                    }
                }
            }
            else
            {
                if (depth < 2)
                    currentGroup = storyboardSprite?.Group;

                string commandType = split[0];

                switch (commandType)
                {
                    case "T":
                    {
                        string triggerName = split[1];
                        double startTime = split.Length > 2 ? Parsing.ParseDouble(split[2]) : double.MinValue;
                        double endTime = split.Length > 3 ? Parsing.ParseDouble(split[3]) : double.MaxValue;
                        int groupNumber = split.Length > 4 ? Parsing.ParseInt(split[4]) : 0;
                        currentGroup = storyboardSprite?.AddTriggerGroup(triggerName, startTime, endTime, groupNumber);
                        break;
                    }

                    case "L":
                    {
                        double startTime = Parsing.ParseDouble(split[1]);
                        int repeatCount = Parsing.ParseInt(split[2]);
                        currentGroup = storyboardSprite?.AddLoopingGroup(startTime, Math.Max(0, repeatCount - 1));
                        break;
                    }

                    default:
                    {
                        if (string.IsNullOrEmpty(split[3]))
                            split[3] = split[2];

                        var easing = (Easing)Parsing.ParseInt(split[1]);
                        double startTime = Parsing.ParseDouble(split[2]);
                        double endTime = Parsing.ParseDouble(split[3]);

                        switch (commandType)
                        {
                            case "F":
                            {
                                float startValue = Parsing.ParseFloat(split[4]);
                                float endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                currentGroup?.AddAlpha(startTime, endTime, startValue, endValue, easing);
                                break;
                            }

                            case "S":
                            {
                                float startValue = Parsing.ParseFloat(split[4]);
                                float endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                currentGroup?.AddScale(startTime, endTime, startValue, endValue, easing);
                                break;
                            }

                            case "V":
                            {
                                float startX = Parsing.ParseFloat(split[4]);
                                float startY = Parsing.ParseFloat(split[5]);
                                float endX = split.Length > 6 ? Parsing.ParseFloat(split[6]) : startX;
                                float endY = split.Length > 7 ? Parsing.ParseFloat(split[7]) : startY;
                                currentGroup?.AddVectorScale(startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY), easing);
                                break;
                            }

                            case "R":
                            {
                                float startValue = Parsing.ParseFloat(split[4]);
                                float endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                currentGroup?.AddRotation(startTime, endTime, MathUtils.RadiansToDegrees(startValue), MathUtils.RadiansToDegrees(endValue), easing);
                                break;
                            }

                            case "M":
                            {
                                float startX = Parsing.ParseFloat(split[4]);
                                float startY = Parsing.ParseFloat(split[5]);
                                float endX = split.Length > 6 ? Parsing.ParseFloat(split[6]) : startX;
                                float endY = split.Length > 7 ? Parsing.ParseFloat(split[7]) : startY;
                                currentGroup?.AddX(startTime, endTime, startX, endX, easing);
                                currentGroup?.AddY(startTime, endTime, startY, endY, easing);
                                break;
                            }

                            case "MX":
                            {
                                float startValue = Parsing.ParseFloat(split[4]);
                                float endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                currentGroup?.AddX(startTime, endTime, startValue, endValue, easing);
                                break;
                            }

                            case "MY":
                            {
                                float startValue = Parsing.ParseFloat(split[4]);
                                float endValue = split.Length > 5 ? Parsing.ParseFloat(split[5]) : startValue;
                                currentGroup?.AddY(startTime, endTime, startValue, endValue, easing);
                                break;
                            }

                            case "C":
                            {
                                float startRed = Parsing.ParseFloat(split[4]);
                                float startGreen = Parsing.ParseFloat(split[5]);
                                float startBlue = Parsing.ParseFloat(split[6]);
                                float endRed = split.Length > 7 ? Parsing.ParseFloat(split[7]) : startRed;
                                float endGreen = split.Length > 8 ? Parsing.ParseFloat(split[8]) : startGreen;
                                float endBlue = split.Length > 9 ? Parsing.ParseFloat(split[9]) : startBlue;
                                currentGroup?.AddColour(startTime, endTime,
                                    new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                                    new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1), easing);
                                break;
                            }

                            case "P":
                            {
                                string type = split[4];

                                switch (type)
                                {
                                    case "A":
                                        currentGroup?.AddBlendingParameters(startTime, endTime, BlendingParameters.Additive,
                                            startTime == endTime ? BlendingParameters.Additive : BlendingParameters.Inherit, easing);
                                        break;

                                    case "H":
                                        currentGroup?.AddFlipH(startTime, endTime, true, startTime == endTime, easing);
                                        break;

                                    case "V":
                                        currentGroup?.AddFlipV(startTime, endTime, true, startTime == endTime, easing);
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

        private string parseLayer(string value) => Enum.Parse<LegacyStoryLayer>(value).ToString();

        private Anchor parseOrigin(string value)
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

        private AnimationLoopType parseAnimationLoopType(string value)
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
