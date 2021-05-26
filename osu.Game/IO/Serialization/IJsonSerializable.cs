// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.IO.Serialization;

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

        /// <summary>
        /// Creates the default <see cref="JsonSerializerSettings"/> that should be used for all <see cref="IJsonSerializable"/>s.
        /// </summary>
        public static JsonSerializerSettings CreateGlobalSettings() => new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            Converters = new List<JsonConverter> { new Vector2Converter() },
            ContractResolver = new KeyContractResolver()
        };
    }
}
