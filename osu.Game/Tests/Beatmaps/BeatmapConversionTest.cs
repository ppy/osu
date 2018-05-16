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
using osu.Game.Rulesets;
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

        protected void Test(string name)
        {
            var ourResult = convert(name);
            var expectedResult = read(name);

            Assert.Multiple(() =>
            {
                int mappingCounter = 0;
                while (true)
                {
                    if (mappingCounter >= ourResult.Mappings.Count && mappingCounter >= expectedResult.Mappings.Count)
                        break;
                    if (mappingCounter >= ourResult.Mappings.Count)
                        Assert.Fail($"A conversion did not generate any hitobjects, but should have, for hitobject at time: {expectedResult.Mappings[mappingCounter].StartTime}\n");
                    else if (mappingCounter >= expectedResult.Mappings.Count)
                        Assert.Fail($"A conversion generated hitobjects, but should not have, for hitobject at time: {ourResult.Mappings[mappingCounter].StartTime}\n");
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
                                    Assert.Fail($"The conversion did not generate a hitobject, but should have, for hitobject at time: {expectedMapping.StartTime}:\n"
                                                + $"Expected: {JsonConvert.SerializeObject(expectedMapping.Objects[objectCounter])}\n");
                                else if (objectCounter >= expectedMapping.Objects.Count)
                                    Assert.Fail($"The conversion generated a hitobject, but should not have, for hitobject at time: {ourMapping.StartTime}:\n"
                                                + $"Received: {JsonConvert.SerializeObject(ourMapping.Objects[objectCounter])}\n");
                                else if (!EqualityComparer<TConvertValue>.Default.Equals(expectedMapping.Objects[objectCounter], ourMapping.Objects[objectCounter]))
                                {
                                    Assert.Fail($"The conversion generated differing hitobjects for object at time: {expectedMapping.StartTime}\n"
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

        private ConvertResult convert(string name)
        {
            var beatmap = getBeatmap(name);

            var rulesetInstance = CreateRuleset();
            beatmap.BeatmapInfo.Ruleset = beatmap.BeatmapInfo.RulesetID == rulesetInstance.RulesetInfo.ID ? rulesetInstance.RulesetInfo : new RulesetInfo();

            var result = new ConvertResult();

            var converter = rulesetInstance.CreateBeatmapConverter(beatmap);
            converter.ObjectConverted += (orig, converted) =>
            {
                converted.ForEach(h => h.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.BaseDifficulty));

                var mapping = new ConvertMapping { StartTime = orig.StartTime };
                foreach (var obj in converted)
                    mapping.Objects.AddRange(CreateConvertValue(obj));
                result.Mappings.Add(mapping);
            };

            converter.Convert();

            return result;
        }

        private ConvertResult read(string name)
        {
            using (var resStream = openResource($"{resource_namespace}.{name}{expected_conversion_suffix}.json"))
            using (var reader = new StreamReader(resStream))
            {
                var contents = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<ConvertResult>(contents);
            }
        }

        private IBeatmap getBeatmap(string name)
        {
            using (var resStream = openResource($"{resource_namespace}.{name}.osu"))
            using (var stream = new StreamReader(resStream))
            {
                var decoder = Decoder.GetDecoder<Beatmap>(stream);
                ((LegacyBeatmapDecoder)decoder).ApplyOffsets = false;
                return decoder.Decode(stream);
            }
        }

        private Stream openResource(string name)
        {
            var localPath = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
            return Assembly.LoadFrom(Path.Combine(localPath, $"{ResourceAssembly}.dll")).GetManifestResourceStream($@"{ResourceAssembly}.Resources.{name}");
        }

        protected abstract IEnumerable<TConvertValue> CreateConvertValue(HitObject hitObject);
        protected abstract Ruleset CreateRuleset();

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
