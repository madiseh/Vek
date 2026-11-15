// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.Docs.Services.Notifications;

namespace Vek.Docs.Pages.About.Announcements;

public partial class AnnouncementOverviewPage
{
    private IDictionary<NotificationMessage, bool> _messages;

    [Inject] public INotificationService NotificationService { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _messages = await NotificationService.GetNotifications();
            await NotificationService.MarkNotificationsAsRead();
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }
}
