using System;
using System.Threading.Tasks;
using Windows.Storage;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.
namespace RestoLAddition.Data
{
    /// <summary>
    /// Creates a collection of bills and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource : DataSource
    {
        #region singleton

        private static SampleDataSource _dataSource = new SampleDataSource();
        public static SampleDataSource GetInstance { get { return _dataSource; } }

        #endregion

        protected override async Task<string> LoadDataAsync()
        {
            Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            return await FileIO.ReadTextAsync(file);   // be shure file is encoded in utf8 with bom format if any accents chars inside
        }

        protected override async Task SaveDataAsync()
        {
            // do nothing
        }
    }
}