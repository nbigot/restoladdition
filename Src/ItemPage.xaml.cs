using RestoLAddition.Common;
using RestoLAddition.Data;
using System;
using System.Globalization;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace RestoLAddition
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private const string BillKey = "Bill";
        private const string OrderKey = "ItemOrder";
        private RestaurantBill bill;
        private Order orderItem;
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

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
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var tuple = e.NavigationParameter as Tuple<RestaurantBill, Order>;
            this.bill = tuple.Item1;
            this.orderItem = tuple.Item2;
            this.DefaultViewModel[BillKey] = bill;
            this.DefaultViewModel[OrderKey] = orderItem;
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

        private async void DeleteDishAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            // Create the message dialog and set its content
            var messageDialog = new MessageDialog(this.resourceLoader.GetString("DialogTitleDeleteThisDish"));    // "Supprimer ce plat?"

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
                // delete this dish and navigate to main page
                bill.Orders.Remove( orderItem );
                this.navigationHelper.GoBack();
            }
        }

        private void CommandInvokedHandler(IUICommand command)
        {
        }

        private async void ValidateDishAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // change dish name
                var TBIDishName = FindName("TBIDishName") as TextBox;
                if (TBIDishName.Text != orderItem.Title)
                {
                    //Debug.WriteLine("change dish name from: " + itemOrder.Title + " to: " + TBIDishName.Text);
                    orderItem.Title = TBIDishName.Text;
                }

                // change dish price
                var TBIDishPrice = FindName("TBIDishPrice") as TextBox;
                if (TBIDishPrice.Text != orderItem.Price.ToString(CultureInfo.CurrentCulture))
                {
                    //Debug.WriteLine("change dish price from: " + itemOrder.Price.ToString(CultureInfo.CurrentCulture) + " to: " + TBIDishPrice.Text);
                    orderItem.Price = decimal.Parse(TBIDishPrice.Text, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture);
                }

                this.navigationHelper.GoBack();
            }
            catch (FormatException ex)
            {
                MessageDialog messageDialog = new MessageDialog("Error: " + ex.Message, "Not a Number");    // TODO : localize the text
                var result = await messageDialog.ShowAsync();
            }
        }

        private void DiscardEditDishAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.navigationHelper.GoBack();
        }

        private void TextBox_Dish_Name_Loaded(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            textbox.Text = orderItem.Title;
        }

        private void TextBox_Dish_Price_Loaded(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            //textbox.Text = item.Price.ToString("0:0.##");
            textbox.Text = orderItem.Price.ToString("F");
        }
    }
}