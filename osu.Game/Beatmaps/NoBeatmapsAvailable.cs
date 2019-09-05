// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Beatmaps
{
    public class NoBeatmapsAvailableWorkingBeatmap : DummyWorkingBeatmap
    {
        protected override string Name => "no beatmaps available!";
        protected override string Description => "please load a beatmap!";

        public NoBeatmapsAvailableWorkingBeatmap(AudioManager audio, TextureStore textures)
            : base(audio, textures)
        {
        }
    }
}
