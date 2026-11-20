// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Docs.Services.Notifications;

namespace Vek.Docs.Pages.About.Announcements;

public partial class AnnoucementPage
{
    [Parameter] public string Id { get; set; }
    [Inject] public INotificationService NotificationService { get; set; }

    private NotificationMessage _message;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        _message = await NotificationService.GetMessageById(Id);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await NotificationService.MarkNotificationsAsRead(Id);
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
