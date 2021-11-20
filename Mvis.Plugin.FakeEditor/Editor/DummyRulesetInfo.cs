using System;
using System.Collections.Generic;
using System.Threading;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace Mvis.Plugin.FakeEditor.Editor
{
    /// <summary>
    /// From osu.Game/Beatmaps/DummyWorkingBeatmap.cs
    /// <see cref="osu.Game.Beatmaps.DummyWorkingBeatmap"/>
    /// </summary>
    public class DummyRulesetInfo : RulesetInfo
    {
        public override Ruleset CreateInstance() => new DummyRuleset();

        private class DummyRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => Array.Empty<Mod>();

            public override ISkin CreateLegacySkinProvider(ISkin skin, IBeatmap beatmap) => new DummySkinProvider();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            {
                throw new NotImplementedException();
            }

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap)
                => new DummyBeatmapConverter { Beatmap = beatmap };

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => null;

            public override string Description => "desc";

            public override string ShortName => "name";

            private class DummyBeatmapConverter : IBeatmapConverter
            {
                public event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;

                public IBeatmap Beatmap { get; set; }

                public bool CanConvert() => true;

                public IBeatmap Convert(CancellationToken cancellationToken = default)
                {
                    foreach (var obj in Beatmap.HitObjects)
                        ObjectConverted?.Invoke(obj, obj.Yield());

                    return Beatmap;
                }
            }
        }
    }
}
