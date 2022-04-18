// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.API.Requests
{
    /// <summary>
    /// Represents a pagination data used for <see cref="PaginatedAPIRequest{T}"/>.
    /// </summary>
    public readonly struct Pagination
    {
        /// <summary>
        /// The starting point of the request.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// The maximum number of items to return in this request.
        /// </summary>
        public int Limit { get; }

        public Pagination(int offset, int limit)
        {
            Offset = offset;
            Limit = limit;
        }

        public Pagination(int limit)
            : this(0, limit)
        {
        }

        /// <summary>
        /// Returns a <see cref="Pagination"/> of the next number of items defined by <paramref name="limit"/> after this.
        /// </summary>
        /// <param name="limit">The limit of the next pagination.</param>
        public Pagination TakeNext(int limit) => new Pagination(Offset + Limit, limit);
    }
}
