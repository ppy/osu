// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public abstract class BeatmapConversionTest<TConvertValue>
        where TConvertValue : IEquatable<TConvertValue>
    {
        private const string resource_namespace = "Testing.Beatmaps";
        private const string expected_conversion_suffix = "-expected-conversion";

        protected abstract string ResourceAssembly { get; }

        protected void Test(int beatmapId)
        {
            var ourResult = convert(beatmapId);
            var expectedResult = read(beatmapId);

            Assert.Multiple(() =>
            {
                int mappingCounter = 0;
                while (true)
                {
                    if (mappingCounter >= ourResult.Mappings.Count && mappingCounter >= expectedResult.Mappings.Count)
                        break;
                    if (mappingCounter >= ourResult.Mappings.Count)
                        Assert.Fail($"Missing conversion for object at time: {expectedResult.Mappings[mappingCounter].StartTime}");
                    else if (mappingCounter >= expectedResult.Mappings.Count)
                        Assert.Fail($"Extra conversion for object at time: {ourResult.Mappings[mappingCounter].StartTime}");
                    else
                    {
                        var counter = mappingCounter;
                        Assert.Multiple(() =>
                        {
                            var ourMapping = ourResult.Mappings[counter];
                            var expectedMapping = expectedResult.Mappings[counter];

                            int objectCounter = 0;
                            while (true)
                            {
                                if (objectCounter >= ourMapping.Objects.Count && objectCounter >= expectedMapping.Objects.Count)
                                    break;
                                if (objectCounter >= ourMapping.Objects.Count)
                                    Assert.Fail($"Expected conversion for object at time: {expectedMapping.StartTime}:\n{JsonConvert.SerializeObject(expectedMapping.Objects[objectCounter])}");
                                else if (objectCounter >= expectedMapping.Objects.Count)
                                    Assert.Fail($"Unexpected conversion for object at time: {ourMapping.StartTime}:\n{JsonConvert.SerializeObject(ourMapping.Objects[objectCounter])}");
                                else if (!EqualityComparer<TConvertValue>.Default.Equals(expectedMapping.Objects[objectCounter], ourMapping.Objects[objectCounter]))
                                {
                                    Assert.Fail($"Converted hitobjects differ for object at time: {expectedMapping.StartTime}\n"
                                                + $"Expected: {JsonConvert.SerializeObject(expectedMapping.Objects[objectCounter])}\n"
                                                + $"Received: {JsonConvert.SerializeObject(ourMapping.Objects[objectCounter])}\n");
                                }

                                objectCounter++;
                            }
                        });
                    }

                    mappingCounter++;
                }
            });
        }

        private ConvertResult convert(int beatmapId)
        {
            var beatmap = getBeatmap(beatmapId);

            var result = new ConvertResult();

            var converter = CreateConverter();
            converter.ObjectConverted += (orig, converted) =>
            {
                converted.ForEach(h => h.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty));

                var mapping = new ConvertMapping { StartTime = orig.StartTime };
                foreach (var obj in converted)
                    mapping.Objects.Add(CreateConvertValue(obj));
                result.Mappings.Add(mapping);
            };

            converter.Convert(beatmap);

            return result;
        }

        private ConvertResult read(int beatmapId)
        {
            using (var resStream = openResource($"{resource_namespace}.{beatmapId}{expected_conversion_suffix}.json"))
            using (var reader = new StreamReader(resStream))
            {
                var contents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<ConvertResult>(contents);
            }
        }

        private Beatmap getBeatmap(int beatmapId)
        {
            var decoder = new LegacyBeatmapDecoder();
            using (var resStream = openResource($"{resource_namespace}.{beatmapId}.osu"))
            using (var stream = new StreamReader(resStream))
                return decoder.DecodeBeatmap(stream);
        }

        private Stream openResource(string name)
        {
            var localPath = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            return Assembly.LoadFrom(Path.Combine(localPath, $"{ResourceAssembly}.dll")).GetManifestResourceStream($@"{ResourceAssembly}.Resources.{name}");
        }

        protected abstract TConvertValue CreateConvertValue(HitObject hitObject);
        protected abstract ITestableBeatmapConverter CreateConverter();

        private class ConvertMapping
        {
            [JsonProperty]
            public double StartTime;
            [JsonProperty]
            public List<TConvertValue> Objects = new List<TConvertValue>();
        }

        private class ConvertResult
        {
            [JsonProperty]
            public List<ConvertMapping> Mappings = new List<ConvertMapping>();
        }
    }
}
