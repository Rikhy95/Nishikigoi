using UnityEngine;

public class Timer
{
    float startTime;
    bool unscaled;

    float CurrentTime => unscaled ? Time.unscaledTime : Time.time;

    public Timer(bool unscaled = false)
    {
        this.unscaled = unscaled;
        startTime = CurrentTime;
    }

    public float Elapsed => CurrentTime - startTime;

    public float Ratio(float overTime)
    {
        return Mathf.Clamp01(Elapsed / overTime);
    }

    public void Restart()
    {
        startTime = CurrentTime;
    }

    public bool Every(float repeatTime)
    {
        if (Elapsed >= repeatTime)
        {
            startTime = CurrentTime + (Elapsed - repeatTime);
            return true;
        }
        return false;
    }

}