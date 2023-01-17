// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    /// <summary>
    /// Holds the state of a vertex in the path of a <see cref="EditablePath"/>.
    /// </summary>
    public class VertexState
    {
        /// <summary>
        /// Whether the vertex is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether the vertex can be moved or deleted.
        /// </summary>
        public bool IsFixed { get; set; }

        /// <summary>
        /// The position of the vertex before a vertex moving operation starts.
        /// This is used to implement "memory-less" moving operations (only the final position matters) to improve UX.
        /// </summary>
        public JuiceStreamPathVertex VertexBeforeChange { get; set; }
    }
}
