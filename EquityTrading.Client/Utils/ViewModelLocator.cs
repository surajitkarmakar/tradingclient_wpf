using Unity;
using EquityTrading.Client.Services;
using EquityTrading.Client.ViewModels;

namespace EquityTrading.Client.Utils
{
    public class ViewModelLocator
    {
        private UnityContainer container;

        public ViewModelLocator()
        {
            container = new UnityContainer();
            container.RegisterType<IOrderService, OrderService>();
            container.RegisterType<IDialogService, DialogService>();
        }

        public MainWindowViewModel MainVM
        {
            get { return container.Resolve<MainWindowViewModel>(); }
        }
    }
}
