// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.IO;
using osu.Game.IO.Legacy;

namespace osu.Game.Modes.Replays
{
    /// <summary>
    /// Reads a replay from a legacy replay file (.osr v1)
    /// </summary>
    public class LegacyReplay : FramedReplay
    {
        public LegacyReplay(StreamReader reader)
        {
            float lastTime = 0;

            foreach (var l in reader.ReadToEnd().Split(','))
            {
                var split = l.Split('|');

                if (split.Length < 4 || float.Parse(split[0]) < 0) continue;

                lastTime += float.Parse(split[0]);

                Frames.Add(new ReplayFrame(
                    lastTime,
                    float.Parse(split[1]),
                    384 - float.Parse(split[2]),
                    (ReplayButtonState)int.Parse(split[3])
                    ));
            }
        }

        public class LegacyReplayFrame : ReplayFrame
        {
            public LegacyReplayFrame(Stream s) : this(new SerializationReader(s))
            {
            }

            public LegacyReplayFrame(SerializationReader sr)
            {
                ButtonState = (ReplayButtonState)sr.ReadByte();

                byte bt = sr.ReadByte();
                if (bt > 0)//Handle Pre-Taiko compatible replays.
                    ButtonState = ReplayButtonState.Right1;

                MouseX = sr.ReadSingle();
                MouseY = sr.ReadSingle();
                Time = sr.ReadInt32();
            }

            public void ReadFromStream(SerializationReader sr)
            {
                throw new NotImplementedException();
            }

            public void WriteToStream(SerializationWriter sw)
            {
                sw.Write((byte)ButtonState);
                sw.Write((byte)0);
                sw.Write(MouseX);
                sw.Write(MouseY);
                sw.Write(Time);
            }
        }
    }
}
