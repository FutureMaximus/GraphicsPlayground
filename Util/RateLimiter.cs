namespace GraphicsPlayground.Util;

public class RateLimiter(float rate)
{
    private readonly float _rate = rate;
    private float _lastCall = 0f;
    private float _timeSinceLastCall = 0f;
    private bool _isFirstCall = true;

    public bool CanProceed(float time)
    {
        if (_isFirstCall)
        {
            _isFirstCall = false;
            return true;
        }
        _timeSinceLastCall = time - _lastCall;
        if (_timeSinceLastCall >= _rate)
        {
            _lastCall = time;
            return true;
        }
        return false;
    }
}
