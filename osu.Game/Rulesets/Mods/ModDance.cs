using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDance<T> : ModDance, IApplicableToDrawableRuleset<T>
        where T : HitObject
    {
        //Copied from ModAutoplay.cs
        public virtual void ApplyToDrawableRuleset(DrawableRuleset<T> drawableRuleset)
        {
            drawableRuleset.SetReplayScore(CreateReplayScore(drawableRuleset.Beatmap, drawableRuleset.Mods));
        }
        //Copy end
    }

    public abstract class ModDance : Mod
    {
        [SettingSource("保存Dance回放")]
        public Bindable<bool> SaveScore { get; } = new BindableBool();

        public override ModType Type => ModType.Automation;

        //Copied from ModAutoplay.cs
        [Obsolete("Use the mod-supporting override")] // can be removed 20210731
        public virtual Score CreateReplayScore(IBeatmap beatmap) => new Score { Replay = new Replay() };

#pragma warning disable 618
        public virtual Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => CreateReplayScore(beatmap);
#pragma warning restore 618
        //Copy end
    }
}
