using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace RestoLAddition.Data
{
    public interface IDataSource
    {
        ObservableCollection<RestaurantBill> Bills { get; }
        bool LoadStatus { get; }

        Task<RestaurantBill> AddBillAsync(string title, List<string> guests);
        Task<bool> DeleteBillAsync(string uniqueId);
        Task<string> GenerateNewDefaultNameForBill();
        Task<RestaurantBill> GetBillAsync(string uniqueId);
        Task<IEnumerable<RestaurantBill>> GetBillsAsync();
        Task<RestaurantBill> GetGroupOfItemAsync(string uniqueId);
        Task<Geoposition> GetLocation();
        Task<Order> GetOrderAsync(string uniqueId);
    }
}