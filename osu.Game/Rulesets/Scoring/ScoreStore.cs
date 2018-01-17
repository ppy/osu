// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Legacy;
using osu.Game.IPC;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;
using SharpCompress.Compressors.LZMA;

namespace osu.Game.Rulesets.Scoring
{
    public class ScoreStore : DatabaseBackedStore
    {
        private readonly Storage storage;

        private readonly BeatmapManager beatmaps;
        private readonly RulesetStore rulesets;

        private const string replay_folder = @"replays";

        // ReSharper disable once NotAccessedField.Local (we should keep a reference to this so it is not finalised)
        private ScoreIPCChannel ipc;

        public ScoreStore(Storage storage, Func<OsuDbContext> factory, IIpcHost importHost = null, BeatmapManager beatmaps = null, RulesetStore rulesets = null) : base(factory)
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
                score = new Score
                {
                    Ruleset = rulesets.GetRuleset(sr.ReadByte())
                };

                /* score.Pass = true;*/
                var version = sr.ReadInt32();
                /* score.FileChecksum = */
                var beatmapHash = sr.ReadString();
                score.Beatmap = beatmaps.QueryBeatmap(b => b.MD5Hash == beatmapHash);
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
                        score.Replay = createLegacyReplay(reader);
                        score.Replay.User = score.User;
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Creates a legacy replay which is read from a stream.
        /// </summary>
        /// <param name="reader">The stream reader.</param>
        /// <returns>The legacy replay.</returns>
        private Replay createLegacyReplay(StreamReader reader)
        {
            var frames = new List<ReplayFrame>();

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

                frames.Add(new ReplayFrame(
                    lastTime,
                    float.Parse(split[1]),
                    float.Parse(split[2]),
                    (ReplayButtonState)int.Parse(split[3])
                ));
            }

            return new Replay { Frames = frames };
        }
    }
}
