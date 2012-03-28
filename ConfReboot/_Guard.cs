using System;

namespace ConfReboot
{
    public static class Guard
    {
        public static void That(bool assertion, string message, params object[] msgpars)
        {
            if (!assertion)
                throw new InvalidOperationException(string.Format(message, msgpars));
        }

        public static void Against(bool assertion, string message, params object[] msgpars)
        {
            That(!assertion, message, msgpars);
        }
    }
}