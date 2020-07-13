using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System;
using System.Reactive;
using AndroidX.AppCompat.App;
using Toggl.Core;
using Toggl.Core.UI;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.Droid.Activities;
using Toggl.Droid.BroadcastReceivers;
using Toggl.Droid.Presentation;
using static Android.Content.Intent;

namespace Toggl.Droid
{
    [Activity(Label = "Toggl for Devs",
              MainLauncher = true,
              Icon = "@mipmap/ic_launcher",
              Theme = "@style/Theme.Splash",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [IntentFilter(
        new[] { "android.intent.action.VIEW", "android.intent.action.EDIT" },
        Categories = new[] { "android.intent.category.BROWSABLE", "android.intent.category.DEFAULT" },
        DataSchemes = new[] { "toggl" },
        DataHost = "*")]
    [IntentFilter(
        new[] { "android.intent.action.PROCESS_TEXT" },
        Categories = new[] { "android.intent.category.DEFAULT" },
        DataMimeType = "text/plain")]
    public partial class SplashScreen : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var dependencyContainer = AndroidDependencyContainer.Instance;
            registerTimezoneChangedBroadcastReceiver(dependencyContainer.TimeService);

            var app = new AppStart(dependencyContainer);
            app.UpdateOnboardingProgress();

            var accessLevel = app.GetAccessLevel();
            if (accessLevel != AccessLevel.LoggedIn)
            {
                var intent = createIntentForNotLoggedInAccessLevel(accessLevel);
                StartActivity(intent);
                Finish();
                return;
            }

            clearAllViewModelsAndSetupRootViewModel(
                dependencyContainer.ViewModelCache,
                dependencyContainer.ViewModelLoader);

            var navigationUrl = Intent.Data?.ToString() ?? getTrackUrlFromProcessedText();
            if (string.IsNullOrEmpty(navigationUrl))
            {
                app.ForceFullSync();
                StartActivity(typeof(MainTabBarActivity));
                Finish();
                return;
            }

            handleDeepLink(new Uri(navigationUrl), dependencyContainer);
            return;
        }

        private void clearAllViewModelsAndSetupRootViewModel(ViewModelCache viewModelCache, ViewModelLoader viewModelLoader)
        {
            viewModelCache.ClearAll();
            var viewModel = viewModelLoader.Load<MainTabBarViewModel>();
            viewModelCache.Cache(viewModel);

            viewModel.Initialize(MainTabBarParameters.Default);
        }

        private void registerTimezoneChangedBroadcastReceiver(ITimeService timeService)
        {
            var togglApplication = getTogglApplication();
            var currentTimezoneChangedBroadcastReceiver = togglApplication.TimezoneChangedBroadcastReceiver;
            if (currentTimezoneChangedBroadcastReceiver != null)
            {
                Application.UnregisterReceiver(currentTimezoneChangedBroadcastReceiver);
            }

            togglApplication.TimezoneChangedBroadcastReceiver = new TimezoneChangedBroadcastReceiver(timeService);
            ApplicationContext.RegisterReceiver(togglApplication.TimezoneChangedBroadcastReceiver, new IntentFilter(ActionTimezoneChanged));
        }

        private TogglApplication getTogglApplication()
            => (TogglApplication)Application;

        private Intent createIntentForNotLoggedInAccessLevel(AccessLevel accessLevel)
        {
            switch (accessLevel)
            {
                case AccessLevel.AccessRestricted:
                    loadAndCacheViewModelWithParams<OutdatedAppViewModel, Unit>(Unit.Default);
                    return new Intent(this, typeof(OutdatedAppActivity))
                        .AddFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                
                case AccessLevel.NotLoggedIn:
                    loadAndCacheViewModelWithParams<OnboardingViewModel, OnboardingParameters>(OnboardingParameters.Default);
                    return new Intent(this, typeof(OnboardingActivity))
                        .AddFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);

                case AccessLevel.TokenRevoked:
                    loadAndCacheViewModelWithParams<TokenResetViewModel, Unit>(Unit.Default);
                    return new Intent(this, typeof(TokenResetActivity))
                        .AddFlags(ActivityFlags.ClearTop | ActivityFlags.ClearTask | ActivityFlags.NewTask);
                
                default:
                    throw new ArgumentException("Invalid not logged in access level");
            }
        }

        public override void Finish()
        {
            base.Finish();
            OverridePendingTransition(0, Transitions.Fade.OtherIn);
        }
    }
}
