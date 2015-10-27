using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

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
    /// DataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public abstract class DataSource : IDataSource
    {
        public Boolean LoadStatus { get; protected set; } = false;
        protected ObservableCollection<RestaurantBill> _bills = new ObservableCollection<RestaurantBill>();
        public ObservableCollection<RestaurantBill> Bills
        {
            get { return this._bills; }
        }

        public async Task<string> GenerateNewDefaultNameForBill()
        {
            await GetDataAsync();
            var now = DateTime.Now;
            var title = "Resto " + now.ToString("ddd d MMM", CultureInfo.CurrentCulture);
            var i = 0;
            while (Bills.Any(billIt => billIt.Title == title))
            {
                title = string.Format("Resto {0} ({1})", now.ToString("ddd d MMM", CultureInfo.CurrentCulture), ++i);
            }

            return title;
        }

        public async Task<RestaurantBill> AddBillAsync(string title, List<string> guests)
        {
            await GetDataAsync();

            Location location = null;
            var geoloc = await GetLocation();
            if (geoloc != null)
            {
                //Debug.WriteLine("location pos acc: " + geoloc.Coordinate?.Accuracy.ToString() ?? "undefined");
                //Debug.WriteLine("location pos long: " + geoloc.Coordinate?.Point.Position.Longitude.ToString() ?? "undefined");
                //Debug.WriteLine("location pos lat: " + geoloc.Coordinate?.Point.Position.Latitude.ToString() ?? "undefined");
                location = new Location(geoloc.Coordinate.Point.Position.Longitude, geoloc.Coordinate.Point.Position.Latitude);
            }

            var newUniqueId = Guid.NewGuid().ToString();
            var now = DateTime.Now;
            var bill = new RestaurantBill(newUniqueId, title, "", "", "", now, location);
            foreach (var guest in guests)
            {
                bill.AddGuest(guest);
            }
            Bills.Add(bill);
            return bill;
        }

        public async Task<bool> DeleteBillAsync(string uniqueId)
        {
            await GetDataAsync();

            return Bills.Remove(
                Bills.First(b => b.UniqueId == uniqueId)
            );
        }

        /// <summary>
        /// https://msdn.microsoft.com/fr-fr/library/windows/apps/jj206956(v=vs.105).aspx
        /// http://stackoverflow.com/questions/23692120/getting-civicaddress-on-windows-phone-8-1
        /// </summary>
        /// <returns>device position geolocalisation</returns>
        public async Task<Geoposition> GetLocation()
        {
            try
            {
                var geolocator = new Geolocator();
                return await geolocator.GetGeopositionAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in GetDataAsync : " + ex.Message);
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    Debug.WriteLine("location is disabled in phone settings.");
                }
                return null;
            }

            //private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
            //{
            //    //Get position data
            //    var pos = e.Position.Location;
            //    //Update mypos object
            //    mypos.update(pos.Latitude, pos.Longitude);
            //    //Update data on the main interface
            //    MainMap.SetView(mypos.getCoordinate(), MainMap.ZoomLevel, MapAnimationKind.Parabolic);
            //}
        }

        public async Task<IEnumerable<RestaurantBill>> GetBillsAsync()
        {
            await GetDataAsync();
            return Bills;
        }

        public async Task<RestaurantBill> GetBillAsync(string uniqueId)
        {
            await GetDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = Bills.Where((bill) => bill.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public async Task<Order> GetOrderAsync(string uniqueId)
        {
            await GetDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = Bills.SelectMany(bill => bill.Orders).Where((order) => order.UniqueId.Equals(uniqueId));
            if (matches.Count() > 1)
                throw new Exception("invalid data model : multiple occurences found for unique id : " + uniqueId);
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public async Task<RestaurantBill> GetGroupOfItemAsync(string uniqueId)
        {
            await GetDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = Bills.Where((bill) => bill.Orders.Any(order => order.UniqueId.Equals(uniqueId)));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public async Task<RestaurantBill> GetMostRecentBillAsync()
        {
            await GetDataAsync();
            return Bills.OrderByDescending(bill => bill.Date).First();
        }

        protected abstract Task GetDataAsync();
    }
}