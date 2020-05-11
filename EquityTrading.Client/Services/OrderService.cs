using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EquityTrading.Client.Models;
using System.Net;
using Microsoft.AspNet.SignalR.Client;


namespace EquityTrading.Client.Services
{
    public class OrderService : IOrderService
    {
        public event Action<string, MessageType> ReceiveNotification;
        public event Action<IEnumerable<Order>> ReceiveNewOrder;
        public event Action<string> ReceiveBidDepth;
        public event Action<string> ReceiveAskDepth;
        public event Action<string> UserDisconnected;
        public event Action<User> UserLoggedIn;
        public event Action<string> UserLoggedOut;
        public event Action<string> UserReconnected;
        public event Action ConnectionReconnecting;
        public event Action ConnectionReconnected;
        public event Action ConnectionClosed;

        private IHubProxy _hubProxy;
        private HubConnection _connection;
        private string url = "http://localhost:8080/tradingclient";

        public async Task ConnectAsync()
        {
            _connection = new HubConnection(url);
            _hubProxy = _connection.CreateHubProxy("OrderBook");
            _hubProxy.On<User>("UserLogin", (u) => UserLoggedIn?.Invoke(u));
            _hubProxy.On<string>("UserLogout", (n) => UserLoggedOut?.Invoke(n));
            _hubProxy.On<string>("UserDisconnection", (n) => UserDisconnected?.Invoke(n));
            _hubProxy.On<string>("UserReconnection", (n) => UserReconnected?.Invoke(n));
            _hubProxy.On<IEnumerable<Order>>("BroadcastOrders", (n) => ReceiveNewOrder?.Invoke(n));
            _hubProxy.On<string, MessageType>("NotifyUser", (n,m) => ReceiveNotification?.Invoke(n,m));
            _hubProxy.On<string>("SendBidDepth", (x) => ReceiveBidDepth?.Invoke(x));
            _hubProxy.On<string>("SendAskDepth", (x => ReceiveAskDepth?.Invoke(x)));

            _connection.Reconnecting += Reconnecting;
            _connection.Reconnected += Reconnected;
            _connection.Closed += Disconnected;

            ServicePointManager.DefaultConnectionLimit = 10;
            await _connection.Start();
        }

        private void Disconnected()
        {
            ConnectionClosed?.Invoke();
        }

        private void Reconnected()
        {
            ConnectionReconnected?.Invoke();
        }

        private void Reconnecting()
        {
            ConnectionReconnecting?.Invoke();
        }

        public async Task<List<User>> LoginAsync(string name)
        {
            return await _hubProxy.Invoke<List<User>>("Login", new object[] { name });
        }

        public async Task LogoutAsync()
        {
            await _hubProxy.Invoke("Logout");
        }

        public async Task<bool> ProcessOrderAsync(Order order)
        {
            await _hubProxy.Invoke("ProcessOrder", order);
            return true;
        }
    }
}