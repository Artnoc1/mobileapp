using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Toggl.Droid.Widgets.Services;

namespace Toggl.Droid.Widgets
{
    [BroadcastReceiver(Label = "Toggl Suggestions Widget", Exported = true)]
    [IntentFilter(new string[] { AppWidgetManager.ActionAppwidgetUpdate })]
    [MetaData("android.appwidget.provider", Resource = "@xml/suggestionswidgetprovider")]
    public class SuggestionsWidget : AppWidgetProvider
    {
        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);
        }

        public override void OnDeleted(Context context, int[] appWidgetIds)
        {
            reportInstallationState(context, false);
            base.OnDeleted(context, appWidgetIds);
        }

        public override void OnEnabled(Context context)
        {
            reportInstallationState(context, true);
            base.OnEnabled(context);
        }

        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
        }

        public override void OnAppWidgetOptionsChanged(Context context, AppWidgetManager appWidgetManager, int appWidgetId, Bundle newOptions)
        {
            base.OnAppWidgetOptionsChanged(context, appWidgetManager, appWidgetId, newOptions);
        }

        private void reportInstallationState(Context context, bool installed)
        {
            var intent = new Intent(context, typeof(WidgetsAnalyticsService));
            intent.SetAction(WidgetsAnalyticsService.TimerWidgetInstallAction);
            intent.PutExtra(WidgetsAnalyticsService.TimerWidgetInstallStateParameter, installed);
            WidgetsAnalyticsService.EnqueueWork(context, intent);
        }
    }
}
