// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Vek
{
    /// <summary>
    /// Assigns a display label to a model property or field so Vek form fields bound with <c>For</c> can show it automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class LabelAttribute : Attribute
    {
        public LabelAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
