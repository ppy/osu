// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Database;
using osu.Game.IO.Legacy;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Replays;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Replays;
using SharpCompress.Compressors.LZMA;

namespace osu.Game.Scoring.Legacy
{
    public abstract class LegacyScoreDecoder
    {
        private IBeatmap currentBeatmap;
        private Ruleset currentRuleset;

        private float beatmapOffset;

        public Score Parse(Stream stream)
        {
            var score = new Score
            {
                Replay = new Replay()
            };

            WorkingBeatmap workingBeatmap;
            byte[] compressedScoreInfo = null;

            using (SerializationReader sr = new SerializationReader(stream))
            {
                currentRuleset = GetRuleset(sr.ReadByte());
                var scoreInfo = new ScoreInfo { Ruleset = currentRuleset.RulesetInfo };

                score.ScoreInfo = scoreInfo;

                int version = sr.ReadInt32();

                scoreInfo.IsLegacyScore = version < LegacyScoreEncoder.FIRST_LAZER_VERSION;

                // TotalScoreVersion gets initialised to LATEST_VERSION.
                // In the case where the incoming score has either an osu!stable or old lazer version, we need
                // to mark it with the correct version increment to trigger reprocessing to new standardised scoring.
                //
                // See StandardisedScoreMigrationTools.ShouldMigrateToNewStandardised().
                scoreInfo.TotalScoreVersion = version < 30000002 ? 30000001 : LegacyScoreEncoder.LATEST_VERSION;

                string beatmapHash = sr.ReadString();

                workingBeatmap = GetBeatmap(beatmapHash);

                if (workingBeatmap is DummyWorkingBeatmap)
                    throw new BeatmapNotFoundException(beatmapHash);

                scoreInfo.User = new APIUser { Username = sr.ReadString() };

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
                    scoreInfo.Mods = scoreInfo.Mods.Append(currentRuleset.CreateMod<ModClassic>()).ToArray();

                currentBeatmap = workingBeatmap.GetPlayableBeatmap(currentRuleset.RulesetInfo, scoreInfo.Mods);
                scoreInfo.BeatmapInfo = currentBeatmap.BeatmapInfo;

                // As this is baked into hitobject timing (see `LegacyBeatmapDecoder`) we also need to apply this to replay frame timing.
                beatmapOffset = currentBeatmap.BeatmapInfo.BeatmapVersion < 5 ? LegacyBeatmapDecoder.EARLY_VERSION_TIMING_OFFSET : 0;

                /* score.HpGraphString = */
                sr.ReadString();

                scoreInfo.Date = sr.ReadDateTime();

                byte[] compressedReplay = sr.ReadByteArray();

                if (version >= 20140721)
                    scoreInfo.LegacyOnlineID = sr.ReadInt64();
                else if (version >= 20121008)
                    scoreInfo.LegacyOnlineID = sr.ReadInt32();

                if (version >= 30000001)
                    compressedScoreInfo = sr.ReadByteArray();

                if (compressedReplay?.Length > 0)
                    readCompressedData(compressedReplay, reader => readLegacyReplay(score.Replay, reader));

                if (compressedScoreInfo?.Length > 0)
                {
                    readCompressedData(compressedScoreInfo, reader =>
                    {
                        LegacyReplaySoloScoreInfo readScore = JsonConvert.DeserializeObject<LegacyReplaySoloScoreInfo>(reader.ReadToEnd());

                        Debug.Assert(readScore != null);

                        score.ScoreInfo.OnlineID = readScore.OnlineID;
                        score.ScoreInfo.Statistics = readScore.Statistics;
                        score.ScoreInfo.MaximumStatistics = readScore.MaximumStatistics;
                        score.ScoreInfo.Mods = readScore.Mods.Select(m => m.ToMod(currentRuleset)).ToArray();
                        score.ScoreInfo.ClientVersion = readScore.ClientVersion;
                    });
                }
            }

            if (score.ScoreInfo.IsLegacyScore || compressedScoreInfo == null)
                PopulateLegacyAccuracyAndRank(score.ScoreInfo);
            else
                populateLazerAccuracyAndRank(score.ScoreInfo);

            // before returning for database import, we must restore the database-sourced BeatmapInfo.
            // if not, the clone operation in GetPlayableBeatmap will cause a dereference and subsequent database exception.
            score.ScoreInfo.BeatmapInfo = workingBeatmap.BeatmapInfo;
            score.ScoreInfo.BeatmapHash = workingBeatmap.BeatmapInfo.Hash;

            return score;
        }

        private void readCompressedData(byte[] data, Action<StreamReader> readFunc)
        {
            using (var replayInStream = new MemoryStream(data))
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
                    readFunc(reader);
            }
        }

        /// <summary>
        /// Populates the accuracy of a given <see cref="ScoreInfo"/> from its contained statistics.
        /// </summary>
        /// <remarks>
        /// Legacy use only.
        /// </remarks>
        /// <param name="score">The <see cref="ScoreInfo"/> to populate.</param>
        public static void PopulateLegacyAccuracyAndRank(ScoreInfo score)
        {
            int countMiss = score.GetCountMiss() ?? 0;
            int count50 = score.GetCount50() ?? 0;
            int count100 = score.GetCount100() ?? 0;
            int count300 = score.GetCount300() ?? 0;
            int countGeki = score.GetCountGeki() ?? 0;
            int countKatu = score.GetCountKatu() ?? 0;

            switch (score.Ruleset.OnlineID)
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

        private void populateLazerAccuracyAndRank(ScoreInfo scoreInfo)
        {
            scoreInfo.Accuracy = StandardisedScoreMigrationTools.ComputeAccuracy(scoreInfo);

            var rank = currentRuleset.CreateScoreProcessor().RankFromScore(scoreInfo.Accuracy, scoreInfo.Statistics);

            foreach (var mod in scoreInfo.Mods.OfType<IApplicableToScoreProcessor>())
                rank = mod.AdjustRank(rank, scoreInfo.Accuracy);

            scoreInfo.Rank = rank;
        }

        private void readLegacyReplay(Replay replay, StreamReader reader)
        {
            float lastTime = beatmapOffset;
            ReplayFrame currentFrame = null;

            string[] frames = reader.ReadToEnd().Split(',');

            for (int i = 0; i < frames.Length; i++)
            {
                string[] split = frames[i].Split('|');

                if (split.Length < 4)
                    continue;

                if (split[0] == "-12345")
                {
                    // Todo: The seed is provided in split[3], which we'll need to use at some point
                    continue;
                }

                float diff = Parsing.ParseFloat(split[0]);
                float mouseX = Parsing.ParseFloat(split[1], Parsing.MAX_COORDINATE_VALUE);
                float mouseY = Parsing.ParseFloat(split[2], Parsing.MAX_COORDINATE_VALUE);

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
            public string Hash { get; }

            public BeatmapNotFoundException(string hash)
            {
                Hash = hash;
            }
        }
    }
}
