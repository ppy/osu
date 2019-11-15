// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
                var scoreInfo = new LegacyScoreInfo { Ruleset = currentRuleset.RulesetInfo };

                score.ScoreInfo = scoreInfo;

                var version = sr.ReadInt32();

                var workingBeatmap = GetBeatmap(sr.ReadString());
                if (workingBeatmap is DummyWorkingBeatmap)
                    throw new BeatmapNotFoundException();

                currentBeatmap = workingBeatmap.Beatmap;
                scoreInfo.Beatmap = currentBeatmap.BeatmapInfo;

                scoreInfo.User = new User { Username = sr.ReadString() };

                // MD5Hash
                sr.ReadString();

                scoreInfo.Count300 = sr.ReadUInt16();
                scoreInfo.Count100 = sr.ReadUInt16();
                scoreInfo.Count50 = sr.ReadUInt16();
                scoreInfo.CountGeki = sr.ReadUInt16();
                scoreInfo.CountKatu = sr.ReadUInt16();
                scoreInfo.CountMiss = sr.ReadUInt16();

                scoreInfo.TotalScore = sr.ReadInt32();
                scoreInfo.MaxCombo = sr.ReadUInt16();

                /* score.Perfect = */
                sr.ReadBoolean();

                scoreInfo.Mods = currentRuleset.ConvertLegacyMods((LegacyMods)sr.ReadInt32()).ToArray();

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

                var diff = Parsing.ParseFloat(split[0]);
                lastTime += diff;

                // Todo: At some point we probably want to rewind and play back the negative-time frames
                // but for now we'll achieve equal playback to stable by skipping negative frames
                if (diff < 0)
                    continue;

                currentFrame = convertFrame(new LegacyReplayFrame(lastTime,
                    Parsing.ParseFloat(split[1], Parsing.MAX_COORDINATE_VALUE),
                    Parsing.ParseFloat(split[2], Parsing.MAX_COORDINATE_VALUE),
                    (ReplayButtonState)Parsing.ParseInt(split[3])), currentFrame);

                replay.Frames.Add(currentFrame);
            }
        }

        private ReplayFrame convertFrame(LegacyReplayFrame currentFrame, ReplayFrame lastFrame)
        {
            var convertible = currentRuleset.CreateConvertibleReplayFrame();
            if (convertible == null)
                throw new InvalidOperationException($"Legacy replay cannot be converted for the ruleset: {currentRuleset.Description}");

            convertible.ConvertFrom(currentFrame, currentBeatmap, lastFrame);

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
