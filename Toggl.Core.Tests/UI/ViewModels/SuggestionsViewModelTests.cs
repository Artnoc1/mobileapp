﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Core.Interactors;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.Suggestions;
using Toggl.Core.Tests.Generators;
using Xunit;
using TimeEntry = Toggl.Core.Models.TimeEntry;
using Toggl.Core.Models.Interfaces;
using Toggl.Core.DataSources;
using System.Reactive.Subjects;
using Toggl.Core.UI.Extensions;
using Toggl.Shared.Extensions;
using Toggl.Core.Tests.TestExtensions;
using System.Collections.Immutable;
using Toggl.Core.Analytics;
using System.Collections.Generic;
using static Toggl.Core.Analytics.CalendarSuggestionProviderState;
using static Toggl.Core.Analytics.SuggestionPresentedEvent;

namespace Toggl.Core.Tests.UI.ViewModels
{
    public sealed class SuggestionsViewModelTests
    {
        public abstract class SuggestionsViewModelTest : BaseViewModelTests<SuggestionsViewModel>
        {
            protected override SuggestionsViewModel CreateViewModel()
                => new SuggestionsViewModel(DataSource, InteractorFactory, OnboardingStorage, SchedulerProvider, RxActionFactory, AnalyticsService, TimeService, PermissionsService);

            protected override void AdditionalViewModelSetup()
            {
                base.AdditionalViewModelSetup();

                var provider = Substitute.For<ISuggestionProvider>();
                provider.GetSuggestions().Returns(Observable.Empty<Suggestion>());
            }
        }

        public sealed class TheConstructor : SuggestionsViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(
                bool useDataSource,
                bool useOnboardingStorage,
                bool useInteractorFactory,
                bool useSchedulerProvider,
                bool useRxActionFactory,
                bool useAnalyticsService,
                bool useTimeService,
                bool usePermissionsService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var onboardingStorage = useOnboardingStorage ? OnboardingStorage : null;
                var interactorFactory = useInteractorFactory ? InteractorFactory : null;
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;
                var rxActionFactory = useRxActionFactory ? RxActionFactory : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var timeService = useTimeService ? TimeService : null;
                var permissionsService = usePermissionsService ? PermissionsService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SuggestionsViewModel(dataSource, interactorFactory, onboardingStorage, schedulerProvider, rxActionFactory, analyticsService, timeService, permissionsService);

                tryingToConstructWithEmptyParameters
                    .Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheSuggestionsProperty : SuggestionsViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task IsEmptyIfThereAreNoSuggestions()
            {
                InteractorFactory.GetSuggestions(Arg.Any<int>()).Execute().Returns(Observable.Return(new Suggestion[0]));
                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);
                TestScheduler.Start();

                var suggestions = observer.Messages.First().Value.Value;
                suggestions.Should().HaveCount(0);
            }

            [Fact, LogIfTooSlow]
            public async Task ReloadsSuggestionsWhenWorkspacesUpdate()
            {
                var workspaceUpdatedSubject = new Subject<Unit>();
                InteractorFactory.ObserveWorkspaceOrTimeEntriesChanges().Execute()
                    .Returns(workspaceUpdatedSubject.AsObservable());

                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);

                workspaceUpdatedSubject.OnNext(Unit.Default);

                TestScheduler.Start();

                observer.Messages.Should().HaveCount(2);
                observer.Messages.First().Value.Value.Should().HaveCount(0);
                observer.LastEmittedValue().Should().HaveCount(0);
            }

            [Fact, LogIfTooSlow]
            public async Task ReloadsSuggestionsWhenTimeEntriesUpdate()
            {
                var changesSubject = new Subject<Unit>();
                InteractorFactory.ObserveWorkspaceOrTimeEntriesChanges().Execute().Returns(changesSubject);

                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);

                changesSubject.OnNext(Unit.Default);

                TestScheduler.Start();

                observer.Messages.Should().HaveCount(2);
                observer.Messages.First().Value.Value.Should().HaveCount(0);
                observer.LastEmittedValue().Should().HaveCount(0);
            }

            [Fact, LogIfTooSlow]
            public async Task TracksSuggestionPresentedEvent()
            {
                var suggestions = prepareSuggestionsForSuggestionsPresentedEvent();
                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);
                TestScheduler.Start();

                AnalyticsService.Received().Track(Arg.Is<SuggestionPresentedEvent>(e =>
                    e.ToDictionary()[SuggestionProviderType.Calendar.ToString()] == "3"
                    && e.ToDictionary()[SuggestionProviderType.MostUsedTimeEntries.ToString()] == "2"
                    && e.ToDictionary()[SuggestionProviderType.RandomForest.ToString()] == "1"
                ));
            }

            [Fact, LogIfTooSlow]
            public async Task TracksSuggestionPresentedEventWhenCalendarUnauthorized()
            {
                PermissionsService.CalendarPermissionGranted.Returns(Observable.Return(false));
                var suggestions = prepareSuggestionsForSuggestionsPresentedEvent();
                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);
                TestScheduler.Start();

                AnalyticsService.Received().Track(Arg.Is<SuggestionPresentedEvent>(e =>
                    e.ToDictionary()[CalendarProviderStateName] == Unauthorized.ToString()
                ));
            }

            [Fact, LogIfTooSlow]
            public async Task TracksSuggestionPresentedEventWhenCalendarYieldsNoSuggestions()
            {
                PermissionsService.CalendarPermissionGranted.Returns(Observable.Return(true));
                var suggestions = prepareSuggestionsForSuggestionsPresentedEvent(hasCalendarSuggestions: false);
                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);
                TestScheduler.Start();

                AnalyticsService.Received().Track(Arg.Is<SuggestionPresentedEvent>(e =>
                    e.ToDictionary()[CalendarProviderStateName] == NoEvents.ToString()
                ));
            }

            [Fact, LogIfTooSlow]
            public async Task TracksSuggestionPresentedEventWhenCalendarYieldsSuggestions()
            {
                PermissionsService.CalendarPermissionGranted.Returns(Observable.Return(true));
                var suggestions = prepareSuggestionsForSuggestionsPresentedEvent();
                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);
                TestScheduler.Start();

                AnalyticsService.Received().Track(Arg.Is<SuggestionPresentedEvent>(e =>
                    e.ToDictionary()[CalendarProviderStateName] == SuggestionsAvailable.ToString()
                ));
            }

            [Fact, LogIfTooSlow]
            public async Task TracksSuggestionPresentedEventWithCorrectTotalCount()
            {
                var suggestions = prepareSuggestionsForSuggestionsPresentedEvent();
                var observer = TestScheduler.CreateObserver<IImmutableList<Suggestion>>();

                await ViewModel.Initialize();
                ViewModel.Suggestions.Subscribe(observer);
                TestScheduler.Start();

                AnalyticsService.Received().Track(Arg.Is<SuggestionPresentedEvent>(e =>
                    e.ToDictionary()[SuggestionsCountName] == suggestions.Length.ToString()
                ));
            }

            private Suggestion[] prepareSuggestionsForSuggestionsPresentedEvent(bool hasCalendarSuggestions = true)
            {
                var suggestions = new[] {
                    createDefaultSuggestionFor(SuggestionProviderType.MostUsedTimeEntries),
                    createDefaultSuggestionFor(SuggestionProviderType.MostUsedTimeEntries),
                    createDefaultSuggestionFor(SuggestionProviderType.RandomForest),
                };

                if (hasCalendarSuggestions)
                {
                    suggestions = suggestions.Concat(new[] {
                        createDefaultSuggestionFor(SuggestionProviderType.Calendar),
                        createDefaultSuggestionFor(SuggestionProviderType.Calendar),
                        createDefaultSuggestionFor(SuggestionProviderType.Calendar),
                    }).ToArray();
                }

                var getSuggestionsInteractor = Substitute.For<IInteractor<IObservable<IEnumerable<Suggestion>>>>();
                getSuggestionsInteractor.Execute().Returns(Observable.Return(suggestions));
                InteractorFactory.GetSuggestions(Arg.Any<int>()).Returns(getSuggestionsInteractor);
                var changeInteractor = Substitute.For<IInteractor<IObservable<Unit>>>();
                changeInteractor.Execute().Returns(Observable.Empty<Unit>());
                InteractorFactory.ObserveWorkspaceOrTimeEntriesChanges().Returns(changeInteractor);

                return suggestions;
            }

            private Suggestion createSuggestion(int index) => createSuggestion($"te{index}", 0, 0);

            private Suggestion createDefaultSuggestionFor(SuggestionProviderType type)
                => createSuggestion("Description", 12, 20, type);

            private Suggestion createSuggestion(string description, long taskId, long projectId, SuggestionProviderType type = SuggestionProviderType.MostUsedTimeEntries)
                => new Suggestion(
                    TimeEntry.Builder.Create(0)
                        .SetDescription(description)
                        .SetStart(DateTimeOffset.UtcNow)
                        .SetAt(DateTimeOffset.UtcNow)
                        .SetTaskId(taskId)
                        .SetProjectId(projectId)
                        .SetWorkspaceId(11)
                        .SetUserId(12)
                        .Build(),
                    type
                );

            private Recorded<Notification<Suggestion>> createRecorded(int ticks, Suggestion suggestion)
                => new Recorded<Notification<Suggestion>>(ticks, Notification.CreateOnNext(suggestion));
        }

        public sealed class TheStartTimeEntryAction : SuggestionsViewModelTest
        {
            public TheStartTimeEntryAction()
            {
                var user = Substitute.For<IThreadSafeUser>();
                user.Id.Returns(10);
                DataSource.User.Current.Returns(Observable.Return(user));

                TimeService.CurrentDateTime.Returns(DateTimeOffset.Now);
            }

            [Fact, LogIfTooSlow]
            public async Task CallsTheCreateTimeEntryInteractor()
            {
                var suggestion = createSuggestion();
                await ViewModel.Initialize();

                ViewModel.StartTimeEntry.Execute(suggestion);
                TestScheduler.Start();

                InteractorFactory.Received().StartSuggestion(suggestion);
            }

            [Fact, LogIfTooSlow]
            public async Task ExecutesTheContinueTimeEntryInteractor()
            {
                var suggestion = createSuggestion();
                var mockedInteractor = Substitute.For<IInteractor<IObservable<IThreadSafeTimeEntry>>>();
                InteractorFactory.StartSuggestion(Arg.Any<Suggestion>()).Returns(mockedInteractor);
                await ViewModel.Initialize();

                ViewModel.StartTimeEntry.Execute(suggestion);
                TestScheduler.Start();

                await mockedInteractor.Received().Execute();
            }

            [Fact, LogIfTooSlow]
            public async Task CanBeExecutedForTheSecondTimeIfStartingTheFirstOneFinishesSuccessfully()
            {
                var suggestion = createSuggestion();
                var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                var mockedInteractor = Substitute.For<IInteractor<IObservable<IThreadSafeTimeEntry>>>();
                InteractorFactory.StartSuggestion(Arg.Any<Suggestion>()).Returns(mockedInteractor);
                mockedInteractor.Execute()
                    .Returns(Observable.Return(timeEntry));
                await ViewModel.Initialize();

                var auxObservable = TestScheduler.CreateObserver<IThreadSafeTimeEntry>();
                ViewModel.StartTimeEntry.ExecuteSequentally(suggestion, suggestion)
                    .Subscribe(auxObservable);

                TestScheduler.Start();

                InteractorFactory.Received(2).StartSuggestion(suggestion);
            }

            [Fact, LogIfTooSlow]
            public async Task MarksTheActionForOnboardingPurposes()
            {
                var suggestion = createSuggestion();
                await ViewModel.Initialize();

                var auxObservable = TestScheduler.CreateObserver<IThreadSafeTimeEntry>();
                ViewModel.StartTimeEntry.ExecuteSequentally(suggestion, suggestion)
                    .Subscribe(auxObservable);

                TestScheduler.Start();

                OnboardingStorage.Received().SetTimeEntryContinued();
            }

            [Fact, LogIfTooSlow]
            public async Task TracksCalendarSuggestionContinued()
            {
                var suggestion = createSuggestion(SuggestionProviderType.Calendar);
                await ViewModel.Initialize();

                ViewModel.StartTimeEntry.Execute(suggestion);

                TestScheduler.Start();

                AnalyticsService.Received().Track(Arg.Is<CalendarSuggestionContinuedEvent>(e => true));
            }

            [Theory, LogIfTooSlow]
            [InlineData(SuggestionProviderType.Calendar)]
            [InlineData(SuggestionProviderType.MostUsedTimeEntries)]
            [InlineData(SuggestionProviderType.RandomForest)]
            public async Task TracksSuggestionStarted(SuggestionProviderType type)
            {
                var suggestion = createSuggestion(type);
                await ViewModel.Initialize();

                ViewModel.StartTimeEntry.Execute(suggestion);

                TestScheduler.Start();

                AnalyticsService.SuggestionStarted.Received().Track(type);
            }

            private Suggestion createSuggestion(SuggestionProviderType type = SuggestionProviderType.MostUsedTimeEntries)
            {
                var timeEntry = Substitute.For<IThreadSafeTimeEntry>();
                timeEntry.Duration.Returns((long)TimeSpan.FromMinutes(30).TotalSeconds);
                timeEntry.Description.Returns("Testing");
                timeEntry.WorkspaceId.Returns(10);
                return new Suggestion(timeEntry, type);
            }
        }
    }
}
