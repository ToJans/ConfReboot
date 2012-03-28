namespace ConfReboot
{
    public class ConferenceOrderSaga
    {
        public void OrderRegistered(string ConferenceId, string OrderId, int Amount)
        {
            OrderSeats(ConferenceId, OrderId, Amount);
        }

        public virtual void OrderSeats(string ConferenceId, string OrderId, int Amount) { }
    }
}