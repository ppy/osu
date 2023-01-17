// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Database
{
    public struct ImportParameters
    {
        /// <summary>
        /// Whether this import is part of a larger batch.
        /// </summary>
        /// <remarks>
        /// May skip intensive pre-import checks in favour of faster processing.
        ///
        /// More specifically, imports will be skipped before they begin, given an existing model matches on hash and filenames. Should generally only be used for large batch imports, as it may defy user expectations when updating an existing model.
        ///
        /// Will also change scheduling behaviour to run at a lower priority.
        /// </remarks>
        public bool Batch { get; set; }

        /// <summary>
        /// Whether this import should use hard links rather than file copy operations if available.
        /// </summary>
        public bool PreferHardLinks { get; set; }
    }
}
