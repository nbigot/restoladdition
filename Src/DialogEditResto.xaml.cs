using RestoLAddition.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers.Provider;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace RestoLAddition
{
    public sealed partial class DialogEditResto : ContentDialog
    {
        private string _title;
        private RestaurantBill _bill;

        public string RestaurantTitle { get { return _title; } }

        public DialogEditResto(string defaultTitle)
        {
            _title = defaultTitle;
            _bill = null;
            this.InitializeComponent();
        }

        public DialogEditResto(RestaurantBill bill)
        {
            _bill = bill;
            _title = _bill.Title;
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // validate button
            if (_bill != null)
            {
                var guestsNames = GetGuests();
                // TODO: update bill there...
                //var sortedGuests = _bill.Guests.ToList();
                //sortedGuests.Sort();
            }
        }

        public List<string> GetGuests()
        {
            var guestsNames = new List<string>(
                new string[] { this.Guest1.Text, this.Guest2.Text, this.Guest3.Text, this.Guest4.Text, this.Guest5.Text }
                .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct()
            );
            guestsNames.Sort();
            return guestsNames;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // cancel button
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.RestaurantName.Text = _title;
            if (_bill != null) {
                var sortedGuests = _bill.Guests.ToList();
                sortedGuests.Sort();
                var cpt = sortedGuests.Count;
                if (cpt > 0)
                {
                    this.Guest1.Text = sortedGuests[0];
                }
                if (cpt > 1)
                {
                    this.Guest2.Text = sortedGuests[1];
                }
                if (cpt > 2)
                {
                    this.Guest3.Text = sortedGuests[2];
                }
                if (cpt > 3)
                {
                    this.Guest4.Text = sortedGuests[3];
                }
                if (cpt > 4)
                {
                    this.Guest5.Text = sortedGuests[4];
                }
            }
        }
    }
}
