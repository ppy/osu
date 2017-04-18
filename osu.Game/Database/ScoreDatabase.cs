// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.IO.Legacy;
using osu.Game.IPC;
using osu.Game.Rulesets.Scoring;
using SharpCompress.Compressors.LZMA;
using SQLite.Net;

namespace osu.Game.Database
{
    public class ScoreDatabase : Database
    {
        private readonly Storage storage;

        private readonly BeatmapDatabase beatmaps;
        private readonly RulesetDatabase rulesets;

        private const string replay_folder = @"replays";

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private ScoreIPCChannel ipc;

        public ScoreDatabase(Storage storage, SQLiteConnection connection, IIpcHost importHost = null, BeatmapDatabase beatmaps = null, RulesetDatabase rulesets = null) : base(storage, connection)
        {
            this.storage = storage;
            this.beatmaps = beatmaps;
            this.rulesets = rulesets;

            if (importHost != null)
                ipc = new ScoreIPCChannel(importHost, this);
        }

        public Score ReadReplayFile(string replayFilename)
        {
            Score score;

            using (Stream s = storage.GetStream(Path.Combine(replay_folder, replayFilename)))
            using (SerializationReader sr = new SerializationReader(s))
            {
                var ruleset = rulesets.GetRuleset(sr.ReadByte()).CreateInstance();
                score = ruleset.CreateScoreProcessor().CreateScore();

                /* score.Pass = true;*/
                var version = sr.ReadInt32();
                /* score.FileChecksum = */
                var beatmapHash = sr.ReadString();
                score.Beatmap = beatmaps.Query<BeatmapInfo>().FirstOrDefault(b => b.Hash == beatmapHash);
                /* score.PlayerName = */
                sr.ReadString();
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
                sr.ReadInt32();
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
                        throw new Exception("input .lzma is too short");
                    long outSize = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        int v = replayInStream.ReadByte();
                        if (v < 0)
                            throw new Exception("Can't Read 1");
                        outSize |= (long)(byte)v << (8 * i);
                    }

                    long compressedSize = replayInStream.Length - replayInStream.Position;

                    using (var lzma = new LzmaStream(properties, replayInStream, compressedSize, outSize))
                    using (var reader = new StreamReader(lzma))
                        score.Replay = score.CreateReplay(reader);
                }
            }

            return score;
        }

        protected override void Prepare(bool reset = false)
        {
        }

        protected override Type[] ValidTypes => new[] { typeof(Score) };
    }
}
