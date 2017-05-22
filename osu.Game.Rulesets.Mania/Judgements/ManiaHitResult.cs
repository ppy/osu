using System.ComponentModel;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public enum ManiaHitResult
    {
        [Description("PERFECT")]
        Perfect,
        [Description("GREAT")]
        Great,
        [Description("GOOD")]
        Good,
        [Description("OK")]
        Ok,
        [Description("BAD")]
        Bad
    }
}