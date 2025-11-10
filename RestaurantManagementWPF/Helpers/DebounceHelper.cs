using System;
using System.Windows.Threading;

namespace RestaurantManagementWPF.Helpers
{
    /// <summary>
    /// Helper class to debounce actions (delay execution until user stops interacting)
    /// </summary>
    public class DebounceHelper
    {
        private DispatcherTimer? _timer;
        private Action? _action;

        public void Debounce(int delayMilliseconds, Action action)
        {
            _action = action;

            // Cancel previous timer if exists
            _timer?.Stop();

            // Create new timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(delayMilliseconds)
            };

            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                _action?.Invoke();
            };

            _timer.Start();
        }
    }
}
