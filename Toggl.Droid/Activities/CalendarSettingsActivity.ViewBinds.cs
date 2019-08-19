﻿using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Toggl.Droid.Activities
{
    public sealed partial class CalendarSettingsActivity
    {
        private View toggleCalendarsView;
        private Switch toggleCalendarsSwitch;
        private View calendarsContainer;
        private RecyclerView calendarsRecyclerView;
        private Toolbar toolbar;
        private TextView linkCalendarsTitle;
        private TextView linkCalendarsMessage;
        private TextView selectCalendarsTitle;
        private TextView selectCalendarsMessage;

        protected override void InitializeViews()
        {
            linkCalendarsTitle = FindViewById<TextView>(Resource.Id.LinkCalendarsTitle);
            linkCalendarsMessage = FindViewById<TextView>(Resource.Id.LinkCalendarsMessage);
            selectCalendarsTitle = FindViewById<TextView>(Resource.Id.SelectCalendarsTitle);
            selectCalendarsMessage = FindViewById<TextView>(Resource.Id.SelectCalendarsMessage);
            toggleCalendarsView = FindViewById(Resource.Id.ToggleCalendarsView);
            toggleCalendarsSwitch = FindViewById<Switch>(Resource.Id.ToggleCalendarsSwitch);
            calendarsContainer = FindViewById(Resource.Id.CalendarsContainer);
            calendarsRecyclerView = FindViewById<RecyclerView>(Resource.Id.CalendarsRecyclerView);
            toolbar = FindViewById<Toolbar>(Resource.Id.Toolbar);
        }
    }
}
