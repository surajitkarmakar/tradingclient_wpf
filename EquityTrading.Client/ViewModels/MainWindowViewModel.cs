using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using EquityTrading.Client.Services;
using EquityTrading.Client.Enums;
using EquityTrading.Client.Models;
using EquityTrading.Client.Commands;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Collections.Concurrent;
using System.Windows;
using Newtonsoft.Json;
using MessageType = EquityTrading.Client.Services.MessageType;

namespace EquityTrading.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IOrderService orderService;
        private IDialogService dialogService;
        private TaskFactory ctxTaskFactory;
        private Order _currentOrder = new Order();
        private ObservableCollection<MarketDepth> _bidDepth = new ObservableCollection<MarketDepth>();
        public ObservableCollection<MarketDepth> BidDepth 
        {
            get => _bidDepth;
            set
            {
                _bidDepth = value;
                OnPropertyChanged(nameof(BidDepth));
            }
        }
        private ObservableCollection<MarketDepth> _askDepth = new ObservableCollection<MarketDepth>();
        public ObservableCollection<MarketDepth> AskDepth
        {
            get => _askDepth;
            set
            {
                _askDepth = value;
                OnPropertyChanged(nameof(AskDepth));
            }
        }
        public Order CurrentOrder 
        {
            get
            {
                return _currentOrder;
            }
            set 
            {
                _currentOrder = value;
                OnPropertyChanged(nameof(CurrentOrder));
            }
        }
        private ObservableCollection<Order> _orderBook = new ObservableCollection<Order>();
        public ObservableCollection<Order> OrderBook { get { return _orderBook; } 
            set 
            {
                _orderBook = value;
                OnPropertyChanged(nameof(OrderBook));
            } 
        } 

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<UserBase> _participants = new ObservableCollection<UserBase>();
        public ObservableCollection<UserBase> Participants
        {
            get { return _participants; }
            set
            {
                _participants = value;
                OnPropertyChanged();
            }
        }

        private UserBase _selectedParticipant;
        public UserBase SelectedParticipant
        {
            get { return _selectedParticipant; }
            set
            {
                _selectedParticipant = value;
                if (SelectedParticipant.HasSentNewMessage) SelectedParticipant.HasSentNewMessage = false;
                OnPropertyChanged();
            }
        }

        private UserModes _userMode;
        public UserModes UserMode
        {
            get { return _userMode; }
            set
            {
                _userMode = value;
                OnPropertyChanged();
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        #region SaveOrderCommand
        private ICommand _saveOrderCommand;
        public ICommand SaveOrderCommand
        {
            get
            {
                return _saveOrderCommand ?? (_saveOrderCommand = new RelayCommandAsync(() => SaveOrder()));
            }
        }
        public async Task<bool> SaveOrder()
        {
            try
            {
                _currentOrder.UserName = this.UserName;
                if (this.UserName == null) _currentOrder = null;
                var result = await orderService.ProcessOrderAsync(_currentOrder);
                CurrentOrder = new Order();
                IsConnected = true;
                return result;
            }
            catch (Exception) { return false; }
        }
        #endregion

        #region Send Bulk Buy Orders Command
        private ICommand _sendBuyOrdersCommand;
        public ICommand SendBuyOrdersCommand => _sendBuyOrdersCommand ?? (_sendBuyOrdersCommand = new RelayCommandAsync(SendBulkBuyOrders));

        public async Task<bool> SendBulkBuyOrders()
        {
            for (int i = 920; i <= 940; i++)
            {
                Order o = new Order() { Type = OrderType.BUY, Price = i, Quantity = new Random().Next(10, 50), Symbol = "HDFCBANK" };
                _currentOrder = o;
                await SaveOrder();
            }
            return true;
        }
        #endregion

        #region Send Bulk Sell Orders Command
        private ICommand _sendSellOrdersCommand;
        public ICommand SendSellOrdersCommand => _sendSellOrdersCommand ?? (_sendSellOrdersCommand = new RelayCommandAsync(SendBulkSellOrders));

        public async Task<bool> SendBulkSellOrders()
        {
            for (int i = 970; i >= 950; i--)
            {
                Order o = new Order() { Type = OrderType.SELL, Price = i, Quantity = new Random().Next(10, 50), Symbol = "HDFCBANK" };
                _currentOrder = o;
                await SaveOrder();
            }
            return true;
        }
        #endregion

        #region Connect Command
        private ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommandAsync(() => Connect()));
            }
        }

        private async Task<bool> Connect()
        {
            try
            {
                await orderService.ConnectAsync();
                IsConnected = true;
                return true;
            }
            catch (Exception) { return false; }
        }
        #endregion

        #region Login Command
        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand =
                    new RelayCommandAsync(() => Login(), (o) => CanLogin()));
            }
        }

        private async Task<bool> Login()
        {
            try
            {
                List<User> users = new List<User>();
                users = await orderService.LoginAsync(_userName);
                if (users != null)
                {
                    users.ForEach(u => Participants.Add(new UserBase { Name = u.Name }));
                    UserMode = UserModes.Trade;
                    IsLoggedIn = true;
                    return true;
                }
                else
                {
                    dialogService.DisplayToast("Username is already in use");
                    return false;
                }

            }
            catch (Exception e) { return false; }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(UserName) && UserName.Length >= 2 && IsConnected;
        }
        #endregion

        #region Logout Command
        private ICommand _logoutCommand;
        public ICommand LogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand =
                    new RelayCommandAsync(() => Logout(), (o) => CanLogout()));
            }
        }

        private async Task<bool> Logout()
        {
            try
            {
                await orderService.LogoutAsync();
                UserMode = UserModes.Login;
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool CanLogout()
        {
            return IsConnected && IsLoggedIn;
        }
        #endregion


        #region Event Handlers
        private void OnReceiveNewOrder(IEnumerable<Order> orders)
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    _orderBook.Clear();
                    foreach (var order in orders)
                    {
                        _orderBook.Add(order);
                    }
                }
            );
        }

        private void ParticipantLogin(User u)
        {
            var ptp = Participants.FirstOrDefault(p => string.Equals(p.Name, u.Name));
            if (_isLoggedIn && ptp == null)
            {
                ctxTaskFactory.StartNew(() => Participants.Add(new UserBase
                {
                    Name = u.Name,
                })).Wait();
            }
        }

        private void ParticipantDisconnection(string name)
        {
            var person = Participants.Where((p) => string.Equals(p.Name, name)).FirstOrDefault();
            if (person != null) person.IsLoggedIn = false;
        }

        private void ParticipantReconnection(string name)
        {
            var person = Participants.Where((p) => string.Equals(p.Name, name)).FirstOrDefault();
            if (person != null) person.IsLoggedIn = true;
        }

        private void Reconnecting()
        {
            IsConnected = false;
            IsLoggedIn = false;
        }

        private async void Reconnected()
        {
            if (!string.IsNullOrEmpty(_userName)) await orderService.LoginAsync(_userName);
            IsConnected = true;
            IsLoggedIn = true;
        }

        private async void Disconnected()
        {
            var connectionTask = orderService.ConnectAsync();
            await connectionTask.ContinueWith(t => {
                if (!t.IsFaulted)
                {
                    IsConnected = true;
                    orderService.LoginAsync(_userName).Wait();
                    IsLoggedIn = true;
                }
            });
        }

        #endregion


        public MainWindowViewModel(IOrderService orderSvc, IDialogService dialogSvc)
        {
            dialogService = dialogSvc;
            orderService = orderSvc;
            orderSvc.ReceiveNewOrder += OnReceiveNewOrder;
            orderSvc.ReceiveNotification += OrderNotificationHandler;
            orderSvc.UserLoggedIn += ParticipantLogin;
            orderSvc.UserLoggedOut += ParticipantDisconnection;
            orderSvc.UserDisconnected += ParticipantDisconnection;
            orderSvc.UserReconnected += ParticipantReconnection;
            orderSvc.ConnectionReconnecting += Reconnecting;
            orderSvc.ConnectionReconnected += Reconnected;
            orderSvc.ConnectionClosed += Disconnected;
            orderSvc.ReceiveBidDepth += OnReceiveBidDepth;
            orderSvc.ReceiveAskDepth+= OnReceiveAskDepth;

            CurrentOrder = new Order();
            ctxTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnReceiveAskDepth(string obj)
        {
            AskDepth = new ObservableCollection<MarketDepth>(JsonConvert.DeserializeObject<IEnumerable<MarketDepth>>(obj));

        }
        private void OnReceiveBidDepth(string obj)
        {
            BidDepth = new ObservableCollection<MarketDepth>(JsonConvert.DeserializeObject<IEnumerable<MarketDepth>>(obj));
        }

        private void OrderNotificationHandler(string message, MessageType type)
        {
            dialogService.DisplayToast(message,type);
        }
    }
}