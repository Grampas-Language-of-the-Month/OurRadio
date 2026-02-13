using Microsoft.AspNetCore.SignalR;
using OurRadio.Data;
using OurRadio.Models;
using OurRadio.Services;

namespace OurRadio.Hubs;

public sealed class RadioHub : Hub
{
    private readonly RadioClockService _clock;
    private readonly RadioService _radioService;

    public RadioHub(RadioClockService clock, RadioService radioService)
    {
        _clock = clock;
        _radioService = radioService;
    }

    public async Task<RadioPlaybackState?> JoinRadio(int radioId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, RadioClockService.GetGroup(radioId));
        _clock.EnsureRadioRunning(radioId);
        return _clock.GetState(radioId);
    }

    public Task LeaveRadio(int radioId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, RadioClockService.GetGroup(radioId));

    public int[] GetQueue(int radioId)
        => _clock.GetQueueSnapshot(radioId);

    public async Task QueueSong(int radioId, int songId)
    {
        var isInRadio = await _radioService.IsSongInRadioAsync(radioId, songId);
        if (!isInRadio)
        {
            return;
        }

        _clock.EnsureRadioRunning(radioId);
        await _clock.EnqueueSongAsync(radioId, songId, Context.ConnectionAborted);
    }
}
