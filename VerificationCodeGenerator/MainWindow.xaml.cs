using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using System.Windows;

namespace VerificationCodeGenerator
{
    public partial class MainWindow : Window
    {
        private volatile bool stopThreads = false;
        private readonly object lockObject = new object();
        private Random _random = new Random();
        private ObservableCollection<VerificationCode> threadItems = new ObservableCollection<VerificationCode>();
        private AppOptionsSection _appOptions;
        private string _connectionString;
        public MainWindow()
        {
            InitializeComponent();
            lvResults.ItemsSource = threadItems;
            _appOptions = ConfigurationManager.GetSection("appOptions") as AppOptionsSection;
            _connectionString = PropertyHelper.GetPropertyValue("ConnectionString", _appOptions);
        }

        private void StartThreads_Click(object sender, RoutedEventArgs e)
        {
            int minimumThreadCount = Convert.ToInt32(PropertyHelper.GetPropertyValue("MinimumThreadCount", _appOptions));
            int maximumThreadCount = Convert.ToInt32(PropertyHelper.GetPropertyValue("MaximumThreadCount", _appOptions));

            if (int.TryParse(txtThreadCount.Text, out int threadCount))
            {
                if (threadCount >= minimumThreadCount && threadCount <= maximumThreadCount)
                {
                    stopThreads = false;
                    threadItems.Clear();

                    for (int i = 0; i < threadCount; i++)
                    {
                        ThreadPool.QueueUserWorkItem(DoWork);
                    }
                }
                else
                {
                    lblResult.Content = "Please enter a number between 2 and 15.";
                }
            }
            else
            {
                lblResult.Content = "Invalid input. Please enter a valid number.";
            }
        }

        private void StopThreads_Click(object sender, RoutedEventArgs e)
        {
            lock (lockObject)
            {
                stopThreads = true;
            }
        }

        private void DoWork(object state)
        {
            ThreadLocal<Random> random = new ThreadLocal<Random>();
            int minimumRandomNumber = Convert.ToInt32(PropertyHelper.GetPropertyValue("MinimumDataLength", _appOptions));
            int maximumRandomNumber = Convert.ToInt32(PropertyHelper.GetPropertyValue("MaximumDataLength", _appOptions));

            while (true)
            {
                lock (lockObject)
                {
                    if (stopThreads)
                    {
                        Dispatcher.Invoke(() => lblResult.Content = "Threads stopped.");
                        return;
                    }
                }

                int threadId = Thread.CurrentThread.ManagedThreadId;
                string randomData = RandomGenerator.Next(minimumRandomNumber, maximumRandomNumber);
                int minimumThreadSleepTime = Convert.ToInt32(PropertyHelper.GetPropertyValue("MinimumThreadSleepTime", _appOptions));
                int maximumThreadSleepTime = Convert.ToInt32(PropertyHelper.GetPropertyValue("MaximumThreadSleepTime", _appOptions));

                Thread.Sleep(_random.Next(minimumThreadSleepTime, maximumThreadSleepTime));

                Dispatcher.Invoke(() =>
                {
                    var newItem = new VerificationCode
                    {
                        ID = Guid.NewGuid(),
                        ThreadID = threadId,
                        Time = DateTime.UtcNow,
                        Data = randomData,
                    };

                    threadItems.Insert(0, newItem);
                    SaveToDatabase(newItem);

                    int threadItemsCount = Convert.ToInt32(PropertyHelper.GetPropertyValue("ThreadItemsCount", _appOptions));

                    if (threadItems.Count > threadItemsCount)
                    {
                        threadItems.RemoveAt(threadItems.Count - 1);
                    }
                });
            }
        }

        private void SaveToDatabase(VerificationCode item)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO dbo.VerificationCodes (ID, ThreadID, Time, Data) VALUES (@ID, @ThreadID, @Time, @Data)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", item.ID);
                        command.Parameters.AddWithValue("@ThreadID", item.ThreadID);
                        command.Parameters.AddWithValue("@Time", item.Time);
                        command.Parameters.AddWithValue("@Data", item.Data);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
            }
        }
    }
}

