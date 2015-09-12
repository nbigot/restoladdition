﻿using App3.Common;
using App3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace App3
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ItemPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        } 

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data.
            var item = await SampleDataSource.GetOrderAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void DeleteDishAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO
            this.navigationHelper.GoBack();
        }

        private void ValidateDishAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO : save changes
            var item = this.DefaultViewModel["Item"] as Order;
            var TBIDishName = FindName("TBIDishName") as TextBox;
            if (TBIDishName.Text != item.Title)
            {
                Debug.WriteLine("change dish name from: " + item.Title + " to: " + TBIDishName.Text);
                //RestaurantBill group;
                //var group = await SampleDataSource.GetGroupOfItemAsync(item.UniqueId);
                //group.Items.SetItem(456, item);
                //Task.Run(async () => await SampleDataSource.GetGroupOfItemAsync(item.UniqueId)).Result;
                //group.Items
                //item.
                //item.Title = TBIDishName.Text;
            }

            this.navigationHelper.GoBack();
        }

        //private async void ValidateDishAppBarButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //TODO : save changes
        //    var item = this.DefaultViewModel["Item"] as Order;
        //    var TBIDishName = FindName("TBIDishName") as TextBox;
        //    if (TBIDishName.Text != item.Title)
        //    {
        //        Debug.WriteLine("change dish name from: "+ item.Title + " to: "+ TBIDishName.Text);
        //        //RestaurantBill group;
        //        var group = await SampleDataSource.GetGroupOfItemAsync(item.UniqueId);
        //        group.Items.SetItem(456, item);
        //        //Task.Run(async () => await SampleDataSource.GetGroupOfItemAsync(item.UniqueId)).Result;
        //        //group.Items
        //        //item.
        //        //item.Title = TBIDishName.Text;
        //    }

        //    this.navigationHelper.GoBack();
        //}

        private void DiscardEditDishAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.navigationHelper.GoBack();
        }

        private void TextBox_Dish_Name_Loaded(object sender, RoutedEventArgs e)
        {
            var item = this.DefaultViewModel["Item"] as Order;
            var textbox = sender as TextBox;
            textbox.Text = item.Title;
        }

        private void TextBox_Dish_Price_Loaded(object sender, RoutedEventArgs e)
        {
            var item = this.DefaultViewModel["Item"] as Order;
            //Debug.WriteLine("price: "+ item.Price);
            var textbox = sender as TextBox;
            //textbox.Text = item.Price.ToString("0:0.##");
            textbox.Text = item.Price.ToString("F");
        }
    }
}