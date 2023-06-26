// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;

namespace osu.Game.Rulesets.UI
{
    /// <summary>
    /// Expose the <see cref="ReplayRecorder"/> in a capable <see cref="InputManager"/>.
    /// </summary>
    public interface IHasRecordingHandler
    {
        public ReplayRecorder? Recorder { set; }
    }
}
