// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// An interface representing a filter operation which can be run on a <see cref="Carousel{T}"/>.
    /// </summary>
    public interface ICarouselFilter
    {
        /// <summary>
        /// Execute the filter operation.
        /// </summary>
        /// <param name="items">The items to be filtered.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The post-filtered items.</returns>
        Task<IEnumerable<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken);
    }
}
