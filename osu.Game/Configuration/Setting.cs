// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Configuration
{
    /// <summary>
    /// A binding of a <see cref="Bindings.KeyCombination"/> to an action.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// The combination of keys which will trigger this binding.
        /// </summary>
        public object Key;

        /// <summary>
        /// The resultant action which is triggered by this binding.
        /// </summary>
        public object Value;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="key">The combination of keys which will trigger this binding.</param>
        /// <param name="action">The resultant action which is triggered by this binding. Usually an enum type.</param>
        public Setting(object key, object value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Constructor for derived classes that may require serialisation.
        /// </summary>
        public Setting()
        {
        }

        public override string ToString() => $"{Key}=>{Value}";
    }
}
