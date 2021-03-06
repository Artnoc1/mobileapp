﻿using FluentAssertions;
using FsCheck;
using Microsoft.Reactive.Testing;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute.ClearExtensions;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework.Internal;
using Toggl.Core.Analytics;
using Toggl.Core.Exceptions;
using Toggl.Core.Helper;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.Tests.Generators;
using Toggl.Core.Tests.TestExtensions;
using Toggl.Core.UI;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Parameters;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.UI.Views;
using Toggl.Networking;
using Toggl.Networking.ApiClients;
using Toggl.Networking.Exceptions;
using Toggl.Networking.Network;
using Toggl.Shared;
using Toggl.Storage.Settings;
using Xunit;

namespace Toggl.Core.Tests.UI.ViewModels
{
    public sealed class LoginViewModelTests
    {
        public abstract class LoginViewModelTest : BaseViewModelWithInputTests<LoginViewModel, CredentialsParameter>
        {
            protected readonly ITogglApi TogglApi = Substitute.For<ITogglApi>();
            protected readonly IUserApi UserApi = Substitute.For<IUserApi>();
            protected Email ValidEmail { get; } = Email.From("person@company.com");
            protected Email InvalidEmail { get; } = Email.From("this is not an email");

            protected Password ValidPassword { get; } = Password.From("T0t4lly s4afe p4$$");
            protected Password InvalidPassword { get; } = Password.From("123");
            protected Password EmptyPassword { get; } = Password.From("");

            protected ILastTimeUsageStorage LastTimeUsageStorage { get; } = Substitute.For<ILastTimeUsageStorage>();

            protected override LoginViewModel CreateViewModel()
                => new LoginViewModel(
                    UserAccessManager,
                    AnalyticsService,
                    OnboardingStorage,
                    NavigationService,
                    ErrorHandlingService,
                    LastTimeUsageStorage,
                    TimeService,
                    SchedulerProvider,
                    RxActionFactory,
                    InteractorFactory);

            protected override void AdditionalSetup()
            {
                UserAccessManager.UserLoggedIn.Returns(Observable.Return(TogglApi));
                var container = new TestDependencyContainer { MockSyncManager = SyncManager };
                TestDependencyContainer.Initialize(container);
            }
        }

        public sealed class TheConstructor : LoginViewModelTest
        {
            [Xunit.Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useUserAccessManager,
                bool useAnalyticsService,
                bool useOnboardingStorage,
                bool userNavigationService,
                bool useApiErrorHandlingService,
                bool useLastTimeUsageStorage,
                bool useTimeService,
                bool useSchedulerProvider,
                bool useRxActionFactory,
                bool useInteractorFactory)
            {
                var userAccessManager = useUserAccessManager ? UserAccessManager : null;
                var analyticsSerivce = useAnalyticsService ? AnalyticsService : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var navigationService = userNavigationService ? NavigationService : null;
                var apiErrorHandlingService = useApiErrorHandlingService ? ErrorHandlingService : null;
                var lastTimeUsageStorage = useLastTimeUsageStorage ? LastTimeUsageStorage : null;
                var timeService = useTimeService ? TimeService : null;
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;
                var rxActionFactory = useRxActionFactory ? RxActionFactory : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new LoginViewModel(userAccessManager,
                                             analyticsSerivce,
                                             onboardingStorage,
                                             navigationService,
                                             apiErrorHandlingService,
                                             lastTimeUsageStorage,
                                             timeService,
                                             schedulerProvider,
                                             rxActionFactory,
                                             interactorFactory);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheLoginMethod : LoginViewModelTest
        {
            [Fact, LogIfTooSlow]
            public void CallsTheUserAccessManagerWhenTheEmailAndPasswordAreValid()
            {
                ViewModel.Email.Accept(ValidEmail);
                ViewModel.Password.Accept(ValidPassword);

                ViewModel.Login.Execute();

                UserAccessManager.Received().Login(Arg.Is(ValidEmail), Arg.Is(ValidPassword));
            }

            [Fact, LogIfTooSlow]
            public void DoesNothingWhenThePageIsCurrentlyLoading()
            {
                var never = Observable.Never<ITogglApi>();
                UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>()).Returns(never);
                ViewModel.Email.Accept(ValidEmail);
                ViewModel.Password.Accept(ValidPassword);
                ViewModel.Login.Execute();

                ViewModel.Login.Execute();

                UserAccessManager.Received(1).Login(Arg.Any<Email>(), Arg.Any<Password>());
            }

            public sealed class WhenLoginSucceeds : LoginViewModelTest
            {
                public WhenLoginSucceeds()
                {
                    TogglApi.User.Returns(UserApi);
                    ViewModel.Initialize(CredentialsParameter.Empty).Wait();
                    ViewModel.Email.Accept(ValidEmail);
                    ViewModel.Password.Accept(ValidPassword);
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Return(TogglApi));
                }

                [Fact, LogIfTooSlow]
                public void SetsIsNewUserToFalse()
                {
                    ViewModel.Login.Execute();

                    OnboardingStorage.Received().SetIsNewUser(false);
                }

                [Fact, LogIfTooSlow]
                public async Task NavigatesToTheTimeEntriesViewModel()
                {
                    ViewModel.Login.Execute();

                    await NavigationService.Received()
                        .Navigate<MainTabBarViewModel, MainTabBarParameters>(MainTabBarParameters.Default,
                            ViewModel.View);
                }

                [Fact, LogIfTooSlow]
                public async Task LinksAccountIfNeeded()
                {
                    ViewModel.Initialize(CredentialsParameter.With(ValidEmail, ValidPassword, true, "code")).Wait();
                    UserApi.LinkSso(Arg.Any<Email>(), Arg.Any<string>()).Returns("");

                    ViewModel.Login.Execute();

                    await NavigationService.Received()
                        .Navigate<MainTabBarViewModel, MainTabBarParameters>(Arg.Is<MainTabBarParameters>(parameters => parameters.LinkResult == MainTabBarParameters.SsoLinkResult.SUCCESS),
                            ViewModel.View);
                }

                [Fact, LogIfTooSlow]
                public async Task TriesToLinkAccountsAndReportsErrorIfEmailsDontMatch()
                {
                    ViewModel.Initialize(CredentialsParameter.With(ValidEmail, ValidPassword, true, "code")).Wait();
                    var request = Substitute.For<IRequest>();
                    var response = Substitute.For<IResponse>();
                    response.RawData.Returns("");
                    var exception = new BadSsoEmailException(request, response);
                    UserApi.LinkSso(Arg.Any<Email>(), Arg.Any<string>())
                        .Returns(Task.FromException<string>(exception));

                    ViewModel.Login.Execute();

                    await NavigationService.Received()
                        .Navigate<MainTabBarViewModel, MainTabBarParameters>(Arg.Is<MainTabBarParameters>(parameters => parameters.LinkResult == MainTabBarParameters.SsoLinkResult.BAD_EMAIL_ERROR),
                            ViewModel.View);
                }

                [Fact, LogIfTooSlow]
                public async Task TriesToLinkAccountAndReportsError()
                {
                    ViewModel.Initialize(CredentialsParameter.With(ValidEmail, ValidPassword, true, "code")).Wait();
                    UserApi.LinkSso(Arg.Any<Email>(), Arg.Any<string>())
                        .Returns(Task.FromException<string>(new Exception()));

                    ViewModel.Login.Execute();

                    await NavigationService.Received()
                        .Navigate<MainTabBarViewModel, MainTabBarParameters>(Arg.Is<MainTabBarParameters>(parameters => parameters.LinkResult == MainTabBarParameters.SsoLinkResult.GENERIC_ERROR),
                            ViewModel.View);
                }

                [Fact, LogIfTooSlow]
                public void TracksTheLoginEvent()
                {
                    ViewModel.Login.Execute();

                    AnalyticsService.Received().Login.Track(AuthenticationMethod.EmailAndPassword);
                }

                [Fact, LogIfTooSlow]
                public void ReportsUserIdToAnalytics()
                {
                    var id = 1234567890L;
                    var user = Substitute.For<IThreadSafeUser>();
                    user.Id.Returns(id);
                    var observable = Observable.Return(user);
                    InteractorFactory.GetCurrentUser().Execute().Returns(observable);

                    ViewModel.Login.Execute();

                    AnalyticsService.Received().SetUserId(id);
                }

                [FsCheck.Xunit.Property(MaxTest = 1)]
                public void SavesTheTimeOfLastLogin(DateTimeOffset now)
                {
                    TimeService.CurrentDateTime.Returns(now);
                    var viewModel = CreateViewModel();
                    viewModel.AttachView(View);
                    viewModel.Email.Accept(ValidEmail);
                    viewModel.Password.Accept(ValidPassword);

                    ViewModel.Login.Execute();

                    LastTimeUsageStorage.Received().SetLogin(Arg.Is(now));
                }

                [Fact]
                public void ExecutesTheCreateOnboardingTimeEntryIfNeededInteractor()
                {
                    ViewModel.Login.Execute();

                    InteractorFactory.CreateOnboardingTimeEntryIfNeeded().Received().Execute();
                }
            }

            public sealed class WhenLoginFails : LoginViewModelTest
            {
                public WhenLoginFails()
                {
                    ViewModel.Email.Accept(ValidEmail);
                    ViewModel.Password.Accept(ValidPassword);
                }

                [Fact, LogIfTooSlow]
                public void SetsIsLoadingToFalse()
                {
                    var observer = TestScheduler.CreateObserver<bool>();
                    ViewModel.IsLoading.Subscribe(observer);
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglApi>(new Exception()));

                    ViewModel.Login.Execute();

                    TestScheduler.Start();
                    observer.Messages.AssertEqual(
                        ReactiveTest.OnNext(1, false),
                        ReactiveTest.OnNext(2, true),
                        ReactiveTest.OnNext(3, false)
                    );
                }

                [Fact, LogIfTooSlow]
                public void DoesNotNavigate()
                {
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglApi>(new Exception()));

                    ViewModel.Login.Execute();

                    NavigationService.DidNotReceive().Navigate<MainViewModel>(ViewModel.View);
                }

                [Fact, LogIfTooSlow]
                public void SetsTheErrorMessageToIncorrectEmailOrPasswordWhenReceivedUnauthorizedException()
                {
                    var observer = TestScheduler.CreateObserver<string>();
                    ViewModel.LoginErrorMessage.Subscribe(observer);
                    var exception = new UnauthorizedException(
                        Substitute.For<IRequest>(), Substitute.For<IResponse>());
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglApi>(exception));

                    ViewModel.Login.Execute();

                    TestScheduler.Start();
                    observer.Messages.AssertEqual(
                        ReactiveTest.OnNext(1, ""),
                        ReactiveTest.OnNext(2, Resources.IncorrectEmailOrPasswordPleaseTryAgain)
                    );
                }

                [Fact, LogIfTooSlow]
                public void TracksIncorrectEmailOrPasswordLoginFailureWhenReceivedUnauthorizedException()
                {
                    var exception = new UnauthorizedException(
                        Substitute.For<IRequest>(), Substitute.For<IResponse>());
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglApi>(exception));

                    ViewModel.Login.Execute();

                    AnalyticsService.IncorrectEmailOrPasswordLoginFailure.Received().Track();
                }

                [Fact, LogIfTooSlow]
                public void TracksTrueForLocalEmailValidationLoginCheckWhenEmailIsValid()
                {
                    ViewModel.Login.Execute();

                    AnalyticsService.LocalEmailValidationLoginCheck.Received().Track(true);
                }

                [Fact, LogIfTooSlow]
                public void TracksFalseForLocalEmailValidationLoginCheckWhenEmailIsNotValid()
                {
                    ViewModel.Email.Accept(InvalidEmail);

                    ViewModel.Login.Execute();

                    AnalyticsService.LocalEmailValidationLoginCheck.Received().Track(false);
                }

                [Fact, LogIfTooSlow]
                public void TracksTrueForLocalPasswordValidationLoginCheckWhenEmailIsValid()
                {
                    ViewModel.Login.Execute();

                    AnalyticsService.LocalPasswordValidationLoginCheck.Received().Track(true);
                }

                [Fact, LogIfTooSlow]
                public void TracksFalseForLocalPasswordValidationLoginCheckWhenEmailIsNotValid()
                {
                    ViewModel.Password.Accept(EmptyPassword);

                    ViewModel.Login.Execute();

                    AnalyticsService.LocalPasswordValidationLoginCheck.Received().Track(false);
                }

                [Fact, LogIfTooSlow]
                public void SetsTheErrorMessageToGenericLoginErrorForAnyOtherException()
                {
                    var observer = TestScheduler.CreateObserver<string>();
                    var exception = new Exception();
                    ViewModel.LoginErrorMessage.Subscribe(observer);
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglApi>(exception));

                    ViewModel.Login.Execute();

                    TestScheduler.Start();
                    observer.Messages.AssertEqual(
                        ReactiveTest.OnNext(1, ""),
                        ReactiveTest.OnNext(2, Resources.GenericLoginError)
                    );
                }

                [Fact, LogIfTooSlow]
                public void DoesNothingWhenErrorHandlingServiceHandlesTheException()
                {
                    var observer = TestScheduler.CreateObserver<string>();
                    var exception = new Exception();
                    ViewModel.LoginErrorMessage.Subscribe(observer);
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglApi>(exception));
                    ErrorHandlingService.TryHandleDeprecationError(Arg.Any<Exception>())
                        .Returns(true);

                    ViewModel.Login.Execute();

                    TestScheduler.Start();
                    observer.Messages.AssertEqual(
                        ReactiveTest.OnNext(1, "")
                    );
                }

                [Fact, LogIfTooSlow]
                public void TracksTheEventAndException()
                {
                    var exception = new Exception();
                    UserAccessManager.Login(Arg.Any<Email>(), Arg.Any<Password>())
                        .Returns(Observable.Throw<ITogglApi>(exception));

                    ViewModel.Login.Execute();

                    AnalyticsService.UnknownLoginFailure.Received()
                        .Track(exception.GetType().FullName, exception.Message);
                    AnalyticsService.Received().TrackAnonymized(exception);
                }
            }
        }

        public sealed class TheSignupCommand : LoginViewModelTest
        {
            [FsCheck.Xunit.Property]
            public void NavigatesToTheSignUpViewModel(
                NonEmptyString emailString, NonEmptyString passwordString)
            {
                var email = Email.From(emailString.Get);
                var password = Password.From(passwordString.Get);
                ViewModel.Email.Accept(email);
                ViewModel.Password.Accept(password);

                ViewModel.SignUp.Execute();

                TestScheduler.Start();
                NavigationService
                    .Received()
                    .Navigate<SignUpViewModel, CredentialsParameter>(
                        Arg.Is<CredentialsParameter>(parameter
                            => parameter.Email.Equals(email)
                            && parameter.Password.Equals(password)
                        ), ViewModel.View
                    ).Wait();
            }
        }

        public sealed class ThePrepareMethod : LoginViewModelTest
        {
            [Xunit.Theory]
            [InlineData("valid.email@company.com")]
            [InlineData("not a valid email")]
            [InlineData("almost.valid@company")]
            public void SetsTheEmail(string emailString)
            {
                var viewModel = CreateViewModel();
                viewModel.AttachView(View);
                var email = Email.From(emailString);
                var password = Password.Empty;
                var parameter = CredentialsParameter.With(email, password);
                var expectedValues = new[] { Email.Empty, email.TrimmedEnd() };
                var observer = TestScheduler.CreateObserver<Email>();
                viewModel.Email.Subscribe(observer);

                viewModel.Initialize(parameter);
                TestScheduler.Start();

                observer.Values().Should().BeEquivalentTo(expectedValues);
            }

            [Xunit.Theory]
            [InlineData("")]
            [InlineData("1")]
            [InlineData("12")]
            [InlineData("qwertyASDF123^*)k.")]
            public void SetsThePassword(string passwordString)
            {
                var viewModel = CreateViewModel();
                viewModel.AttachView(View);
                var email = Email.Empty;
                var password = Password.From(passwordString);
                var parameter = CredentialsParameter.With(email, password);
                var expectedValues = new[] { Password.Empty, password };
                var observer = TestScheduler.CreateObserver<Password>();
                viewModel.Password.Subscribe(observer);

                viewModel.Initialize(parameter);

                TestScheduler.Start();
                observer.Values().Should().BeEquivalentTo(expectedValues);
            }
        }
    }
}
