using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Edit.Tools
{
    public class HitObjectCompositionTool<T> : ICompositionTool
        where T : HitObject
    {
        public string Name => typeof(T).Name;
    }
}
