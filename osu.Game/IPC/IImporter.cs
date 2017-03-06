// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Threading.Tasks;
namespace osu.Game.IPC
{
    public interface IImporter
    {
        Task ImportAsync(string path);
    }
}
