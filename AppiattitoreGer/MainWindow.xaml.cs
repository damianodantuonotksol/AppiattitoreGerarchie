using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {

        string _connectionString { get 
            {
                if (String.IsNullOrEmpty(hostname.Text))
                {
                    return "";
                }
                if (SQL_RADIO.IsChecked == true)
                {
                    return _database!=null?$"Server={_server};Database={_database};User Id={_user};Password={_password};":$"Server={_server};User Id={_user};Password={_password};";
                }
                else
                {
                    return _database != null ? $"Server={_server};Database={_database};Integrated Security=True;":$"Server={_server};Integrated Security=True";
                }
            } 
        }
        string? _database { get
            {
                if (Databases.SelectedIndex > -1)
                {
                    return Databases.SelectedItem.ToString();
                }
                else
                {
                    return null;
                }
            }
        }
        string? _table { get
            {
                return Tables.SelectedItem.ToString();
            }
        }        
        string? _descriptionTable { get
            {
                return DescriptionTables.SelectedItem.ToString();
            }
        }
        string _server { get
            {
                if (String.IsNullOrEmpty(hostname.Text))
                {
                    return "";
                }
                string port_string = String.IsNullOrEmpty(port.Text) ? ",1433":"," + port.Text;
                if (istanza.Text != null)
                {
                    return hostname.Text + "\\" + istanza.Text + port_string;
                }
                else
                {
                    return hostname.Text + port_string;
                }
            } 
        }
        string? _user { get
            {
                return userTextBox.Text;
            }
        }
        string? _password
        {
            get
            {
                return passwordTextBox.Password;
            }
        }

        string? _result;

        DispatcherTimer dispatcherTimer;

        //Create a timer with interval of 2 secs
        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 3);
        }

        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            successLabel.Visibility = Visibility.Hidden;
            dispatcherTimer.IsEnabled = false;
        }

        private void Windows_Checked(object sender, RoutedEventArgs e)
        {
            TextBox textBox = userTextBox;
            if (textBox != null)
            {
               textBox.Text = WindowsIdentity.GetCurrent().Name;
               textBox.IsEnabled = false;
               passwordTextBox.IsEnabled = false;
            }
        }        
        private void SQL_Checked(object sender, RoutedEventArgs e)
        {
            TextBox textBox = userTextBox;
            if (textBox != null)
            {
               textBox.Text = "";
               textBox.IsEnabled = true;
               passwordTextBox.IsEnabled = true;
               textBox.Focus();
            }
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_connectionString))
            {
                Databases.Items.Clear();

                List<string> items = DataLayer.DataProcess.GetDatabases(_connectionString);

                foreach (string item in items)
                {
                    Databases.Items.Add(item);
                }
            }
        }        

        private void Table_DropDownOpened(object sender, EventArgs e)
        {
            Tables.Items.Clear();

            List<string> items = DataLayer.DataProcess.GetTables(_connectionString);

            foreach (string item in items)
            {
                Tables.Items.Add(item);
            }
        }                
        private void DescriptionTable_DropDownOpened(object sender, EventArgs e)
        {
            DescriptionTables.Items.Clear();

            List<string> items = DataLayer.DataProcess.GetDescriptionTables(_connectionString);

            foreach (string item in items)
            {
                DescriptionTables.Items.Add(item);
            }
        }        
   
        private void generateQuery_Button(object sender, EventArgs e)
        {
            _result = DataLayer.DataProcess.GenerateQuery(_connectionString, _table, _descriptionTable);
            if (_result != null)
            {
                resultQuery.Text = _result;
            }
            extractPreview();
        }

        private void extractPreview()
        {
            DataTable extraction = new DataTable();
            extraction = DataLayer.DataProcess.GeneratePreview(_connectionString, _result);
            Preview.DataContext = extraction.DefaultView;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_result != null)
            {
                Clipboard.SetText(_result);
                successLabel.Visibility = Visibility.Visible;
                dispatcherTimer.Start();
            }
        }
    }
}
