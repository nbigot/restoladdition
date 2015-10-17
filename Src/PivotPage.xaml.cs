using RestoLAddition.Common;
using RestoLAddition.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace RestoLAddition
{
    public sealed partial class PivotPage : Page
    {
        private const string CurrentBill = "CurrentBill";
        //private const string SecondGroupName = "SecondGroup";
        //private const string ThirdGroupName = "SecondGroup";

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private RestaurantBill bill;

        public PivotPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

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
        /// session. The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            //var bill = await SampleDataSource.GetBillAsync("Group-1");

            bill = e.NavigationParameter as RestaurantBill;
            this.DefaultViewModel[CurrentBill] = bill;

            ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
            PriceStringFormatConverter.PriceStrFormat = resourceLoader.GetString("PriceStrFormat");
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Adds an item to the list when the app bar button is clicked.
        /// </summary>
        private void BarButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            //string groupName = this.pivot.SelectedIndex == 0 ? CurrentBill : SecondGroupName;
            //var group = this.DefaultViewModel[groupName] as RestaurantBill;
            var nextItemId = bill.Orders.Count + 1;
            var newItem = new Order(
                string.Format(CultureInfo.InvariantCulture, "Group-{0}-Item-{1}", this.pivot.SelectedIndex + 1, nextItemId),
                string.Format(CultureInfo.CurrentCulture, this.resourceLoader.GetString("NewItemTitle"), nextItemId),
                string.Empty,
                string.Empty,
                this.resourceLoader.GetString("NewItemDescription"),
                string.Empty,
                0);

            bill.Orders.Add(newItem);

            // Scroll the new item into view.
            var container = this.pivot.ContainerFromIndex(this.pivot.SelectedIndex) as ContentControl;
// attention bug: container.ContentTemplateRoot est null si je clique sur (+) depuis la page de plats!!!
            var listView = container.ContentTemplateRoot as ListView;
            listView.ScrollIntoView(newItem, ScrollIntoViewAlignment.Leading);
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            if (!Frame.Navigate(typeof(ItemPage), new Tuple<RestaurantBill, Order>(bill, e.ClickedItem as Order)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        /// <summary>
        /// Loads the content for the second pivot item when it is scrolled into view.
        /// </summary>
        private void SecondPivot_Loaded(object sender, RoutedEventArgs e)
        {
            //var RestaurantBill = await SampleDataSource.GetBillAsync("Group-2");
            //this.DefaultViewModel[SecondGroupName] = RestaurantBill;
        }

        /// <summary>
        /// Loads the content for the third pivot item when it is scrolled into view.
        /// </summary>
        private void ThirdPivot_Loaded(object sender, RoutedEventArgs e)
        {
            //var RestaurantBill = await SampleDataSource.GetBillAsync("Group-2");
            //this.DefaultViewModel[ThirdGroupName] = RestaurantBill;
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

        /// <summary>
        /// affiche le context menu quand le user hold l'item (le nom du plat)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            // http://www.visuallylocated.com/post/2014/05/29/Migrating-from-the-Windows-Phone-Toolkit-ContextMenu-to-the-new-Runtime-MenuFlyout.aspx

            // this event is fired multiple times. We do not want to show the menu twice
            if (e.HoldingState != HoldingState.Started) return;

            FrameworkElement element = sender as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            FlyoutBase.ShowAttachedFlyout(element);
        }

        /// <summary>
        /// quand on clique sur la commande supprimer l'element 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            var item = menuFlyoutItem?.DataContext as Order;
            if (item == null) return;
            bill.Orders.Remove(item);
        }

        private void BarButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private async void BarButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(this.resourceLoader.GetString("DialogTitleDeleteThisBill"));    // "Supprimer cette note de restaurant?"
            
            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            messageDialog.Commands.Add(new UICommand(
                this.resourceLoader.GetString("BtnDelete"),  // "Supprimer"
                new UICommandInvokedHandler(this.CommandInvokedHandler),
                1));
            messageDialog.Commands.Add(new UICommand(
                this.resourceLoader.GetString("BtnCancel"),  // "Annuler"
                new UICommandInvokedHandler(this.CommandInvokedHandler),
                0));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            var result = await messageDialog.ShowAsync();
            if ((int)result.Id == 1)
            {
                // delete this bill and navigate to main page
                var oldBill = bill;
                this.DefaultViewModel[CurrentBill] = null;
                bill = null;
                await SampleDataSource.DeleteBillAsync(oldBill.UniqueId);

                this.navigationHelper.GoBack();
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
        }
    }
}
