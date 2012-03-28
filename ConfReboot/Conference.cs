using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfReboot
{
    public class Conference
    {
        public virtual string ConferenceId { get; set; }

        private enum SeatStateEnum
        {
            Free,
            Pending,
            Paid
        }

        private class SeatState
        {
            public SeatState() { }

            public SeatStateEnum State = SeatStateEnum.Free;
            public string OrderId;
        }

        DateTime Date;
        List<SeatState> Seats = new List<SeatState>();

        public void RegisterConference(string Name, DateTime StartDate, int MaxSeats)
        {
            Guard.Against(Seats.Any(), "This conference has already been registered");
            ConferenceRegistered(Name, StartDate, MaxSeats);
        }

        public void OrderSeats(string OrderId, int Amount)
        {
            Guard.Against(Seats.Count(x => x.State != SeatStateEnum.Paid) < Amount,
                "There are not enough free seats left in this conference");
            Guard.Against(Seats.Count(x => x.State == SeatStateEnum.Free) < Amount,
                "There are currently not enough free seats left; some might be pending, so please try again later");
            SeatsReserved(OrderId, Amount);
        }

        protected virtual void SeatsReserved(string OrderId, int Amount)
        {
            foreach (var seat in Seats.Where(x => x.State == SeatStateEnum.Free).Take(Amount))
            {
                seat.OrderId = OrderId;
                seat.State = SeatStateEnum.Pending;
            }
        }

        protected virtual void ConferenceRegistered(string Name, DateTime StartDate, int MaxSeats)
        {
            this.Date = StartDate;
            for (int i = 0; i < MaxSeats; i++)
                this.Seats.Add(new SeatState());
        }

        public virtual void OrderPaymentConfirmed(string OrderId)
        {
            foreach (var seat in Seats.Where(x => x.State == SeatStateEnum.Pending && x.OrderId == OrderId))
            {
                seat.State = SeatStateEnum.Paid;
            }
        }

        protected virtual void OrderPaymentCancelled(string OrderId)
        {
            foreach (var seat in Seats.Where(x => x.State == SeatStateEnum.Pending && x.OrderId == OrderId))
            {
                seat.State = SeatStateEnum.Free;
                seat.OrderId = null;
            }
        }
    }
}