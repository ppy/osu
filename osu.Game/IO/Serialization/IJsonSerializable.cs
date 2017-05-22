// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;

namespace osu.Game.IO.Serialization
{
    public interface IJsonSerializable
    {
    }

    public static class JsonSerializableExtensions
    {
        public static string Serialize(this IJsonSerializable obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static T Deserialize<T>(this string objString)
        {
            return JsonConvert.DeserializeObject<T>(objString);
        }

        public static T DeepClone<T>(this T obj)
            where T : IJsonSerializable
        {
            return Deserialize<T>(Serialize(obj));
        }
    }
}
