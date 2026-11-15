// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vek.Interfaces;

namespace Vek.Utilities
{
    /// <summary>
    /// Carries the form field and its new value when a field managed by <see cref="VekForm"/> reports a change.
    /// </summary>
    public class FormFieldChangedEventArgs
    {
        public IFormComponent? Field { get; set; }
        public object? NewValue { get; set; }
    }
}
