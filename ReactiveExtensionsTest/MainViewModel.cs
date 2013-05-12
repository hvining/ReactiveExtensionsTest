using Microsoft.Practices.Prism.StoreApps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using System.Threading;
using ReactiveExtensionsTest.Services;

namespace ReactiveExtensionsTest
{
    public class MainViewModel : BindableBase
    {
        #region Fields
        private Dictionary<IObservable<StockQuote>, Boolean> _processes;
        private CancellationTokenSource _cancellationTokenSource; 
        #endregion

        #region Constructors
        public MainViewModel()
        {
            _processes = new Dictionary<IObservable<StockQuote>, Boolean>();

            MSFTStock = new ObservableCollection<StockQuote>();
            YHOOStock = new ObservableCollection<StockQuote>();

            LoadDataCommand = new DelegateCommand(() => LoadData(), () => CanLoadData);
            CancelCommand = new DelegateCommand(Cancel, () => CanCancel);
        } 
        #endregion

        #region Properties
        public ObservableCollection<StockQuote> StockQuotes { get; set; }
        public ObservableCollection<StockQuote> MSFTStock { get; set; }
        public ObservableCollection<StockQuote> YHOOStock { get; set; }
        #endregion

        #region Commands
        public DelegateCommand LoadDataCommand { get; set; }
        private async Task LoadData()
        {
            Clear();

            _cancellationTokenSource = CreateCancellationToken();
            MyHistoricalScheduler s = new MyHistoricalScheduler();
            var quotes = GetQuotes(s, await StockQuote.LoadQuotes());
            var msQuery = GetQuery(quotes, "MSFT");
            var yhQuery = GetQuery(quotes, "YHOO");
            Subscribe(msQuery, MSFTStock, _cancellationTokenSource.Token);
            Subscribe(yhQuery, YHOOStock, _cancellationTokenSource.Token);
            CreateTimeOut(msQuery, 2);
            CreateTimeOut(yhQuery, 2);

            s.Run(TimeSpan.FromSeconds(.1));
        }
        public Boolean CanLoadData
        {
            get
            {
                OnPropertyChanged("CanLoadData");

                foreach (var process in _processes)
                {
                    if (process.Value)
                        return false;
                }

                return true;
            }
        }

        public DelegateCommand CancelCommand { get; set; }
        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
        private Boolean CanCancel
        {
            get
            {
                return !CanLoadData;
            }
        }
        #endregion

        #region Methods
        private void Clear()
        {
            MSFTStock.Clear();
            YHOOStock.Clear();
        }

        private IObservable<StockQuote> GetQuery(IObservable<StockQuote> quotes, string ticker)
        {
            return from quote in quotes
                   where quote.Symbol == ticker
                   select quote;
        }

        private void Subscribe(IObservable<StockQuote> query, ObservableCollection<StockQuote> stock, CancellationToken token)
        {
            CoreDispatcher dispatcher = CoreApplication.Properties["Dispatcher"] as CoreDispatcher;

            query.Subscribe(async quote =>
                {
                    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        stock.Add(quote);
                        while (stock.Count > 50)
                            stock.RemoveAt(0);
                    });
                }, async (ex) =>
                {
                    await DialogService.DisplayDialog(ex.Message, "Error Occurred");
                }, () =>
                {
                    if (_processes.ContainsKey(query))
                    {
                        _processes[query] = false;
                        LoadDataCommand.RaiseCanExecuteChanged();
                        CancelCommand.RaiseCanExecuteChanged();
                    }
                }, token);

            _processes.Add(query, true);
            LoadDataCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }

        private IObservable<StockQuote> GetQuotes(IScheduler s, IEnumerable<StockQuote> quotes)
        {
            var subject = new Subject<StockQuote>();

            foreach (var quote in quotes)
            {
                var thisQuote = quote;
                s.Schedule(quote.Date, () => subject.OnNext(quote));
            }

            return subject;
        }

        private CancellationTokenSource CreateCancellationToken()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Token.Register(() =>
            {
                var keys = _processes.Keys.ToList();

                foreach (var key in keys)
                {
                    _processes[key] = false;
                }

                LoadDataCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            });

            return cancellationTokenSource;
        }

        private void CreateTimeOut(IObservable<StockQuote> query, int interval)
        {
            var timeout = Observable.Timeout(query, TimeSpan.FromSeconds(interval));
            timeout.Subscribe((quote) =>
            {
            }, (ex) =>
            {
                _processes[query] = false;
                LoadDataCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            });
        }
        #endregion
    }

    public class MyHistoricalScheduler : HistoricalScheduler
    {
        public void Run(TimeSpan delay)
        {
            Scheduler.ThreadPool.Schedule(
                delay,
                self =>
                {
                    var next = GetNext();
                    if (next == null)
                        return;
                    var dt = next.DueTime;
                    AdvanceTo(dt);
                    self(delay);
                });
        }
    }
}
