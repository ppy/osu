﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Legacy;
using osu.Game.IO.Legacy;
using osu.Game.Replays;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;
using SharpCompress.Compressors.LZMA;

namespace osu.Game.Scoring.Legacy
{
    public abstract class LegacyScoreDecoder
    {
        private IBeatmap currentBeatmap;
        private Ruleset currentRuleset;

        public Score Parse(Stream stream)
        {
            var score = new Score
            {
                Replay = new Replay()
            };

            WorkingBeatmap workingBeatmap;

            using (SerializationReader sr = new SerializationReader(stream))
            {
                currentRuleset = GetRuleset(sr.ReadByte());
                var scoreInfo = new ScoreInfo { Ruleset = currentRuleset.RulesetInfo };

                score.ScoreInfo = scoreInfo;

                var version = sr.ReadInt32();

                workingBeatmap = GetBeatmap(sr.ReadString());
                if (workingBeatmap is DummyWorkingBeatmap)
                    throw new BeatmapNotFoundException();

                scoreInfo.User = new User { Username = sr.ReadString() };

                // MD5Hash
                sr.ReadString();

                scoreInfo.SetCount300(sr.ReadUInt16());
                scoreInfo.SetCount100(sr.ReadUInt16());
                scoreInfo.SetCount50(sr.ReadUInt16());
                scoreInfo.SetCountGeki(sr.ReadUInt16());
                scoreInfo.SetCountKatu(sr.ReadUInt16());
                scoreInfo.SetCountMiss(sr.ReadUInt16());

                scoreInfo.TotalScore = sr.ReadInt32();
                scoreInfo.MaxCombo = sr.ReadUInt16();

                /* score.Perfect = */
                sr.ReadBoolean();

                scoreInfo.Mods = currentRuleset.ConvertFromLegacyMods((LegacyMods)sr.ReadInt32()).ToArray();

                // lazer replays get a really high version number.
                if (version < LegacyScoreEncoder.FIRST_LAZER_VERSION)
                    scoreInfo.Mods = scoreInfo.Mods.Append(currentRuleset.GetAllMods().OfType<ModClassic>().Single()).ToArray();

                currentBeatmap = workingBeatmap.GetPlayableBeatmap(currentRuleset.RulesetInfo, scoreInfo.Mods);
                scoreInfo.Beatmap = currentBeatmap.BeatmapInfo;

                /* score.HpGraphString = */
                sr.ReadString();

                scoreInfo.Date = sr.ReadDateTime();

                var compressedReplay = sr.ReadByteArray();

                if (version >= 20140721)
                    scoreInfo.OnlineScoreID = sr.ReadInt64();
                else if (version >= 20121008)
                    scoreInfo.OnlineScoreID = sr.ReadInt32();

                if (scoreInfo.OnlineScoreID <= 0)
                    scoreInfo.OnlineScoreID = null;

                if (compressedReplay?.Length > 0)
                {
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
                            readLegacyReplay(score.Replay, reader);
                    }
                }
            }

            CalculateAccuracy(score.ScoreInfo);

            // before returning for database import, we must restore the database-sourced BeatmapInfo.
            // if not, the clone operation in GetPlayableBeatmap will cause a dereference and subsequent database exception.
            score.ScoreInfo.Beatmap = workingBeatmap.BeatmapInfo;

            return score;
        }

        protected void CalculateAccuracy(ScoreInfo score)
        {
            int countMiss = score.GetCountMiss() ?? 0;
            int count50 = score.GetCount50() ?? 0;
            int count100 = score.GetCount100() ?? 0;
            int count300 = score.GetCount300() ?? 0;
            int countGeki = score.GetCountGeki() ?? 0;
            int countKatu = score.GetCountKatu() ?? 0;

            switch (score.Ruleset.ID)
            {
                case 0:
                {
                    int totalHits = count50 + count100 + count300 + countMiss;
                    score.Accuracy = totalHits > 0 ? (double)(count50 * 50 + count100 * 100 + count300 * 300) / (totalHits * 300) : 1;

                    float ratio300 = (float)count300 / totalHits;
                    float ratio50 = (float)count50 / totalHits;

                    if (ratio300 == 1)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.XH : ScoreRank.X;
                    else if (ratio300 > 0.9 && ratio50 <= 0.01 && countMiss == 0)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.SH : ScoreRank.S;
                    else if ((ratio300 > 0.8 && countMiss == 0) || ratio300 > 0.9)
                        score.Rank = ScoreRank.A;
                    else if ((ratio300 > 0.7 && countMiss == 0) || ratio300 > 0.8)
                        score.Rank = ScoreRank.B;
                    else if (ratio300 > 0.6)
                        score.Rank = ScoreRank.C;
                    else
                        score.Rank = ScoreRank.D;
                    break;
                }

                case 1:
                {
                    int totalHits = count50 + count100 + count300 + countMiss;
                    score.Accuracy = totalHits > 0 ? (double)(count100 * 150 + count300 * 300) / (totalHits * 300) : 1;

                    float ratio300 = (float)count300 / totalHits;
                    float ratio50 = (float)count50 / totalHits;

                    if (ratio300 == 1)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.XH : ScoreRank.X;
                    else if (ratio300 > 0.9 && ratio50 <= 0.01 && countMiss == 0)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.SH : ScoreRank.S;
                    else if ((ratio300 > 0.8 && countMiss == 0) || ratio300 > 0.9)
                        score.Rank = ScoreRank.A;
                    else if ((ratio300 > 0.7 && countMiss == 0) || ratio300 > 0.8)
                        score.Rank = ScoreRank.B;
                    else if (ratio300 > 0.6)
                        score.Rank = ScoreRank.C;
                    else
                        score.Rank = ScoreRank.D;
                    break;
                }

                case 2:
                {
                    int totalHits = count50 + count100 + count300 + countMiss + countKatu;
                    score.Accuracy = totalHits > 0 ? (double)(count50 + count100 + count300) / totalHits : 1;

                    if (score.Accuracy == 1)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.XH : ScoreRank.X;
                    else if (score.Accuracy > 0.98)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.SH : ScoreRank.S;
                    else if (score.Accuracy > 0.94)
                        score.Rank = ScoreRank.A;
                    else if (score.Accuracy > 0.9)
                        score.Rank = ScoreRank.B;
                    else if (score.Accuracy > 0.85)
                        score.Rank = ScoreRank.C;
                    else
                        score.Rank = ScoreRank.D;
                    break;
                }

                case 3:
                {
                    int totalHits = count50 + count100 + count300 + countMiss + countGeki + countKatu;
                    score.Accuracy = totalHits > 0 ? (double)(count50 * 50 + count100 * 100 + countKatu * 200 + (count300 + countGeki) * 300) / (totalHits * 300) : 1;

                    if (score.Accuracy == 1)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.XH : ScoreRank.X;
                    else if (score.Accuracy > 0.95)
                        score.Rank = score.Mods.Any(m => m is ModHidden || m is ModFlashlight) ? ScoreRank.SH : ScoreRank.S;
                    else if (score.Accuracy > 0.9)
                        score.Rank = ScoreRank.A;
                    else if (score.Accuracy > 0.8)
                        score.Rank = ScoreRank.B;
                    else if (score.Accuracy > 0.7)
                        score.Rank = ScoreRank.C;
                    else
                        score.Rank = ScoreRank.D;
                    break;
                }
            }
        }

        private void readLegacyReplay(Replay replay, StreamReader reader)
        {
            float lastTime = 0;
            ReplayFrame currentFrame = null;

            var frames = reader.ReadToEnd().Split(',');

            for (var i = 0; i < frames.Length; i++)
            {
                var split = frames[i].Split('|');

                if (split.Length < 4)
                    continue;

                if (split[0] == "-12345")
                {
                    // Todo: The seed is provided in split[3], which we'll need to use at some point
                    continue;
                }

                var diff = Parsing.ParseFloat(split[0]);
                var mouseX = Parsing.ParseFloat(split[1], Parsing.MAX_COORDINATE_VALUE);
                var mouseY = Parsing.ParseFloat(split[2], Parsing.MAX_COORDINATE_VALUE);

                lastTime += diff;

                if (i < 2 && mouseX == 256 && mouseY == -500)
                    // at the start of the replay, stable places two replay frames, at time 0 and SkipBoundary - 1, respectively.
                    // both frames use a position of (256, -500).
                    // ignore these frames as they serve no real purpose (and can even mislead ruleset-specific handlers - see mania)
                    continue;

                // Todo: At some point we probably want to rewind and play back the negative-time frames
                // but for now we'll achieve equal playback to stable by skipping negative frames
                if (diff < 0)
                    continue;

                currentFrame = convertFrame(new LegacyReplayFrame(lastTime,
                    mouseX,
                    mouseY,
                    (ReplayButtonState)Parsing.ParseInt(split[3])), currentFrame);

                replay.Frames.Add(currentFrame);
            }
        }

        private ReplayFrame convertFrame(LegacyReplayFrame currentFrame, ReplayFrame lastFrame)
        {
            var convertible = currentRuleset.CreateConvertibleReplayFrame();
            if (convertible == null)
                throw new InvalidOperationException($"Legacy replay cannot be converted for the ruleset: {currentRuleset.Description}");

            convertible.FromLegacy(currentFrame, currentBeatmap, lastFrame);

            var frame = (ReplayFrame)convertible;
            frame.Time = currentFrame.Time;

            return frame;
        }

        /// <summary>
        /// Retrieves the <see cref="Ruleset"/> for a specific id.
        /// </summary>
        /// <param name="rulesetId">The id.</param>
        /// <returns>The <see cref="Ruleset"/>.</returns>
        protected abstract Ruleset GetRuleset(int rulesetId);

        /// <summary>
        /// Retrieves the <see cref="WorkingBeatmap"/> corresponding to an MD5 hash.
        /// </summary>
        /// <param name="md5Hash">The MD5 hash.</param>
        /// <returns>The <see cref="WorkingBeatmap"/>.</returns>
        protected abstract WorkingBeatmap GetBeatmap(string md5Hash);

        public class BeatmapNotFoundException : Exception
        {
            public BeatmapNotFoundException()
                : base("No corresponding beatmap for the score could be found.")
            {
            }
        }
    }
}
