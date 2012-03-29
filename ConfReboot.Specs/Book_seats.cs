using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfReboot.Specs
{
    [TestClass]
    public class Book_seats : SUTClass
    {
        // As a conference organiser
        // I want the attendees
        // To be able to book a seat for a conference
        // So that they can come over

        [TestMethod]
        public void Order_seats()
        {
            var sut = new SUTClass();

            sut.RegisterType<Order>();
            sut.RegisterType<Conference>();
            sut.RegisterType<ConferenceOrderSaga>();

            var startdate = DateTime.Today;

            sut.Given(x => x.ConferenceRegistered(ConferenceId: "conferences/1", Name: "SomeConf", StartDate: startdate, MaxSeats: 100));
            sut.When(x => x.RegisterOrder(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10));
            sut.Then(x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10));
        }

        [TestMethod]
        public void Order_seats_when_not_enough_seats_available_but_some_are_pending()
        {
            var sut = new SUTClass();
            sut.RegisterType<Order>();
            sut.RegisterType<Conference>();
            sut.RegisterType<ConferenceOrderSaga>();

            var startdate = DateTime.Today;

            sut.Given(x => x.ConferenceRegistered(ConferenceId: "conferences/1", Name: "SomeConf", StartDate: startdate, MaxSeats: 100),
                      x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 84),
                      x => x.OrderPaid(ConferenceId: "conferences/1", OrderId: "orders/1"),
                      x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/2", Amount: 10)
                      );
            sut.When(x => x.RegisterOrder(ConferenceId: "conferences/1", OrderId: "orders/3", Amount: 11));
            sut.Then(x => x.OrderCancelled(ConferenceId: "conferences/1", OrderId: "orders/3",
                Reason: "There are currently 16/100 seats left in this conference , but 10 are pending; please try again later."));
        }

        [TestMethod]
        public void Order_seats_when_not_enough_seats_available_but_none_are_pending()
        {
            var sut = new SUTClass();
            sut.RegisterType<Order>();
            sut.RegisterType<Conference>();
            sut.RegisterType<ConferenceOrderSaga>();

            var startdate = DateTime.Today;

            sut.Given(x => x.ConferenceRegistered(ConferenceId: "conferences/1", Name: "SomeConf", StartDate: startdate, MaxSeats: 100),
                      x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 94),
                      x => x.OrderPaid(ConferenceId: "conferences/1", OrderId: "orders/1"),
                      x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/2", Amount: 2)
                      );
            sut.When(x => x.RegisterOrder(ConferenceId: "conferences/1", OrderId: "orders/2", Amount: 11));
            sut.Then(x => x.OrderCancelled(ConferenceId: "conferences/1", OrderId: "orders/2",
                Reason: "There are only 6/100 seats left in this conference and 2 of them are pending."));
        }

        [TestMethod]
        public void Pay_order()
        {
            var sut = new SUTClass();
            sut.RegisterType<Order>();
            sut.RegisterType<Conference>();
            sut.RegisterType<ConferenceOrderSaga>();

            var startDate = DateTime.Today;

            sut.Given(x => x.ConferenceRegistered(ConferenceId: "conferences/1", Name: "SomeConf", StartDate: startDate, MaxSeats: 100),
                      x => x.OrderRegistered(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10),
                      x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10));
            sut.When(x => x.PayOrder(OrderId: "orders/1"));
            sut.Then(x => x.OrderPaid(ConferenceId: "conferences/1", OrderId: "orders/1"));
        }

        [TestMethod]
        public void Cancel_order()
        {
            var sut = new SUTClass();
            sut.RegisterType<Order>();
            sut.RegisterType<Conference>();
            sut.RegisterType<ConferenceOrderSaga>();

            var startDate = DateTime.Today;

            sut.Given(x => x.ConferenceRegistered(ConferenceId: "conferences/1", Name: "SomeConf", StartDate: startDate, MaxSeats: 100),
                      x => x.OrderRegistered(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10),
                      x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10));
            sut.When(x => x.CancelOrder(ConferenceId: "conferences/1", OrderId: "orders/1", Reason: "The reason"));
            sut.Then(x => x.OrderCancelled(ConferenceId: "conferences/1", OrderId: "orders/1", Reason: "The reason"));
        }

        [TestMethod]
        public void Order_to_full_capacity_then_cancel_and_order_again()
        {
            var sut = new SUTClass();
            sut.RegisterType<Order>();
            sut.RegisterType<Conference>();
            sut.RegisterType<ConferenceOrderSaga>();

            var startDate = DateTime.Today;

            sut.Given(x => x.ConferenceRegistered(ConferenceId: "conferences/1", Name: "SomeConf", StartDate: startDate, MaxSeats: 100),
                      x => x.SeatsReserved(ConferenceId: "conferences/xxx", OrderId: "orders/xxx", Amount: 88),
                      x => x.OrderRegistered(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10),
                      x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/1", Amount: 10),
                      x => x.OrderCancelled(ConferenceId: "conferences/1", OrderId: "orders/1", Reason: "The reason"));
            sut.When(x => x.RegisterOrder(ConferenceId: "conferences/1", OrderId: "orders/2", Amount: 10));
            sut.Then(x => x.SeatsReserved(ConferenceId: "conferences/1", OrderId: "orders/2", Amount: 10));
        }
    }
}