namespace DiversityPhone {
    using System;
    using System.Diagnostics.Contracts;

    public struct PercentageReporter<T> {
        private readonly IProgress<T> Progress;
        private readonly Func<int, T> PercentageToProgress;
        private readonly int Total;

        private int _CompletedPercentage;
        private int _NextPercent;
        private int _Completed;

        public int Completed {
            get { return _Completed; }
            set {
                Contract.Requires(value <= Total, "Completed may not be greater than Total");
                Contract.Requires(value >= Completed, "Completed may not decrease");

                _Completed = value;
                if (_Completed >= _NextPercent) {
                    _CompletedPercentage = (_Completed * 100) / Total;
                    _NextPercent = ((_CompletedPercentage + 1) * Total) / 100;
                    Progress.Report(PercentageToProgress(_CompletedPercentage));
                }

                Contract.Ensures(Completed == value);
                Contract.Invariant(_NextPercent >= Completed);
            }
        }

        public PercentageReporter(
            IProgress<T> Progress,
            Func<int, T> PrecentageToProgress,
            int Total
            ) {
            Contract.Requires(Progress != null);
            Contract.Requires(PrecentageToProgress != null);
            Contract.Requires(Total > 0, "Total must be > 0");

            this.Progress = Progress;
            this.PercentageToProgress = PrecentageToProgress;
            this.Total = Total;

            _CompletedPercentage = 0;
            _NextPercent = 0;
            _Completed = 0;

            Completed = 0;
        }

    }
}
