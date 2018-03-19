using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace Symcol.Rulesets.Core.Skinning
{
    public class SkinConfigReader<T> : IniConfigManager<T>
        where T : struct
    {
        protected override string Filename => @"skin.ini";

        public SkinConfigReader(Storage storage) : base(storage) { }

        protected override bool PerformSave() { return false; }
    }

    //wildly incomplete
    public enum ClassicIniParameters
    {
        Name,
        Author,
        CursorRotate,
        CursorExpand,
        CursorCentre
    }
}
