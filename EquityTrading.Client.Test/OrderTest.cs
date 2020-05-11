using System;
using System.Threading;
using System.Threading.Tasks;
using EquityTrading.Client.Models;
using EquityTrading.Client.Services;
using EquityTrading.Client.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EquityTrading.Client.Test
{
    [TestClass]
    public class OrderTest
    {
        [TestInitialize]
        public void TestSetUp()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
        [TestMethod]
        public void TestSaveOrder()
        {
            Mock<IDialogService> dialogSvcMockObject = new Mock<IDialogService>();
            Mock<IOrderService> orderSvcMockObject = new Mock<IOrderService>();
            orderSvcMockObject.Setup(x => x.ProcessOrderAsync(null)).Returns(Task.FromResult(true));

            MainWindowViewModel viewModel =
                new MainWindowViewModel(orderSvcMockObject.Object, dialogSvcMockObject.Object);
            var result = viewModel.SaveOrder();
            Assert.AreEqual(true,result.Result);
        }
    }
}
