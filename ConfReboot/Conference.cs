﻿using System;
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
            if (Seats.Count(x => x.State != SeatStateEnum.Paid) < Amount)
            {
                var msg = string.Format("There are only {0}/{1} seats left in this conference and {2} of them are pending."
                    , Seats.Count(x => x.State != SeatStateEnum.Paid)
                    , Seats.Count()
                    , Seats.Count(x => x.State == SeatStateEnum.Pending));
                OrderCancelled(OrderId, msg);
            }
            else if (Seats.Count(x => x.State == SeatStateEnum.Free) < Amount)
            {
                var msg = string.Format("There are currently {0}/{1} seats left in this conference , but {2} are pending; please try again later."
                    , Seats.Count(x => x.State != SeatStateEnum.Paid)
                    , Seats.Count()
                    , Seats.Count(x => x.State == SeatStateEnum.Pending));
                OrderCancelled(OrderId, msg);
            }
            else
            {
                SeatsReserved(OrderId, Amount);
            }
        }

        protected virtual void OrderCancelled(string OrderId, string Reason)
        {
            foreach (var seat in Seats.Where(x => x.State == SeatStateEnum.Pending && x.OrderId == OrderId))
            {
                seat.State = SeatStateEnum.Free;
                seat.OrderId = null;
            }
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

        public virtual void OrderPaid(string OrderId)
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