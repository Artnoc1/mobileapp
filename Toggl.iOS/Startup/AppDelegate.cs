using Foundation;
using Toggl.Core;
using Toggl.Core.UI;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.iOS.Presentation;
using Toggl.iOS.Services;
using Toggl.Shared;
using UIKit;
using UserNotifications;
using Firebase.CloudMessaging;
using Google.SignIn;
using System.Reactive;
using Toggl.iOS.ViewControllers;
using AVFoundation;

namespace Toggl.iOS
{
    [Register(nameof(AppDelegate))]
    public sealed partial class AppDelegate : UIApplicationDelegate, IUNUserNotificationCenterDelegate
    {
        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
#if !DEBUG
            Firebase.Core.App.Configure();
            Messaging.SharedInstance.Delegate = this;
#endif

            UNUserNotificationCenter.Current.Delegate = this;
            UIApplication.SharedApplication.RegisterForRemoteNotifications();

            var googleServiceDictionary = NSDictionary.FromFile("GoogleService-Info.plist");
            SignIn.SharedInstance.ClientId = googleServiceDictionary["CLIENT_ID"].ToString();

            initializeAnalytics();

            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            Window.MakeKeyAndVisible();

            IosDependencyContainer.EnsureInitialized(Window, this);
            var app = new AppStart(IosDependencyContainer.Instance);
            app.LoadLocalizationConfiguration();
            app.UpdateOnboardingProgress();
            app.SetFirstOpened();
            app.SetupBackgroundSync();

            var accessLevel = app.GetAccessLevel();
            loginWithCredentialsIfNecessary(accessLevel);
            navigateAccordingToAccessLevel(accessLevel, app);

            var accessibilityEnabled = UIAccessibility.IsVoiceOverRunning;
            IosDependencyContainer.Instance.AnalyticsService.AccessibilityEnabled.Track(accessibilityEnabled);

            var watchservice = new WatchService();
            watchservice.TryLogWatchConnectivity();

            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Ambient);

            return true;
        }

        private void loginWithCredentialsIfNecessary(AccessLevel accessLevel)
        {
            if (accessLevel == AccessLevel.LoggedIn || accessLevel == AccessLevel.TokenRevoked)
            {
                IosDependencyContainer.Instance
                    .UserAccessManager
                    .LoginWithSavedCredentials();
            }
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            if (url.Scheme == ApplicationUrls.Scheme)
            {
                handleDeeplink(url);
                return true;
            }

            if (Xamarin.Essentials.Platform.OpenUrl(app, url, options))
            {
                return true;
            }

#if USE_ANALYTICS
            return SignIn.SharedInstance.HandleUrl(url);
#endif

            return false;
        }

        public override void ReceiveMemoryWarning(UIApplication application)
        {
            IosDependencyContainer.Instance.AnalyticsService.ReceivedLowMemoryWarning.Track(Platform.Daneel);
        }

        public override void ApplicationSignificantTimeChange(UIApplication application)
        {
            IosDependencyContainer.Instance.TimeService.SignificantTimeChanged();
        }

        private void navigateAccordingToAccessLevel(AccessLevel accessLevel, AppStart app)
        {
            if (accessLevel == AccessLevel.LoggedIn) app.ForceFullSync();

            var vc = accessLevel switch
            {
                AccessLevel.AccessRestricted => loadRootViewController<OutdatedAppViewModel, Unit>(),
                AccessLevel.NotLoggedIn => loadRootViewController<OnboardingViewModel, OnboardingParameters>(OnboardingParameters.Default),
                AccessLevel.TokenRevoked => loadRootViewController<TokenResetViewModel, Unit>(),
                AccessLevel.LoggedIn => loadRootViewController<MainTabBarViewModel, MainTabBarParameters>(MainTabBarParameters.Default)
            };
            Window.RootViewController = vc;
        }

        private UIViewController loadRootViewController<T, TInput>(TInput input = default) where T : ViewModel<TInput, Unit>
        {
            var viewModelLoader = IosDependencyContainer.Instance.ViewModelLoader;
            var viewModel = viewModelLoader.Load<T>();
            viewModel.Initialize(input);
            return ViewControllerLocator.GetViewController(viewModel);
        }
    }
}
