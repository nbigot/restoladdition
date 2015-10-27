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

        protected override async Task GetDataAsync()
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
                Debug.WriteLine("Exception in GetDataAsync : " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}