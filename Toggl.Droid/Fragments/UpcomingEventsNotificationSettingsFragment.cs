﻿using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Toggl.Core.UI.ViewModels.Settings;
using Toggl.Droid.Adapters;
using Toggl.Shared.Extensions;

namespace Toggl.Droid.Fragments
{
    public sealed partial class UpcomingEventsNotificationSettingsFragment : ReactiveDialogFragment<UpcomingEventsNotificationSettingsViewModel>
    {
        private SelectCalendarNotificationsOptionAdapter adapter;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var contextThemeWrapper = new ContextThemeWrapper(Activity, Resource.Style.TogglDialog);
            var wrappedInflater = inflater.CloneInContext(contextThemeWrapper);

            var view = wrappedInflater.Inflate(Resource.Layout.UpcomingEventsNotificationSettingsFragment, container, false);
            InitializeViews(view);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            setupRecyclerView();

            setSmartRemindersTitle.Text = Shared.Resources.SetSmartReminders;
            setSmartRemindersMessage.Text = Shared.Resources.SetSmartRemindersMessage;

            adapter
                .ItemTapObservable
                .Subscribe(ViewModel.SelectOption.Inputs)
                .DisposedBy(DisposeBag);
        }

        public override void OnResume()
        {
            base.OnResume();
            var layoutParams = Dialog.Window.Attributes;
            layoutParams.Width = ViewGroup.LayoutParams.MatchParent;
            layoutParams.Height = ViewGroup.LayoutParams.WrapContent;
            Dialog.Window.Attributes = layoutParams;
        }

        private void setupRecyclerView()
        {
            adapter = new SelectCalendarNotificationsOptionAdapter();
            adapter.Items = ViewModel.AvailableOptions;
            recyclerView.SetAdapter(adapter);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Context));
        }
    }
}
