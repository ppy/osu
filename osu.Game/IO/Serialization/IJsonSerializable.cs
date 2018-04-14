// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Game.IO.Serialization.Converters;

namespace osu.Game.IO.Serialization
{
    public interface IJsonSerializable
    {
    }

    public static class JsonSerializableExtensions
    {
        public static string Serialize(this IJsonSerializable obj) => JsonConvert.SerializeObject(obj, CreateGlobalSettings());

        public static T Deserialize<T>(this string objString) => JsonConvert.DeserializeObject<T>(objString, CreateGlobalSettings());

        public static void DeserializeInto<T>(this string objString, T target) => JsonConvert.PopulateObject(objString, target, CreateGlobalSettings());

        public static T DeepClone<T>(this T obj) where T : IJsonSerializable => Deserialize<T>(Serialize(obj));

        /// <summary>
        /// Creates the default <see cref="JsonSerializerSettings"/> that should be used for all <see cref="IJsonSerializable"/>s.
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings CreateGlobalSettings() => new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            Converters = new JsonConverter[] { new Vector2Converter() },
            ContractResolver = new KeyContractResolver()
        };
    }
}
