using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Devices.Geolocation;
using System.ComponentModel;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.
namespace RestoLAddition.Data
{/*
    /// <summary>
    /// un convive
    /// </summary>
    public class Guest
    {
        public Guest(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// guest name
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
*/
    /// <summary>
    /// part d'une commande pour un convive
    /// </summary>
    public class OrderShare
    {
        public OrderShare(string guest, Decimal price)
        {
            /* HACK!!!! */
            //this.Guest = new Guest(guest);
            this.Guest = guest;
            this.Price = price;
        }

        /*public OrderShare(Guest guest, Decimal price)
        {
            this.Guest = guest.Name;
            this.Price = price;
        }*/

        /// <summary>
        /// le convive
        /// </summary>
        //public Guest Guest { get; set; }
        public string Guest { get; private set; }

        /// <summary>
        /// prix que la personne doit payer
        /// </summary>
        public decimal Price { get; private set; }
    }

    /// <summary>
    /// une commande (générique) : une entrée, un plat, une boisson, un dessert, ...
    /// </summary>
    public class Order : INotifyPropertyChanged
    {
        public Order(String uniqueId, String title, String subtitle, String imagePath, String description, String content, Decimal price)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
            this.Price = price;
            this.Shares = new ObservableCollection<OrderShare>();
        }

        public string UniqueId { get; private set; }
        private string _title;
        public string Title { get { return _title; } set { _title = value; OnPropertyChanged(new PropertyChangedEventArgs("Title")); } }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public decimal _price;
        public decimal Price { get { return _price; } set { _price = value; OnPropertyChanged(new PropertyChangedEventArgs("Price")); } }

        /// <summary>
        /// ce plat est à payer par plusieurs personnnes
        /// la liste des personnes qui doivent payer pour ce plat est Shares
        /// </summary>
        public ObservableCollection<OrderShare> Shares { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Une note de restaurant (l'addition, la facture)
    /// </summary>
    public class Location
    {
        public Location(String Longitude, String Latitude)
        {
            this.Longitude = Longitude;
            this.Latitude = Latitude;
        }

        public Location(double Longitude, double Latitude)
        {
            this.Longitude = Longitude.ToString(CultureInfo.InvariantCulture);
            this.Latitude = Latitude.ToString(CultureInfo.InvariantCulture);
        }

        public string Longitude { get; private set; }
        public string Latitude { get; private set; }
    }

    /// <summary>
    /// Une note de restaurant (l'addition, la facture)
    /// </summary>
    public class RestaurantBill
    {
        public RestaurantBill(String uniqueId, String title, String subtitle, String imagePath, String description, DateTime date, Location Location)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Date = date;
            this.Location = Location;
            this.Orders = new ObservableCollection<Order>();
            //this.Guests = new ObservableCollection<Guest>();
            this.Guests = new ObservableCollection<string>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public Location Location { get; private set; }
        public DateTime Date { get; private set; }
        public ObservableCollection<Order> Orders { get; private set; }
        //public ObservableCollection<Guest> Guests { get; private set; }
        public ObservableCollection<string> Guests { get; private set; }

        public decimal Sum
        {
            get
            {
                return Orders.Sum(item => item.Price);
            }
        }

        public string GuestList
        {
            get
            {
                return string.Join(", ", Guests);
            }
        }

        public void AddGuest(string name)
        {
            //Guests.Add(new Guest(name));
            Guests.Add(name);
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of bills and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        public Boolean LoadStatus { get; private set; } = false;
        private ObservableCollection<RestaurantBill> _bills = new ObservableCollection<RestaurantBill>();
        public ObservableCollection<RestaurantBill> Bills
        {
            get { return this._bills; }
        }

        public static async Task<string> GenerateNewDefaultNameForBill()
        {
            await _sampleDataSource.GetSampleDataAsync();
            var now = DateTime.Now;
            var title = "Resto " + now.ToString("ddd d MMM", CultureInfo.CurrentCulture);
            var i = 0;
            while (_sampleDataSource.Bills.Any(billIt => billIt.Title == title))
            {
                title = string.Format("Resto {0} ({1})", now.ToString("ddd d MMM", CultureInfo.CurrentCulture), ++i);
            }

            return title;
        }

        public static async Task<RestaurantBill> AddBillAsync(string title, List<string> guests)
        {
            await _sampleDataSource.GetSampleDataAsync();

            Location location = null;
            var geoloc = await GetLocation();
            if (geoloc != null)
            {
                Debug.WriteLine("location pos acc: " + geoloc.Coordinate?.Accuracy.ToString() ?? "undefined");
                Debug.WriteLine("location pos long: " + geoloc.Coordinate?.Point.Position.Longitude.ToString() ?? "undefined");
                Debug.WriteLine("location pos lat: " + geoloc.Coordinate?.Point.Position.Latitude.ToString() ?? "undefined");
                location = new Location(geoloc.Coordinate.Point.Position.Longitude, geoloc.Coordinate.Point.Position.Latitude);
            }

            var newUniqueId = Guid.NewGuid().ToString();
            var now = DateTime.Now;
            var bill = new RestaurantBill(newUniqueId, title, "", "", "", now, location);
            foreach (var guest in guests)
            {
                bill.AddGuest(guest);
            }
            _sampleDataSource.Bills.Add(bill);
            return bill;
        }

        public static async Task<bool> DeleteBillAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Bills.Remove(
                _sampleDataSource.Bills.First( b => b.UniqueId == uniqueId)
            );
        }

        /// <summary>
        /// https://msdn.microsoft.com/fr-fr/library/windows/apps/jj206956(v=vs.105).aspx
        /// http://stackoverflow.com/questions/23692120/getting-civicaddress-on-windows-phone-8-1
        /// </summary>
        /// <returns>device position geolocalisation</returns>
        public static async Task<Geoposition> GetLocation()
        {
            try
            {
                var geolocator = new Geolocator();
                return await geolocator.GetGeopositionAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in GetSampleDataAsync : " + ex.Message);
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

        public static async Task<IEnumerable<RestaurantBill>> GetBillsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();
            return _sampleDataSource.Bills;
        }

        public static async Task<RestaurantBill> GetBillAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Bills.Where((bill) => bill.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<Order> GetOrderAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Bills.SelectMany(bill => bill.Orders).Where((order) => order.UniqueId.Equals(uniqueId));
            if (matches.Count() > 1)
                throw new Exception("invalid data model : multiple occurences found for unique id : " + uniqueId);
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<RestaurantBill> GetGroupOfItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Bills.Where((bill) => bill.Orders.Any(order => order.UniqueId.Equals(uniqueId)));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private async Task GetSampleDataAsync()
        {
            if (this.LoadStatus)
                return;
            LoadStatus = true;

            Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);   // be shure file is encoded in utf8 with bom format if any accents chars inside
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
                Debug.WriteLine("Exception in GetSampleDataAsync : " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}