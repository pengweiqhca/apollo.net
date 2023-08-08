namespace Com.Ctrip.Framework.Apollo.Core.Schedule;

internal class ExponentialSchedulePolicy : ISchedulePolicy
{
    private readonly int _delayTimeLowerBound;
    private readonly int _delayTimeUpperBound;
    private int _lastDelayTime;

    public ExponentialSchedulePolicy(int delayTimeLowerBound, int delayTimeUpperBound)
    {
        _delayTimeLowerBound = delayTimeLowerBound;
        _delayTimeUpperBound = delayTimeUpperBound;
    }

    public int Fail() => _lastDelayTime = _lastDelayTime == 0 ? _delayTimeLowerBound : Math.Min(_lastDelayTime << 1, _delayTimeUpperBound);

    public void Success() => _lastDelayTime = 0;
}
