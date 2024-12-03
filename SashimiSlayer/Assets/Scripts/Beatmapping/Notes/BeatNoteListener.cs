using Beatmapping.Notes;
using UnityEngine;

/// <summary>
///     Base class for objects that depend on BeatNote. This is used for initialization, to support editor previewing
/// </summary>
public abstract class BeatNoteListener : MonoBehaviour
{
    public abstract void OnNoteInitialized(BeatNote beatNote);
    public abstract void OnNoteCleanedUp(BeatNote beatNote);
}