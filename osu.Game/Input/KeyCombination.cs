// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Input;

namespace osu.Game.Input
{
    /// <summary>
    /// Represent a combination of more than one <see cref="Key"/>s.
    /// </summary>
    public class KeyCombination : IEquatable<KeyCombination>
    {
        public readonly IEnumerable<Key> Keys;

        public KeyCombination(params Key[] keys)
        {
            Keys = keys;
        }

        public KeyCombination(IEnumerable<Key> keys)
        {
            Keys = keys;
        }

        public KeyCombination(string stringRepresentation)
        {
            Keys = stringRepresentation.Split(',').Select(s => (Key)int.Parse(s));
        }

        public bool CheckValid(IEnumerable<Key> keys, bool requireExactMatch = false)
        {
            if (requireExactMatch)
                return Keys.SequenceEqual(keys);
            else
                return !Keys.Except(keys).Any();
        }

        public bool Equals(KeyCombination other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Keys.SequenceEqual(other.Keys);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KeyCombination)obj);
        }

        public override int GetHashCode() => Keys != null ? Keys.Select(k => k.GetHashCode()).Aggregate((h1, h2) => h1 + h2) : 0;

        public static implicit operator KeyCombination(Key singleKey) => new KeyCombination(singleKey);

        public static implicit operator KeyCombination(string stringRepresentation) => new KeyCombination(stringRepresentation);

        public static implicit operator KeyCombination(Key[] keys) => new KeyCombination(keys);

        public override string ToString() => Keys.Select(k => ((int)k).ToString()).Aggregate((s1, s2) => $"{s1},{s2}");
    }
}