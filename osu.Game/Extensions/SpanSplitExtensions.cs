// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Extensions
{
    internal static class SpanSplitExtensions
    {
        public static SpanSplitEnumerator<T> Split<T>(this ReadOnlySpan<T> input, T separator)
            where T : IEquatable<T>
            => new SpanSplitEnumerator<T>(input, separator);
    }

    public ref struct SpanSplitEnumerator<T>
        where T : IEquatable<T>
    {
        // API shape inspired by https://github.com/dotnet/runtime/issues/934#issuecomment-1165864043

        private readonly ReadOnlySpan<T> value;
        private readonly T separator;
        private int currentFrom;
        private int nextIndex = -1;

        public SpanSplitEnumerator(ReadOnlySpan<T> value, T separator)
        {
            this.value = value;
            this.separator = separator;
        }

        // pattern-based foreach support
        public readonly SpanSplitEnumerator<T> GetEnumerator() => this;

        public bool MoveNext()
        {
            if (value.IsEmpty)
                return false;

            if (nextIndex == value.Length)
            {
                currentFrom = value.Length;
                return false;
            }

            currentFrom = nextIndex + 1;
            int index = value[currentFrom..].IndexOf(separator);
            nextIndex = index != -1 ? index + currentFrom : value.Length;
            return true;
        }

        public readonly Range CurrentRange => currentFrom..nextIndex;

        // foreach support
        public void Dispose() { }

        public readonly ReadOnlySpan<T> Current => value[CurrentRange];

        public readonly ReadOnlySpan<T> RemainingSpan => nextIndex < value.Length ? value[(nextIndex + 1)..] : value[^0..];
    }
}
