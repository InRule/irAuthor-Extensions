using System;
using System.ComponentModel;
using System.Windows;

namespace InRuleLabs.AuthoringExtensions.FieldsInUse.Extensions
{
    public static class ThreadExtensions
    {
        public static void InvokeOnWorkerThread(this DependencyObject obj, Action action)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (sender, e) => { action(); };
            bw.RunWorkerAsync();
        }
    }
}