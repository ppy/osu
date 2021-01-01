// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.IO.Serialization;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// A class that allows for deciding between binding to an immutable base bindable, or a mutable custom bindable.
    /// All determined by <see cref="HasCustomValue"/>'s current value, and the final bindable bound to the determined source is exposed as <see cref="FinalValue"/>.
    /// </summary>
    /// <typeparam name="T">The value type for all bindables.</typeparam>
    public class OverridableBindable<T> : IParseable, ISerializableOverridable
        where T : struct, IComparable<T>, IConvertible, IEquatable<T>
    {
        /// <summary>
        /// The leased instance of the base bindable, <see cref="FinalValue"/> will bind to the original immutable instance of this.
        /// </summary>
        public Bindable<T> BaseValue => baseValue;

        /// <summary>
        /// The bindable holding the custom value, this will revert to <see cref="BaseValue"/>'s current value when <see cref="HasCustomValue"/> is set to false.
        /// </summary>
        public Bindable<T> CustomValue => customValue;

        /// <summary>
        /// The final bindable bound to either of the two bindables according to <see cref="HasCustomValue"/>.
        /// </summary>
        public Bindable<T> FinalValue => finalValue;

        /// <summary>
        /// Whether the final bindable should be bound to <see cref="CustomValue"/>, otherwise will be immutably bound to <see cref="BaseValue"/>.
        /// </summary>
        public Bindable<bool> HasCustomValue { get; } = new BindableBool();

        private readonly BindableNumber<T> immutableBaseValue = new BindableNumber<T>();
        private readonly BindableNumber<T> customValue = new BindableNumber<T>();
        private readonly BindableNumberWithCurrent<T> finalValue = new BindableNumberWithCurrent<T>();

        private readonly LeasedBindable<T> baseValue;

        public OverridableBindable(T? defaultValue = null, T? minValue = null, T? maxValue = null, T? precision = null)
        {
            if (defaultValue != null)
            {
                immutableBaseValue.Value = customValue.Value = finalValue.Value = defaultValue.Value;
                immutableBaseValue.Default = customValue.Default = finalValue.Default = defaultValue.Value;
            }

            if (minValue != null)
                immutableBaseValue.MinValue = customValue.MinValue = finalValue.MinValue = minValue.Value;

            if (maxValue != null)
                immutableBaseValue.MaxValue = customValue.MaxValue = finalValue.MaxValue = maxValue.Value;

            if (precision != null)
                immutableBaseValue.Precision = customValue.Precision = finalValue.Precision = precision.Value;

            // this lease is never returned on purpose.
            // the leased bindable is exposed publicly to be mutable from outside,
            // while the inner one is immutable, so that updateFinalValue() can bind to it if HasCustomValue = false,
            // therefore ensuring that FinalValue is disabled.
            baseValue = immutableBaseValue.BeginLease(false);

            HasCustomValue.BindValueChanged(c =>
            {
                finalValue.Current = c.NewValue ? customValue : immutableBaseValue;

                // inherit base value into custom value when HasCustomValue false, better UX for slider when re-enabling.
                if (!c.NewValue)
                    customValue.Value = baseValue.Value;
            }, true);
        }

        /// <summary>
        /// Parameterless constructor for serialization.
        /// </summary>
        [UsedImplicitly]
        private OverridableBindable()
            : this(null)
        {
        }

        /// <summary>
        /// Parses an input into <see cref="CustomValue"/> and sets <see cref="HasCustomValue"/> accordingly.
        /// </summary>
        /// <param name="input">The input which is to be parsed.</param>
        public void Parse(object input)
        {
            switch (input)
            {
                case null:
                    HasCustomValue.Value = false;
                    return;

                case OverridableBindable<T> setting:
                    HasCustomValue.Value = setting.HasCustomValue.Value;
                    if (HasCustomValue.Value)
                        CustomValue.Value = setting.CustomValue.Value;

                    break;

                default:
                    CustomValue.Parse(input);
                    HasCustomValue.Value = true;
                    break;
            }
        }

        void ISerializableOverridable.SerializeTo(JsonWriter writer, JsonSerializer serializer)
        {
            if (!HasCustomValue.Value)
            {
                writer.WriteNull();
                return;
            }

            serializer.Serialize(writer, customValue);
        }

        void ISerializableOverridable.DeserializeFrom(JsonReader reader, JsonSerializer serializer)
        {
            Parse(serializer.Deserialize(reader));
        }
    }

    [JsonConverter(typeof(OverridableJsonConverter))]
    internal interface ISerializableOverridable : IJsonSerializable
    {
        void SerializeTo(JsonWriter writer, JsonSerializer serializer);
        void DeserializeFrom(JsonReader reader, JsonSerializer serializer);
    }

    internal class OverridableJsonConverter : JsonConverter<ISerializableOverridable>
    {
        public override void WriteJson(JsonWriter writer, ISerializableOverridable value, JsonSerializer serializer)
        {
            value.SerializeTo(writer, serializer);
        }

        public override ISerializableOverridable ReadJson(JsonReader reader, Type objectType, ISerializableOverridable existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = existingValue ?? (ISerializableOverridable)Activator.CreateInstance(objectType, true);
            value.DeserializeFrom(reader, serializer);
            return value;
        }
    }
}
