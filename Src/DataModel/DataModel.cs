using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.ComponentModel;

namespace RestoLAddition.Data
{
/*
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
}