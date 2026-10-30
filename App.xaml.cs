using System;
using System.Windows;
using System.Windows.Threading;

namespace NutritionPlannerApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var inner = e.Exception;
            string details = "";
            while (inner != null)
            {
                details += $"[{inner.GetType().Name}]: {inner.Message}\n{inner.StackTrace}\n\n";
                inner = inner.InnerException;
            }
            try
            {
                System.IO.File.WriteAllText("crash.log", details);
            }
            catch {}
            MessageBox.Show($"應用程式發生非預期錯誤：\n{e.Exception.Message}\n\n內部錯誤：\n{e.Exception.InnerException?.Message}\n\n詳細已寫入 crash.log", "系統錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"應用程式嚴重錯誤：\n{ex.Message}", "系統錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
