namespace ConfReboot
{
    public class Order
    {
        public virtual string OrderId { get; set; }

        string ConferenceId = null;
        bool IsPaid = false;

        public void RegisterOrder(string ConferenceId, int Amount)
        {
            OrderRegistered(ConferenceId, Amount);
        }

        public void PayOrder()
        {
            Guard.Against(IsPaid, "This order has already been paid");
            OrderPaid(ConferenceId);
        }

        public void CancelOrder(string Reason)
        {
            Guard.Against(IsPaid, "This order has already been paid");
            OrderCancelled(ConferenceId, Reason);
        }

        protected virtual void OrderRegistered(string ConferenceId, int Amount)
        {
            this.ConferenceId = ConferenceId;
            this.IsPaid = false;
        }

        protected virtual void OrderCancelled(string ConferenceId, string Reason)
        {
            IsPaid = false;
        }

        protected virtual void OrderPaid(string ConferenceId)
        {
            IsPaid = true;
        }
    }
}