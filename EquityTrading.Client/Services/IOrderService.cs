using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EquityTrading.Client.Models;

namespace EquityTrading.Client.Services
{
    public interface IOrderService
    {
        event Action<User> UserLoggedIn;
        event Action<string> UserLoggedOut;
        event Action<string> UserDisconnected;
        event Action<string> UserReconnected;
        event Action ConnectionReconnecting;
        event Action ConnectionReconnected;
        event Action ConnectionClosed;
        event Action<string, MessageType> ReceiveNotification;
        event Action<IEnumerable<Order>> ReceiveNewOrder;
        event Action<string> ReceiveBidDepth;
        event Action<string> ReceiveAskDepth;
        Task ConnectAsync();
        Task<List<User>> LoginAsync(string name);
        Task LogoutAsync();
        Task<bool> ProcessOrderAsync(Order order);
    }
}