// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Game.Beatmaps;
using osu.Game.IO.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Legacy;
using osu.Game.Users;
using SharpCompress.Compressors.LZMA;
using osu.Game.Beatmaps.Legacy;
using System.Linq;

namespace osu.Game.Rulesets.Scoring.Legacy
{
    public class LegacyScoreParser
    {
        private readonly RulesetStore rulesets;
        private readonly BeatmapManager beatmaps;

        public LegacyScoreParser(RulesetStore rulesets, BeatmapManager beatmaps)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
        }

        private Beatmap currentBeatmap;
        private Ruleset currentRuleset;

        public Score Parse(Stream stream)
        {
            Score score;

            using (SerializationReader sr = new SerializationReader(stream))
            {
                score = new Score { Ruleset = rulesets.GetRuleset(sr.ReadByte()) };
                currentRuleset = score.Ruleset.CreateInstance();

                /* score.Pass = true;*/
                var version = sr.ReadInt32();

                /* score.FileChecksum = */
                var beatmapHash = sr.ReadString();
                score.Beatmap = beatmaps.QueryBeatmap(b => b.MD5Hash == beatmapHash);
                currentBeatmap = beatmaps.GetWorkingBeatmap(score.Beatmap).Beatmap;

                /* score.PlayerName = */
                score.User = new User { Username = sr.ReadString() };
                /* var localScoreChecksum = */
                sr.ReadString();
                /* score.Count300 = */
                sr.ReadUInt16();
                /* score.Count100 = */
                sr.ReadUInt16();
                /* score.Count50 = */
                sr.ReadUInt16();
                /* score.CountGeki = */
                sr.ReadUInt16();
                /* score.CountKatu = */
                sr.ReadUInt16();
                /* score.CountMiss = */
                sr.ReadUInt16();
                score.TotalScore = sr.ReadInt32();
                score.MaxCombo = sr.ReadUInt16();
                /* score.Perfect = */
                sr.ReadBoolean();
                /* score.EnabledMods = (Mods)*/
                score.Mods = currentRuleset.ConvertLegacyMods((LegacyMods)sr.ReadInt32()).ToArray();
                /* score.HpGraphString = */
                sr.ReadString();
                /* score.Date = */
                sr.ReadDateTime();

                var compressedReplay = sr.ReadByteArray();

                if (version >= 20140721)
                    /*OnlineId =*/
                    sr.ReadInt64();
                else if (version >= 20121008)
                    /*OnlineId =*/
                    sr.ReadInt32();

                using (var replayInStream = new MemoryStream(compressedReplay))
                {
                    byte[] properties = new byte[5];
                    if (replayInStream.Read(properties, 0, 5) != 5)
                        throw new IOException("input .lzma is too short");
                    long outSize = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        int v = replayInStream.ReadByte();
                        if (v < 0)
                            throw new IOException("Can't Read 1");
                        outSize |= (long)(byte)v << (8 * i);
                    }

                    long compressedSize = replayInStream.Length - replayInStream.Position;

                    using (var lzma = new LzmaStream(properties, replayInStream, compressedSize, outSize))
                    using (var reader = new StreamReader(lzma))
                    {
                        score.Replay = new Replay { User = score.User };
                        readLegacyReplay(score.Replay, reader);
                    }
                }
            }

            return score;
        }

        private void readLegacyReplay(Replay replay, StreamReader reader)
        {
            float lastTime = 0;

            foreach (var l in reader.ReadToEnd().Split(','))
            {
                var split = l.Split('|');

                if (split.Length < 4)
                    continue;

                if (split[0] == "-12345")
                {
                    // Todo: The seed is provided in split[3], which we'll need to use at some point
                    continue;
                }

                var diff = float.Parse(split[0]);
                lastTime += diff;

                // Todo: At some point we probably want to rewind and play back the negative-time frames
                // but for now we'll achieve equal playback to stable by skipping negative frames
                if (diff < 0)
                    continue;

                replay.Frames.Add(convertFrame(new LegacyReplayFrame(lastTime, float.Parse(split[1]), float.Parse(split[2]), (ReplayButtonState)int.Parse(split[3]))));
            }
        }

        private ReplayFrame convertFrame(LegacyReplayFrame legacyFrame)
        {
            var convertible = currentRuleset.CreateConvertibleReplayFrame();
            if (convertible == null)
                throw new InvalidOperationException($"Legacy replay cannot be converted for the ruleset: {currentRuleset.Description}");
            convertible.ConvertFrom(legacyFrame, currentBeatmap);

            var frame = (ReplayFrame)convertible;
            frame.Time = legacyFrame.Time;

            return frame;
        }
    }
}
