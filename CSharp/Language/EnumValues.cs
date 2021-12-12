﻿// Copyright (c) 2021 Fabio Iotti
// The copyright holders license this file to you under the MIT license,
// available at https://github.com/bruce965/snippets/raw/master/LICENSE

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utility
{
    public static class EnumValues
    {
        class EnumValues<T> : IReadOnlyList<T>
            where T : Enum
        {
            static readonly T[] values
                = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

            public static EnumValues<T> Instance
                = new EnumValues<T>();

            public int Count => values.Length;

            public T this[int index]
                => values[index];

            EnumValues() { }

            public IEnumerator<T> GetEnumerator()
                => ((IEnumerable<T>)values).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => values.GetEnumerator();
        }

        /// <summary>
        /// Read-only equivalent of <see cref="Enum.GetValues(Type)"/>, without temporary allocations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReadOnlyList<T> ValuesOf<T>() where T : Enum
            => EnumValues<T>.Instance;
    }
}
