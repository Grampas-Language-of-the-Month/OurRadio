using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using OurRadio.Data;
using OurRadio.Hubs;
using OurRadio.Models;

namespace OurRadio.Services;

public sealed class RadioClockService
{
    private readonly ConcurrentDictionary<int, RadioPlaybackState> _states = new();
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _loops = new();
    private readonly ConcurrentDictionary<int, ConcurrentQueue<int>> _queues = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<RadioHub> _hub;

    public RadioClockService(IServiceScopeFactory scopeFactory, IHubContext<RadioHub> hub)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
    }

    public RadioPlaybackState? GetState(int radioId)
        => _states.TryGetValue(radioId, out var state) ? state : null;

    public void EnsureRadioRunning(int radioId)
    {
        _loops.GetOrAdd(radioId, _ =>
        {
            var cts = new CancellationTokenSource();
            var loopTask = Task.Run(() => RunLoopAsync(radioId, cts.Token), cts.Token);
            return cts;
        });
    }

    public async Task EnqueueSongAsync(int radioId, int songId, CancellationToken token = default)
    {
        var queue = _queues.GetOrAdd(radioId, _ => new ConcurrentQueue<int>());
        queue.Enqueue(songId);
        await BroadcastQueueUpdatedAsync(radioId, token);
    }

    public int[] GetQueueSnapshot(int radioId)
        => _queues.TryGetValue(radioId, out var queue) ? queue.ToArray() : Array.Empty<int>();

    private async Task RunLoopAsync(int radioId, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var radioService = scope.ServiceProvider.GetRequiredService<RadioService>();
            var songs = await radioService.GetSongsForRadioAsync(radioId);

            if (songs == null || songs.Count == 0)
            {
                _states.TryRemove(radioId, out _);
                await Task.Delay(TimeSpan.FromSeconds(5), token);
                continue;
            }

            var song = songs[Random.Shared.Next(songs.Count)];
            var queuedSong = await TryDequeueSongAsync(radioId, songs, token);
            if (queuedSong != null)
            {
                song = queuedSong;
            }
            var duration = Math.Max(song.DurationSeconds, 1);

            var state = new RadioPlaybackState
            {
                RadioId = radioId,
                SongId = song.Id,
                Filename = song.Filename,
                DurationSeconds = duration,
                StartedAtUtc = DateTimeOffset.UtcNow
            };

            _states[radioId] = state;
            await _hub.Clients.Group(GetGroup(radioId)).SendAsync("PlaybackState", state, token);

            await Task.Delay(TimeSpan.FromSeconds(duration), token);
        }
    }

    private async Task<Song?> TryDequeueSongAsync(int radioId, List<Song> songs, CancellationToken token)
    {
        if (!_queues.TryGetValue(radioId, out var queue))
        {
            return null;
        }

        while (queue.TryDequeue(out var songId))
        {
            var match = songs.FirstOrDefault(song => song.Id == songId);
            await BroadcastQueueUpdatedAsync(radioId, token);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private Task BroadcastQueueUpdatedAsync(int radioId, CancellationToken token)
    {
        var queue = GetQueueSnapshot(radioId);
        return _hub.Clients.Group(GetGroup(radioId)).SendAsync("QueueUpdated", queue, token);
    }

    public static string GetGroup(int radioId) => $"radio:{radioId}";
}
