﻿// SpotifyGPX by Simon Field

using SpotifyGPX.Observation;

namespace SpotifyGPX.Broadcasting;

public class StringBroadcaster : Broadcaster<string>
{
    public string Type { get; set; } = "INFO";

    protected override string HandleMessageBasedOnMessageLogLevel(string message, LogLevel l)
    {
        return $"[{Type}] {message}";
    }

    protected override string HandleMessageBasedOnObserverLogLevelAndMessageLogLevel(string message, LogLevel messageLevel, LogLevel observerLevel)
    {
        if (observerLevel == LogLevel.Debug)
        {
            return $"[OBSERVER-{HashCode}] [{messageLevel.ToString().ToUpper()}] {message}";
        }

        return message;
    }

    protected override void AdditionalSubscriptionInstructions()
    {
        Broadcast($"New subscriber to broadcaster ({HashCode}), now with {Observers.Count} observers.", Observation.LogLevel.Debug);
    }

    public override StringBroadcaster Clone()
    {
        StringBroadcaster clone = new()
        {
            Observers = Observers
        };

        Broadcast($"Cloned broadcaster ({HashCode}) with {Observers.Count} observers into new broadcaster ({clone.HashCode}) with {clone.Observers.Count} observers.", Observation.LogLevel.Debug);

        return clone;
    }
}
