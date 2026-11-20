// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek.UnitTests;

public static class DialogExtensions
{
    public static VekDialogContainer GetDialogContainer(this IVekDialogInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (instance is VekDialogContainer container)
        {
            return container;
        }

        throw new InvalidOperationException("Dialog instance not found!");
    }
}
