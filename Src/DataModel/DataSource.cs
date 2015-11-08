using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
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

        protected async Task GetDataAsync()
        {
            if (this.LoadStatus)
                return;
            LoadStatus = true;

            this.Bills.CollectionChanged -= this.OnCollectionChangedBills;

            this.Bills.Clear();

            string jsonText = await LoadDataAsync();
            DeserializeJson(jsonText);

            this.Bills.CollectionChanged += this.OnCollectionChangedBills;
        }

        protected void DeserializeJson(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                return;
            }

            var tmpBills = JsonConvert.DeserializeObject<MainJsonDataObject>(jsonText);
            foreach (var bill in tmpBills.Bills) {
                this.Bills.Add(bill);
            }

            //TODO: bug date pas deserialisee correctement
            // probeleme avec le viewer de xaml qui n'est pas compatible probablement!!!! :((
            // http://www.newtonsoft.com/json/help/html/DatesInJSON.htm
            // http://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_DateFormatHandling.htm
        }

        protected void old__DeserializeJson(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
            {
                return;
            }

            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Bills"].GetArray();

            try
            {
                foreach (JsonValue RestaurantBillValue in jsonArray)
                {
                    JsonObject RestaurantBillObject = RestaurantBillValue.GetObject();
                    DateTime date = DateTime.Now;
                    if (RestaurantBillObject.ContainsKey("Date"))
                    {
                        // "Date": "/Date(2008-06-15T21:15:07)/",
                        // http://regexlib.com/(X(1)A(plohCQBrb3JPpHX7KcH8auVKuRp8DdGM8wp_WQvQnRkqt078aQ_FHNpu-E3Q15qMcj_h5r_e1sDU99su-W3jeSa4Rg1YPf-sQ2t6j3wMh1hddZDrF4vczWP07PVccTC83u9Xx3nZSy-1p7Y2br4Fi6q9t_ZFiu_CyGmjBW-cH0xS1ybSZ5-oc4Nut3_tlJ_30))/REDetails.aspx?regexp_id=93
                        var r = new Regex(@"(?<grdate>20\d{2}(-|\/)((0[1-9])|(1[0-2]))(-|\/)((0[1-9])|([1-2][0-9])|(3[0-1]))(T|\s)(([0-1][0-9])|(2[0-3])):([0-5][0-9]):([0-5][0-9]))");
                        var strDate = r.Match(RestaurantBillObject["Date"].GetString()).Groups["grdate"].Value;
                        if (!string.IsNullOrEmpty(strDate))
                        {
                            date = DateTime.Parse(strDate);
                        }
                    }

                    Location location = null;
                    if (RestaurantBillObject.ContainsKey("Location"))
                    {
                        JsonObject loc = RestaurantBillObject["Location"].GetObject();
                        location = new Location(loc["Longitude"].GetString(), loc["Latitude"].GetString());
                    }

                    RestaurantBill bill = new RestaurantBill(
                        RestaurantBillObject["UniqueId"].GetString(),
                        RestaurantBillObject["Title"].GetString(),
                        RestaurantBillObject["Subtitle"].GetString(),
                        RestaurantBillObject["ImagePath"].GetString(),
                        RestaurantBillObject["Description"].GetString(),
                        date,
                        location
                    );

                    foreach (JsonValue GuestValue in RestaurantBillObject["Guests"].GetArray())
                    {
                        bill.AddGuest(GuestValue.GetString());
                    }

                    foreach (JsonValue OrderValue in RestaurantBillObject["Orders"].GetArray())
                    {
                        JsonObject OrderObject = OrderValue.GetObject();
                        decimal price = 0;
                        price = decimal.Parse(
                            OrderObject.ContainsKey("Price") ? (OrderObject["Price"].GetString()) : "0",
                            NumberStyles.AllowDecimalPoint,
                            CultureInfo.InvariantCulture);
                        var order = new Order(
                            OrderObject["UniqueId"].GetString(),
                            OrderObject["Title"].GetString(),
                            OrderObject["Subtitle"].GetString(),
                            OrderObject["ImagePath"].GetString(),
                            OrderObject["Description"].GetString(),
                            OrderObject["Content"].GetString(),
                            price
                        );
                        bill.Orders.Add(order);

                        if (OrderObject.Keys.Contains("Shares"))
                        {
                            foreach (JsonValue OrderShareValue in OrderObject["Shares"].GetArray())
                            {
                                JsonObject OrderShareObject = OrderShareValue.GetObject();
                                decimal sharePrice = 0;
                                sharePrice = decimal.Parse(
                                    OrderShareObject.ContainsKey("Price") ? (OrderShareObject["Price"].GetString()) : "0",
                                    NumberStyles.AllowDecimalPoint,
                                    CultureInfo.InvariantCulture);
                                order.Shares.Add(
                                    new OrderShare(
                                        //bill.Guests.First( guest => guest.Name == OrderShareObject["Guest"].GetString() ),
                                        //bill.Guests.First(guest => guest.Name == OrderShareObject["Guest"].GetObject()["Name"].GetString()),
                                        OrderShareObject["Guest"].GetString(),
                                        sharePrice
                                    )
                                );
                            }
                        }
                    }
                    this.Bills.Add(bill);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in GetDataAsync : " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        protected async Task<string> SerializeJson()
        {
            return await Task.Run(() => {
                    string jsonText = JsonConvert.SerializeObject(this.Bills);
                    return $"{{\"Bills\":{jsonText}}}";
            });
        }

        protected virtual void OnCollectionChangedBills(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        protected abstract Task<string> LoadDataAsync();
        protected abstract Task SaveDataAsync();
    }

    internal class MainJsonDataObject
    {
        public List<RestaurantBill> Bills { get; set; }
    }
}