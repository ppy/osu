using System.Collections.Generic;

namespace osu.Game.IO
{
    public interface IHasFiles<TFile>
    {
        List<TFile> Files { get; set; }
    }
}
