namespace QiLabMonitor.Pages
{
    public partial class Index
    {
        public QuantizedInertia qi = QuantizedInertia.singleton;

        public Task ResetTime()
        {
            qi.startTime = DateTime.Now;
            return Task.CompletedTask;
        }

        public double VoltBarVolts()
        {
            return qi.siglent.powerEnabled ? (qi.expiermentVolts * 100.0) / qi.physicalExpiermentVoltsLimit : 0;
        }
        public double VoltBarMax()
        {
            return ((qi.maxExpiermentVolts - (qi.siglent.powerEnabled ? qi.expiermentVolts : 0)) * 100.0) / qi.physicalExpiermentVoltsLimit;
        }
        public double VoltBarSpacer()
        {
            return ((qi.physicalExpiermentVoltsLimit - qi.maxExpiermentVolts) * 100.0) / qi.physicalExpiermentVoltsLimit;
        }

        /// <summary>
        /// Code to make data sync live
        /// </summary>
        private class TimerEventArgs : EventArgs { }

        private static event EventHandler<TimerEventArgs> OnTimerChanged;

        private static Timer InternalTimer = new Timer((state) =>
        {
            if (null != OnTimerChanged)
            {
                OnTimerChanged.Invoke(null, new TimerEventArgs());
            }
        });

        protected override Task OnInitializedAsync()
        {
            InternalTimer.Change(200, 200);

            if (OnTimerChanged == null) { OnTimerChanged += (o, e) => { }; } // odd, this gets set twice, and does not work if I only set it once.
            else
            {
                OnTimerChanged += (o, e) =>
                {
                    this.InvokeAsync(() =>
                    {
                        this.qi.OnTick();
                        this.StateHasChanged();
                    });
                };
            }
            return base.OnInitializedAsync();
        }

    }
}