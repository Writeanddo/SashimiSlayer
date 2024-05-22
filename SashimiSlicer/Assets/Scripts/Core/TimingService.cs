using UnityEngine;

public class TimingService : MonoBehaviour
{
    public static TimingService Instance { get; private set; }

    public double DeltaTime => _deltaTime;
    public bool DidCrossBeatThisFrame => _didCrossBeatThisFrame;
    public double TimePastBeat => _timePastBeat;
    public int BeatNumber => _beatNumber;
    public double TimePerBeat => _intervalPerBeat;
    public double CurrentTime => _currentTime;
    public double CurrentBeatmapTime => _currentTime - _startTime;

    private int _beatNumber;

    private BeatmapConfigSo _currentBeatmap;

    private double _currentTime;
    private double _deltaTime;

    private bool _didCrossBeatThisFrame;

    private double _intervalPerBeat;

    private double _lastFrameTime;

    private double _startTime;
    private double _timePastBeat;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        Tick();
    }

    private void Tick()
    {
        // Update times
        _lastFrameTime = _currentTime;
        double newTime = AudioSettings.dspTime;

        if (newTime == _lastFrameTime)
        {
            // estimate using Deltatime
            newTime += Time.deltaTime;
        }
        else
        {
            // If we somehow need to go back in time, just hold
            if (newTime < _lastFrameTime)
            {
                newTime = _lastFrameTime;
            }
        }

        _deltaTime = newTime - _currentTime;
        _currentTime = newTime;

        // Calculate if we crossed a beat
        CalculateBeatTiming(
            _lastFrameTime,
            _currentTime,
            out _timePastBeat,
            out _didCrossBeatThisFrame,
            out _beatNumber);

        if (_didCrossBeatThisFrame)
        {
            Debug.DrawLine(Vector3.right * (_beatNumber % 4), Vector3.one * 1000f, Color.magenta,
                (float)_intervalPerBeat - 0.01f);
        }
    }

    public void StartBeatmap(BeatmapConfigSo beatmap)
    {
        _currentBeatmap = beatmap;
        _startTime = AudioSettings.dspTime + beatmap.StartTime;
        _intervalPerBeat = 60 / _currentBeatmap.Bpm;
    }

    private void CalculateBeatTiming(
        double lastTime,
        double currentTime,
        out double timePastBeat,
        out bool didCrossBeat,
        out int beatNumber)
    {
        double elapsedTime = currentTime - _startTime;
        double lastElapsedTime = lastTime - _startTime;
        timePastBeat = elapsedTime % _intervalPerBeat;

        beatNumber = (int)(elapsedTime / _intervalPerBeat);
        var lastBeat = (int)(lastElapsedTime / _intervalPerBeat);

        didCrossBeat = beatNumber > lastBeat;
    }

    /// <summary>
    ///     Resync to a new start time. Used for looping
    /// </summary>
    /// <param name="newStartTime"></param>
    public void Resync()
    {
        _startTime = AudioSettings.dspTime + _currentBeatmap.StartTime;
        Tick();
    }

    public int GetClosestBeat()
    {
        double timePastBeat = _timePastBeat;
        double timeToNextBeat = _intervalPerBeat - timePastBeat;
        return timeToNextBeat < timePastBeat ? _beatNumber + 1 : _beatNumber;
    }
}