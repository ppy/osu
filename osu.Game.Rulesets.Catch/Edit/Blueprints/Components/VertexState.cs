// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;

#nullable enable

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public class VertexState
    {
        public bool IsSelected { get; set; }

        public bool IsFixed { get; set; }

        public JuiceStreamPathVertex VertexBeforeChange { get; set; }
    }
}
