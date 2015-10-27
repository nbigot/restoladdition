using Microsoft.Practices.Unity;
using RestoLAddition.Common;
using RestoLAddition.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641
namespace RestoLAddition
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private TransitionCollection transitions;

        private UnityContainer container;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // Initialize the IoC container type bindings
            InitializeIocBindings();

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page.
                rootFrame = new Frame();

                // Associate the frame with a SuspensionManager key.
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: Change this value to a cache size that is appropriate for your application.
                rootFrame.CacheSize = 1;

                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate.
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
                }

                // Place the frame in the current Window.
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                #region init voice commands

                // https://msdn.microsoft.com/fr-fr/library/windows/apps/xaml/dn630430.aspx
                var storageFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommands/VoiceCommandDefinition.xml"));
                await Windows.Media.SpeechRecognition.VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(storageFile);

                #endregion

                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter.
                if (!rootFrame.Navigate(
                    typeof(Bills), 
                    new Tuple<string, IDataSource>(e.Arguments, container.Resolve<IDataSource>())
                ))    // PivotPage
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Ensure the current window is active.
            Window.Current.Activate();
        }


        protected override async void OnActivated(IActivatedEventArgs args)
        {
            // When a Voice Command activates the app, this method is going to 
            // be called and OnLaunched is not. Because of that we need similar
            // code to the code we have in OnLaunched

            // Initialize the IoC container type bindings
            InitializeIocBindings();

            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.CacheSize = 1;
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];
                Window.Current.Content = rootFrame;
                rootFrame.Navigate(
                    typeof(Bills),
                    new Tuple<string, IDataSource>(null, container.Resolve<IDataSource>())
                );
            }

            Window.Current.Activate();

            // For VoiceCommand activations, the activation Kind is ActivationKind.VoiceCommand
            if (args.Kind == ActivationKind.VoiceCommand)
            {
                // since we know this is the kind, a cast will work fine
                VoiceCommandActivatedEventArgs vcArgs = (VoiceCommandActivatedEventArgs) args;

                // The NavigationTarget retrieved here is the value of the Target attribute in the
                // Voice Command Definition xml Navigate node
                string target = vcArgs.Result.SemanticInterpretation.Properties["NavigationTarget"][0];

                // since the target is option, we check for its presence
                if (!String.IsNullOrEmpty(target))
                {
                    // This GetType is a way get the type corresponding to target page
                    // in order to navigate to the page.
                    Type pageType = Type.GetType(typeof(Bills).Namespace + "." + target);

                    // If the Target in the xml does not correspond to a page the pageType
                    // will be null. Such issue would be caught in app development.
                    // The two targets in the xml correspond to a page
                    if (pageType != null)
                    {
                        // Navigate to the page passing the speech result for further processing
                        // in the page. In Silverlight the navigation happens under the hood.
                        // For Phone Store Apps, it is done explicitly like this:
                        if (pageType == typeof(PivotPage))
                        {
                            // voice command: show lastest bill
                            var DataRepository = container.Resolve<IDataSource>();
                            var bill = await DataRepository.GetMostRecentBillAsync();
                            if (bill != null)
                            {
                                // bill found
                                rootFrame.Navigate(
                                    typeof(PivotPage),
                                    new Tuple<IDataSource, RestaurantBill>(DataRepository, bill)
                                );
                            }
                            else
                            {
                                // no bill found : display home page
                                rootFrame.Navigate(
                                    typeof(Bills),
                                    new Tuple<string, IDataSource>(null, container.Resolve<IDataSource>())
                                );
                            }
                        }
                        else if (pageType == typeof(Bills))
                        {
                            // voice command: add new bill
                            var DataRepository = container.Resolve<IDataSource>();
                            string defaultTitle = vcArgs.Result.SemanticInterpretation.Properties["restoName"].FirstOrDefault();
                            if (string.IsNullOrEmpty(defaultTitle))
                                defaultTitle = await DataRepository.GenerateNewDefaultNameForBill();
                            var dialog = new DialogEditResto(defaultTitle);
                            var result = await dialog.ShowAsync();
                            if (result == ContentDialogResult.Primary)
                            {
                                var bill = await DataRepository.AddBillAsync(
                                    dialog.RestaurantTitle,
                                    dialog.GetGuestsNames()
                                );
                                rootFrame.Navigate(
                                   typeof(PivotPage),
                                   new Tuple<IDataSource, RestaurantBill>(DataRepository, bill)
                               );
                            }
                            else
                            {
                                // new bill cancled, display home page
                                rootFrame.Navigate(
                                    typeof(Bills),
                                    new Tuple<string, IDataSource>(null, container.Resolve<IDataSource>())
                                );
                            }
                        }
                        else
                            throw new Exception("voice route not found");
                    }
                }
            }
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        private void InitializeIocBindings()
        {
            container = new UnityContainer();
            //container.RegisterType<IDataSource, SampleDataSource>();
            var data = new SampleDataSource();
            container.RegisterInstance<IDataSource>(data);
        }
    }
}