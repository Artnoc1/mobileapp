﻿using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class CalendarViewModel : MvxViewModel
    {
        private readonly ITimeService timeService;
        private readonly IInteractorFactory interactorFactory;
        private readonly IPermissionsService permissionsService;
        private readonly IMvxNavigationService navigationService;

        public CalendarViewModel(
            ITimeService timeService,
            IInteractorFactory interactorFactory,
            IPermissionsService permissionsService,
            IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(permissionsService, nameof(permissionsService));

            this.timeService = timeService;
            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;
            this.permissionsService = permissionsService;
        }
    }
}
