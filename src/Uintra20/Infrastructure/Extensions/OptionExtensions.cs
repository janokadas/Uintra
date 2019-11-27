﻿using System.Runtime.Caching;
using LanguageExt;
using static LanguageExt.Prelude;

namespace Uintra20.Infrastructure.Extensions
{
    public static class OptionExtensions
    {
        public static T DefaultIfNone<T>(this Option<T> option) where T : class =>
            option.IfNoneUnsafe(default(T));

        public static Option<TDestination> CastOrNone<TDestination>(this object value)
            where TDestination : class
            => Optional(value as TDestination);

        public static Option<T> ToOption<T>(this T value) =>
            Optional(value);

        public static Option<T> GetOrNone<T>(this MemoryCache cache, string key) where T : class =>
            cache.Get(key) as T;
    }
}