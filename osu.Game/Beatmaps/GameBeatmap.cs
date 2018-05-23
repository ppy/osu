// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A <see cref="Bindable{WorkingBeatmap}"/> for the <see cref="OsuGame"/> beatmap.
    /// This should be used sparingly in-favour of <see cref="IGameBeatmap"/>.
    /// </summary>
    public class GameBeatmap : NonNullableBindable<WorkingBeatmap>, IGameBeatmap
    {
        public GameBeatmap(WorkingBeatmap defaultValue)
            : base(defaultValue)
        {
        }

        public GameBeatmap GetBoundCopy()
        {
            var copy = new GameBeatmap(Default);
            copy.BindTo(this);
            return copy;
        }
    }
}
