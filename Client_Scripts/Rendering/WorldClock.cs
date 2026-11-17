using System;
using UnityEngine;

[System.Serializable]
public struct WorldTimer
{
    public float dayStartTime; //0.25f
    public float dayEndTime; //0.75f
    public float sunriseStartTime; //0f
    public float sunriseEndTime; //0.15f
    public float sunsetStartTime; //0.85f
    public float sunsetEndTime; //0.99f
}

[System.Serializable]
public struct Clock
{
    public float totalRatio;
    [Header("Day/Night cycle")]
    public float dayRatio;
    public float nightRatio;
    [Header("Full day cycles")]
    public float dawnRatio;
    public float brightRatio;
    public float duskRatio;
    public float darkRatio;
}

[ExecuteInEditMode]
public class WorldClock : MonoBehaviour
{
    [SerializeField] private float _dayDurationMinutes = 240;
    [SerializeField] private string _timeHour;
    [SerializeField] private float _timeElapsed = 0;
    [SerializeField] private bool _startClock;
    [SerializeField] private WorldTimer _worldTimer;
    [SerializeField] private Clock _clock;

    public Clock Clock { get { return _clock; } }
    public string TimeHour { get { return _timeHour; } }
    public bool StartClock { get { return _startClock; } set { _startClock = value; } }
    public float DayDurationMinutes { get { return _dayDurationMinutes; } set { _dayDurationMinutes = value > 0 ? value : 240f; } }

    private static WorldClock _instance;
    public static WorldClock Instance { get { return _instance; } }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            _startClock = true;
        }
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Update()
    {
        if (_startClock || Application.isPlaying)
        {
            UpdateClock(Time.deltaTime);
        }

        CalculateDayNightRatio();
        CalculateSunPhaseRatio();
    }

    private void UpdateClock(float deltaTime)
    {
        if (_dayDurationMinutes <= 0)
        {
            _dayDurationMinutes = 240f;
        }

        _timeElapsed += deltaTime;

        float totalDurationSec = _dayDurationMinutes * 60f;
        if (_timeElapsed >= totalDurationSec)
        {
            _timeElapsed %= totalDurationSec;
        }
        else if (_timeElapsed < 0)
        {
            _timeElapsed = 0;
        }

        _clock.totalRatio = Mathf.Clamp01(_timeElapsed / totalDurationSec);

        // Calculate the number of seconds based on the percentage
        int seconds = (int)(_clock.totalRatio * 86400f);

        // Convert seconds to hours, minutes, and seconds
        int hours = (seconds / 3600) % 24;
        int minutes = (seconds % 3600) / 60;
        int remainingSeconds = seconds % 60;

        // Create a TimeSpan object with the calculated hours, minutes, and seconds
        TimeSpan time = new TimeSpan(hours, minutes, remainingSeconds);

        // Format the TimeSpan object as a string in the desired format (HH:mm:ss)
        _timeHour = time.ToString(@"hh\:mm\:ss");
    }

    public void SynchronizeClock(int currentGameTime)
    {
        _startClock = true;
        if (_dayDurationMinutes <= 0)
        {
            _dayDurationMinutes = 240f;
        }

        //Gametime goes from 0 to 1439
        float serverDayRatio = Mathf.Clamp01(currentGameTime / 1439f);
        _timeElapsed = serverDayRatio * _dayDurationMinutes * 60f;

        UpdateClock(0f);
        CalculateDayNightRatio();
        CalculateSunPhaseRatio();

        Debug.Log($"[WorldClock] Synchronized game time to {currentGameTime} (server time {_timeHour}, ratio {_clock.totalRatio:F3}). Clock running: {_startClock}");
    }

    public bool IsNightTime()
    {
        return _clock.nightRatio > 0 && _clock.nightRatio <= 1 || _clock.dayRatio < 0.25f;
    }

    private void CalculateDayNightRatio()
    {
        float dayDuration = _worldTimer.dayEndTime - _worldTimer.dayStartTime;
        if (dayDuration <= 0.0001f)
        {
            dayDuration = 0.56f;
        }

        float nightDuration = 1.0f - _worldTimer.dayEndTime + _worldTimer.dayStartTime;
        if (nightDuration <= 0.0001f)
        {
            nightDuration = 0.44f;
        }

        if (_clock.totalRatio >= _worldTimer.dayStartTime && _clock.totalRatio < _worldTimer.dayEndTime)
        {
            _clock.nightRatio = 0;
            _clock.dayRatio = Mathf.Clamp01((_clock.totalRatio - _worldTimer.dayStartTime) / dayDuration);
        }
        else
        {
            _clock.dayRatio = 0;
            if (_clock.totalRatio >= _worldTimer.dayEndTime)
            {
                _clock.nightRatio = Mathf.Clamp01((_clock.totalRatio - _worldTimer.dayEndTime) / nightDuration);
            }
            else
            {
                _clock.nightRatio = Mathf.Clamp01((_clock.totalRatio + (1.0f - _worldTimer.dayEndTime)) / nightDuration);
            }
        }
    }

    private void CalculateSunPhaseRatio()
    {
        _clock.dawnRatio = CalculatePeriodRatio(_worldTimer.sunriseStartTime, _worldTimer.sunriseEndTime);
        _clock.brightRatio = CalculatePeriodRatio(_worldTimer.sunriseEndTime, _worldTimer.sunsetStartTime);
        _clock.duskRatio = CalculatePeriodRatio(_worldTimer.sunsetStartTime, _worldTimer.sunsetEndTime);
        _clock.darkRatio = CalculatePeriodRatio(-.99f, _worldTimer.sunriseStartTime);
    }

    private float CalculatePeriodRatio(float startRatio, float endRatio)
    {
        float periodDuration = (startRatio < 0) ? (Mathf.Abs(startRatio) + endRatio) : (endRatio - startRatio);
        if (periodDuration <= 0.0001f)
        {
            return 0f;
        }

        float ratio = 0;
        if (_clock.dayRatio >= 0 && _clock.nightRatio == 0)
        {
            // Day
            if (_clock.dayRatio <= endRatio)
            {
                // In Range        
                if (startRatio < 0)
                {
                    ratio += Mathf.Abs(startRatio);
                    ratio += _clock.dayRatio;
                }
                else if (_clock.dayRatio >= startRatio)
                {
                    ratio -= startRatio;
                    ratio += _clock.dayRatio;
                }
                else
                {
                    ratio = 0;
                }

                ratio = Mathf.Clamp(ratio / periodDuration, 0, 1);
            }
            else
            {
                ratio = 1;
            }
        }
        else
        {
            // Night
            if (startRatio < 0 && _clock.nightRatio > (1 + startRatio))
            {
                ratio += Mathf.Clamp((_clock.nightRatio - (1 + startRatio)) / periodDuration, 0, 1);
            }
            else
            {
                ratio = 0;
            }
        }

        return ratio;
    }
}
