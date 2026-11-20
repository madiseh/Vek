// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Blazored.LocalStorage;
using Vek.Docs.NotificationContent;

namespace Vek.Docs.Services.Notifications;

public class InMemoryNotificationService(ILocalStorageService localStorageService) : INotificationService
{
    private const string LocalStorageKey = "__notficationTimestamp";

    private readonly List<NotificationMessage> _messages = [];

    private async Task<DateTime> GetLastReadTimestamp()
    {
        if (!await localStorageService.ContainKeyAsync(LocalStorageKey))
        {
            return DateTime.MinValue;
        }

        var timestamp = await localStorageService.GetItemAsync<DateTime>(LocalStorageKey);
        return timestamp;
    }

    public async Task<bool> AreNewNotificationsAvailable()
    {
        var timestamp = await GetLastReadTimestamp();
        var entriesFound = _messages.Any(x => x.PublishDate > timestamp);

        return entriesFound;
    }

    public async Task MarkNotificationsAsRead()
    {
        await localStorageService.SetItemAsync(LocalStorageKey, DateTime.UtcNow.Date);
    }

    public async Task MarkNotificationsAsRead(string id)
    {
        var message = await GetMessageById(id);
        if (message == null) { return; }

        var timestamp = await localStorageService.GetItemAsync<DateTime>(LocalStorageKey);
        if (message.PublishDate > timestamp)
        {
            await localStorageService.SetItemAsync(LocalStorageKey, message.PublishDate);
        }

    }

    public Task<NotificationMessage> GetMessageById(string id) =>
        Task.FromResult(_messages.FirstOrDefault(x => x.Id == id));

    public async Task<IDictionary<NotificationMessage, bool>> GetNotifications()
    {
        var lastReadTimestamp = await GetLastReadTimestamp();
        var items = _messages.ToDictionary(x => x, x => lastReadTimestamp > x.PublishDate);
        return items;
    }

    public Task AddNotification(NotificationMessage message)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    public void Preload()
    {
        _messages.Add(new NotificationMessage(
            nameof(Announcement_v9_GA),
            "Vek v9.0.0 Released",
            "Major Version",
            "Announcement",
            new DateTime(2026, 03, 01),
            "https://github.com/madiseh/vek/blob/f979c2c84e3ddd5f01a20ebc1102838d32a4b01b/content/Nuget.png",
            [
                new NotificationAuthor("The Vek Team", "https://github.com/madiseh/vek_content/Vek.Docs/images/logo.png")
            ], typeof(Announcement_v9_GA)));

        _messages.Add(new NotificationMessage(
            nameof(Announcement_v8_GA),
            "Vek v8.0.0 Released",
            "Major Version",
            "Announcement",
            new DateTime(2025, 01, 19),
            "https://github.com/madiseh/vek/blob/f979c2c84e3ddd5f01a20ebc1102838d32a4b01b/content/Nuget.png",
            [
                new NotificationAuthor("The Vek Team", "https://github.com/madiseh/vek_content/Vek.Docs/images/logo.png")
            ], typeof(Announcement_v8_GA)));

        _messages.Add(new NotificationMessage(
            nameof(Announcement_v7_GA),
            "Vek v7.0.0 Released",
            "Major Version",
            "Announcement",
            new DateTime(2024, 06, 29),
            "https://github.com/madiseh/vek/blob/f979c2c84e3ddd5f01a20ebc1102838d32a4b01b/content/Nuget.png",
            [
                new NotificationAuthor("The Vek Team", "https://github.com/madiseh/vek_content/Vek.Docs/images/logo.png")
            ], typeof(Announcement_v7_GA)));

        _messages.Add(new NotificationMessage(
            "vek-here-to-stay",
            "Vek is here to stay",
            "We are paving the way for the future of Blazor",
            "Announcement",
            new DateTime(2022, 01, 13),
                    "_content/Vek.Docs/images/announcements/vek_heretostay.svg",
            [
                new NotificationAuthor("Jonny Larsson",
                    "https://avatars.githubusercontent.com/u/10367109?v=4")
            ], typeof(Announcement_VekIsHereToStay)));
    }
}
