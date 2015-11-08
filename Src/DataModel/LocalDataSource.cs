using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace RestoLAddition.Data
{
    /// <summary>
    /// Creates a collection of bills and items with content read from a roaming json file.
    /// </summary>
    public sealed class LocalDataSource : DataSource
    {
        #region singleton

        private static LocalDataSource _dataSource = new LocalDataSource();
        public static LocalDataSource GetInstance { get { return _dataSource; } }

        #endregion

        public const string DataFilename = "data.json";

        protected override async Task<string> LoadDataAsync()
        {
            // https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh700362.aspx
            try {
                var dataFile = await ApplicationData.Current.RoamingFolder.GetFileAsync(DataFilename);
                return await FileIO.ReadTextAsync(dataFile);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        protected override async Task SaveDataAsync()
        {
            string jsonText = SerializeJson();
            var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync(DataFilename, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, jsonText);
        }

        protected override async void OnCollectionChangedBills(object sender, NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"OnCollectionChangedBills {e.Action.ToString()}");
            await SaveDataAsync();
        }
    }
}
