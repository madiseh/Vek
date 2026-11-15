// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Vek.Docs.Pages.Features.Icons
{
    internal class IconStorage : IEnumerable<KeyValuePair<string, Type>>
    {
        private readonly IDictionary<string, Type> _icons;

        public IconStorage()
        {
            _icons = new Dictionary<string, Type>();
        }

        public Type this[string key] => _icons[key];

        public void Add(string iconType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
        {
            _icons.Add(iconType, type);
        }

        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator()
        {
            return _icons.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _icons.GetEnumerator();
        }
    }
}
