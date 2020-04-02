// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Reflection;
using JetBrains.Annotations;
using osu.Framework.Bindables;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An attribute to be assigned to skin configuration settings (enum values) to
    /// provide a fallback value in case the requested setting does not have a value.
    /// The fallback can be either a value to be returned or another setting to retrieve the value from.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SkinSettingFallbackAttribute : Attribute
    {
        public SkinSettingFallbackType FallbackType { get; }

        public object FallbackValue { get; }

        public SkinSettingFallbackAttribute(SkinSettingFallbackType fallbackType, object fallbackValue)
        {
            FallbackType = fallbackType;
            FallbackValue = fallbackValue;
        }
    }

    public enum SkinSettingFallbackType
    {
        /// <summary>
        /// The fallback is to a default value.
        /// </summary>
        Value,

        /// <summary>
        /// The fallback is to another configuration setting.
        /// </summary>
        Setting,
    }

    public static class SkinSettingFallbackExtensions
    {
        /// <summary>
        /// Retrieve a configuration value, if unavailable a fallback value or
        /// the default value of <typeparamref cref="TValue"/> will be returned.
        /// </summary>
        /// <param name="skin">The skin to retrieve the requested configuration value from.</param>
        /// <param name="lookup">The requested configuration lookup value.</param>
        /// <returns>A matching value, fallback value or default value of <typeparamref cref="TValue"/> boxed in an <see cref="IBindable{TValue}"/>.</returns>
        [NotNull]
        public static IBindable<TValue> GetConfigOrDefault<TLookup, TValue>(this ISkin skin, TLookup lookup)
        {
            var setting = skin.GetConfig<TLookup, TValue>(lookup);
            if (setting != null)
                return setting;

            var attr = lookup.GetType().GetField(lookup.ToString()).GetCustomAttribute<SkinSettingFallbackAttribute>();

            // The setting does not have a fallback value, return default value of TValue.
            if (attr == null)
                return new Bindable<TValue>();

            switch (attr.FallbackType)
            {
                case SkinSettingFallbackType.Setting:
                    // Setup the configuration value retrieval method (GetConfigOrDefault) to the lookup type of the fallback setting.
                    var method = typeof(SkinSettingFallbackExtensions).GetMethod(nameof(GetConfigOrDefault));
                    var executableMethod = method?.MakeGenericMethod(attr.FallbackValue.GetType(), typeof(TValue));
                    var returnedValue = (IBindable<TValue>)executableMethod?.Invoke(null, new[] { skin, attr.FallbackValue });

                    if (returnedValue == null)
                        return new Bindable<TValue>();

                    return new Bindable<TValue>(returnedValue.Value ?? default);

                case SkinSettingFallbackType.Value:
                    return SkinUtils.As<TValue>(Activator.CreateInstance(typeof(Bindable<>).MakeGenericType(attr.FallbackValue.GetType()), attr.FallbackValue));

                default:
                    throw new ArgumentOutOfRangeException(nameof(lookup), $"{lookup} provides a fallback of unknown type.");
            }
        }
    }
}
