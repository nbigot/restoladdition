using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace App3.Data
{
    /// <summary>
    /// part d'une commande pour un individu
    /// </summary>
    public class OrderShare
    {
        public OrderShare(String person, Decimal price)
        {
            this.Person = person;
            this.Price = price;
        }

        /// <summary>
        /// nom de la personne
        /// </summary>
        public string Person { get; private set; }

        /// <summary>
        /// prix que la personne doit payer
        /// </summary>
        public decimal Price { get; private set; }
    }

    /// <summary>
    /// une commande (générique) : une entrée, un plat, une boisson, un dessert, ...
    /// </summary>
    public class Order
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
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }
        public decimal Price { get; private set; }

        /// <summary>
        /// ce plat est à payer par plusieurs personnnes
        /// la liste des personnes qui doivent payer pour ce plat est Shares
        /// </summary>
        public ObservableCollection<OrderShare> Shares { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Une note de restaurant (l'addition, la facture)
    /// </summary>
    public class RestaurantBill
    {
        public RestaurantBill(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Orders = new ObservableCollection<Order>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<Order> Orders { get; private set; }

        public decimal Sum
        {
            get
            {
                return Orders.Sum(item => item.Price);
            }
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

        private ObservableCollection<RestaurantBill> _bills = new ObservableCollection<RestaurantBill>();
        public ObservableCollection<RestaurantBill> Bills
        {
            get { return this._bills; }
        }

        public static async Task<IEnumerable<RestaurantBill>> GetBillsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();
            return _sampleDataSource.Bills;
        }

        public static async Task<RestaurantBill> GetBillAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
Debug.WriteLine("GetBillAsync find: " + uniqueId);
foreach (var bill in _sampleDataSource.Bills)
                Debug.WriteLine("found bill: " + bill.UniqueId);



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
            if (this._bills.Count != 0)
                return;

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
                    RestaurantBill bill = new RestaurantBill(RestaurantBillObject["UniqueId"].GetString(),
                                                                RestaurantBillObject["Title"].GetString(),
                                                                RestaurantBillObject["Subtitle"].GetString(),
                                                                RestaurantBillObject["ImagePath"].GetString(),
                                                                RestaurantBillObject["Description"].GetString());

                    foreach (JsonValue OrderValue in RestaurantBillObject["Orders"].GetArray())
                    {
                        JsonObject OrderObject = OrderValue.GetObject();
                        decimal price = 0;
                        price = decimal.Parse(OrderObject.ContainsKey("Price") ? (OrderObject["Price"].GetString()) : "0", System.Globalization.NumberStyles.AllowDecimalPoint);
                        var order = new Order(OrderObject["UniqueId"].GetString(),
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
                                sharePrice = decimal.Parse(OrderShareObject.ContainsKey("Price") ? (OrderShareObject["Price"].GetString()) : "0", System.Globalization.NumberStyles.AllowDecimalPoint);
                                order.Shares.Add(
                                    new OrderShare(
                                        OrderShareObject["Person"].GetString(),
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
                throw new Exception(ex.Message);
            }
        }
    }
}