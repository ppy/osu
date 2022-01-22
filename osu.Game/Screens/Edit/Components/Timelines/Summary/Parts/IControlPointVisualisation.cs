// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public interface IControlPointVisualisation
    {
        /// <summary>
        /// For display purposes, check whether the proposed point is made redundant by this visualisation.
        /// </summary>
        bool IsVisuallyRedundant(ControlPoint other);
    }
}
