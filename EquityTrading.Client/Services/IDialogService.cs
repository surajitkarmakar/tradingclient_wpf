namespace EquityTrading.Client.Services
{
    public interface IDialogService
    {
        void DisplayToast(string message, MessageType type= MessageType.Info);
    }
}