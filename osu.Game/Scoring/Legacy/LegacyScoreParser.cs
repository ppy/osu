﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.IO.Legacy;
using osu.Game.Replays;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;
using SharpCompress.Compressors.LZMA;

namespace osu.Game.Scoring.Legacy
{
    public abstract class LegacyScoreParser
    {
        private IBeatmap currentBeatmap;
        private Ruleset currentRuleset;

        public Score Parse(Stream stream)
        {
            var score = new Score
            {
                ScoreInfo = new ScoreInfo(),
                Replay = new Replay()
            };

            using (SerializationReader sr = new SerializationReader(stream))
            {
                currentRuleset = GetRuleset(sr.ReadByte());
                score.ScoreInfo = new ScoreInfo { Ruleset = currentRuleset.RulesetInfo };

                var version = sr.ReadInt32();

                var workingBeatmap = GetBeatmap(sr.ReadString());
                if (workingBeatmap is DummyWorkingBeatmap)
                    throw new BeatmapNotFoundException();

                currentBeatmap = workingBeatmap.Beatmap;
                score.ScoreInfo.Beatmap = currentBeatmap.BeatmapInfo;

                score.ScoreInfo.User = new User { Username = sr.ReadString() };

                // MD5Hash
                sr.ReadString();

                var count300 = (int)sr.ReadUInt16();
                var count100 = (int)sr.ReadUInt16();
                var count50 = (int)sr.ReadUInt16();
                var countGeki = (int)sr.ReadUInt16();
                var countKatu = (int)sr.ReadUInt16();
                var countMiss = (int)sr.ReadUInt16();

                switch (currentRuleset.LegacyID)
                {
                    case 0:
                        score.ScoreInfo.Statistics[HitResult.Great] = count300;
                        score.ScoreInfo.Statistics[HitResult.Good] = count100;
                        score.ScoreInfo.Statistics[HitResult.Meh] = count50;
                        score.ScoreInfo.Statistics[HitResult.Miss] = countMiss;
                        break;
                    case 1:
                        score.ScoreInfo.Statistics[HitResult.Great] = count300;
                        score.ScoreInfo.Statistics[HitResult.Good] = count100;
                        score.ScoreInfo.Statistics[HitResult.Miss] = countMiss;
                        break;
                    case 2:
                        score.ScoreInfo.Statistics[HitResult.Perfect] = count300;
                        score.ScoreInfo.Statistics[HitResult.Miss] = countMiss;
                        break;
                    case 3:
                        score.ScoreInfo.Statistics[HitResult.Perfect] = countGeki;
                        score.ScoreInfo.Statistics[HitResult.Great] = count300;
                        score.ScoreInfo.Statistics[HitResult.Good] = countKatu;
                        score.ScoreInfo.Statistics[HitResult.Ok] = count100;
                        score.ScoreInfo.Statistics[HitResult.Meh] = count50;
                        score.ScoreInfo.Statistics[HitResult.Miss] = countMiss;
                        break;
                }

                score.ScoreInfo.TotalScore = sr.ReadInt32();
                score.ScoreInfo.MaxCombo = sr.ReadUInt16();

                /* score.Perfect = */
                sr.ReadBoolean();

                score.ScoreInfo.Mods = currentRuleset.ConvertLegacyMods((LegacyMods)sr.ReadInt32()).ToArray();

                /* score.HpGraphString = */
                sr.ReadString();

                score.ScoreInfo.Date = sr.ReadDateTime();

                var compressedReplay = sr.ReadByteArray();

                if (version >= 20140721)
                    score.ScoreInfo.OnlineScoreID = sr.ReadInt64();
                else if (version >= 20121008)
                    score.ScoreInfo.OnlineScoreID = sr.ReadInt32();

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

            return score;
        }

        protected void CalculateAccuracy(ScoreInfo score)
        {
            score.Statistics.TryGetValue(HitResult.Miss, out int countMiss);
            score.Statistics.TryGetValue(HitResult.Meh, out int count50);
            score.Statistics.TryGetValue(HitResult.Good, out int count100);
            score.Statistics.TryGetValue(HitResult.Great, out int count300);
            score.Statistics.TryGetValue(HitResult.Perfect, out int countGeki);
            score.Statistics.TryGetValue(HitResult.Ok, out int countKatu);

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
                    else if (ratio300 > 0.8 && countMiss == 0 || ratio300 > 0.9)
                        score.Rank = ScoreRank.A;
                    else if (ratio300 > 0.7 && countMiss == 0 || ratio300 > 0.8)
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
                    else if (ratio300 > 0.8 && countMiss == 0 || ratio300 > 0.9)
                        score.Rank = ScoreRank.A;
                    else if (ratio300 > 0.7 && countMiss == 0 || ratio300 > 0.8)
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
