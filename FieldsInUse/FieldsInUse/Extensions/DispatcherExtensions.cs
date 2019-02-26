using System;
using System.Windows.Threading;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions
{
    public static class DispatcherExtensions
    {
        public static void CheckedInvoke(this Dispatcher dispatcher, Action action)
        {
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }
    }
}
