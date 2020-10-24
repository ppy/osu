// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Text;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.IO.Legacy;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays.Types;
using SharpCompress.Compressors.LZMA;

namespace osu.Game.Scoring.Legacy
{
    public class LegacyScoreEncoder
    {
        public const int LATEST_VERSION = 128;

        private readonly Score score;
        private readonly IBeatmap beatmap;

        public LegacyScoreEncoder(Score score, IBeatmap beatmap)
        {
            this.score = score;
            this.beatmap = beatmap;

            if (score.ScoreInfo.Beatmap.RulesetID < 0 || score.ScoreInfo.Beatmap.RulesetID > 3)
                throw new ArgumentException("Only scores in the osu, taiko, catch, or mania rulesets can be encoded to the legacy score format.", nameof(score));
        }

        public void Encode(Stream stream)
        {
            using (SerializationWriter sw = new SerializationWriter(stream))
            {
                sw.Write((byte)(score.ScoreInfo.Ruleset.ID ?? 0));
                sw.Write(LATEST_VERSION);
                sw.Write(score.ScoreInfo.Beatmap.MD5Hash);
                sw.Write(score.ScoreInfo.UserString);
                sw.Write($"lazer-{score.ScoreInfo.UserString}-{score.ScoreInfo.Date}".ComputeMD5Hash());
                sw.Write((ushort)(score.ScoreInfo.GetCount300() ?? 0));
                sw.Write((ushort)(score.ScoreInfo.GetCount100() ?? 0));
                sw.Write((ushort)(score.ScoreInfo.GetCount50() ?? 0));
                sw.Write((ushort)(score.ScoreInfo.GetCountGeki() ?? 0));
                sw.Write((ushort)(score.ScoreInfo.GetCountKatu() ?? 0));
                sw.Write((ushort)(score.ScoreInfo.GetCountMiss() ?? 0));
                sw.Write((int)(score.ScoreInfo.TotalScore));
                sw.Write((ushort)score.ScoreInfo.MaxCombo);
                sw.Write(score.ScoreInfo.Combo == score.ScoreInfo.MaxCombo);
                sw.Write((int)score.ScoreInfo.Ruleset.CreateInstance().ConvertToLegacyMods(score.ScoreInfo.Mods));

                sw.Write(getHpGraphFormatted());
                sw.Write(score.ScoreInfo.Date.DateTime);
                sw.WriteByteArray(createReplayData());
                sw.Write((long)0);
                writeModSpecificData(score.ScoreInfo, sw);
            }
        }

        private void writeModSpecificData(ScoreInfo score, SerializationWriter sw)
        {
        }

        private byte[] createReplayData()
        {
            var content = new ASCIIEncoding().GetBytes(replayStringContent);

            using (var outStream = new MemoryStream())
            {
                using (var lzma = new LzmaStream(new LzmaEncoderProperties(false, 1 << 21, 255), false, outStream))
                {
                    outStream.Write(lzma.Properties);

                    long fileSize = content.Length;
                    for (int i = 0; i < 8; i++)
                        outStream.WriteByte((byte)(fileSize >> (8 * i)));

                    lzma.Write(content);
                }

                return outStream.ToArray();
            }
        }

        private string replayStringContent
        {
            get
            {
                StringBuilder replayData = new StringBuilder();

                if (score.Replay != null)
                {
                    LegacyReplayFrame lastF = new LegacyReplayFrame(0, 0, 0, ReplayButtonState.None);

                    foreach (var f in score.Replay.Frames.OfType<IConvertibleReplayFrame>().Select(f => f.ToLegacy(beatmap)))
                    {
                        replayData.Append(FormattableString.Invariant($"{f.Time - lastF.Time}|{f.MouseX ?? 0}|{f.MouseY ?? 0}|{(int)f.ButtonState},"));
                        lastF = f;
                    }
                }

                replayData.AppendFormat(@"{0}|{1}|{2}|{3},", -12345, 0, 0, 0);
                return replayData.ToString();
            }
        }

        private string getHpGraphFormatted()
        {
            // todo: implement, maybe?
            return string.Empty;
        }
    }
}
