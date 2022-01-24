using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Si è verificato un errore imprevisto:\n" + e.Exception.GetType() + "\n" + e.Exception.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
           
        }
    }
}
