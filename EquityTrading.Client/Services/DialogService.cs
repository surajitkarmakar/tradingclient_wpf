using System;
using Microsoft.Win32;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;

namespace EquityTrading.Client.Services
{
    public enum MessageType { Error, Info, Success, Warning }
    public class DialogService : IDialogService
    {
        readonly Notifier _notifier = new Notifier(cfg =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                    {
                    cfg.PositionProvider = new WindowPositionProvider(
                        parentWindow: Application.Current.MainWindow,
                        corner: Corner.TopRight,
                        offsetX: 10,
                        offsetY: 10);

                    cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                    cfg.Dispatcher = Application.Current.Dispatcher;
                    }
                );
        });

        public void DisplayToast(string message, MessageType type = MessageType.Info)
        {
            switch (type)
            {
                case MessageType.Error: _notifier.ShowError(message);
                    break;
                case MessageType.Info: _notifier.ShowInformation(message);
                    break;
                case MessageType.Success: _notifier.ShowSuccess(message);
                    break;
                case MessageType.Warning: _notifier.ShowWarning(message);
                    break;
            }
        }
    }
}
