using Microsoft.AspNetCore.SignalR;
using OurRadio.Models;
using OurRadio.Services;

namespace OurRadio.Hubs;

public sealed class RadioHub : Hub
{
    private readonly RadioClockService _clock;

    public RadioHub(RadioClockService clock)
    {
        _clock = clock;
    }

    public async Task<RadioPlaybackState?> JoinRadio(int radioId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, RadioClockService.GetGroup(radioId));
        _clock.EnsureRadioRunning(radioId);
        return _clock.GetState(radioId);
    }

    public Task LeaveRadio(int radioId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, RadioClockService.GetGroup(radioId));
}
