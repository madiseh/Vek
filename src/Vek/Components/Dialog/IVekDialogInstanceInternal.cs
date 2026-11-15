// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Vek;

internal interface IVekDialogInstanceInternal : IVekDialogInstance
{
    /// <summary>
    /// Links a dialog with this instance.
    /// </summary>
    /// <param name="dialog">The dialog to use.</param>
    /// <remarks>
    /// This method is used internally when displaying a new dialog.
    /// </remarks>
    void Register(VekDialog dialog);
}
