// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.IO.Legacy
{
    public interface ILegacySerializable
    {
        void ReadFromStream(SerializationReader sr);
        void WriteToStream(SerializationWriter sw);
    }
}
