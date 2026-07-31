using System;
using MediaRipper.Models.Outputs;

namespace MediaRipper.Services.Interfaces;

public interface IOutputQueueService
{
    /// <summary>
    ///     Gets if the queue is currently running.
    /// </summary>
    OutputQueueStatus Status { get; }

    /// <summary>
    ///     Starts the queue.
    /// </summary>
    void Start();

    /// <summary>
    ///     Stops the queue.
    /// </summary>
    void Stop();

    /// <summary>
    ///     Event that is invoked once the running status changed.
    /// </summary>
    event EventHandler? StatusChanged;
}