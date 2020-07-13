using System.Linq;
using Toggl.Shared;
using Math = System.Math;

namespace Toggl.Core.UI.Helper
{
    public static class Colors
    {
        private static readonly Color lightishPink = new Color(229, 124, 216);
        private static readonly Color steel = new Color(142, 142, 147);
        private static readonly Color darkMint = new Color(76, 190, 100);
        private static readonly Color pinkishGrey = new Color(206, 206, 206);
        private static readonly Color black = new Color(46, 46, 46);
        private static readonly Color darkishPink = new Color(229, 124, 216);
        private static readonly Color macaroniAndCheese = new Color(241, 195, 63);
        private static readonly Color easterPurple = new Color(197, 107, 255);
        private static readonly Color nearlyWhite = new Color(250, 251, 252);
        private static readonly Color silver = new Color(181, 188, 192);
        private static readonly Color brownishGrey = new Color(94, 91, 91);
        private static readonly Color whiteTwo = new Color(244, 244, 244);
        private static readonly Color paleGreyTwo = nearlyWhite;
        private static readonly Color mediumPink = new Color(234, 70, 141);

        public static readonly Color Black = new Color(0, 0, 0);
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        public static class Siri
        {
            public static readonly Color AddButton = lightishPink;
            public static readonly Color InvocationPhrase = brownishGrey;
            public static readonly Color HeaderLabel = silver;
            public static readonly Color HeaderBackground = paleGreyTwo;
        }

        public static class Onboarding
        {
            internal static readonly Color TrackPageBorderColor = new Color(14, 150, 213);
            internal static readonly Color TrackPageBackgroundColor = darkishPink;

            internal static readonly Color LogPageBorderColor = new Color(165, 81, 220);
            internal static readonly Color LogPageBackgroundColor = new Color(187, 103, 241);

            internal static readonly Color ReportsPageBorderColor = new Color(230, 179, 31);
            internal static readonly Color ReportsPageBackgroundColor = macaroniAndCheese;

            public static readonly Color BlueColor = darkishPink;
            public static readonly Color PurpleColor = easterPurple;
            public static readonly Color YellowColor = macaroniAndCheese;
            public static readonly Color GrayColor = pinkishGrey;

            internal static readonly Color LoginPageBackgroundColor = darkishPink;
        }

        public static class Login
        {
            public static readonly Color EnabledButtonColor = new Color(255, 255, 255);
            public static readonly Color DisabledButtonColor = new Color(255, 255, 255, 122);
            public static readonly Color TextViewPlaceholder = pinkishGrey;
            public static readonly Color ForgotPassword = new Color(94, 91, 91);
        }

        public static class Signup
        {
            public static readonly Color HighlightedText = darkishPink;
        }

        public static class NavigationBar
        {
            public static readonly Color BackButton = new Color(94, 91, 91);
            public static readonly Color BackgroundColor = nearlyWhite;
        }

        public static class TabBar
        {
            public static readonly Color SelectedImageTintColor = darkishPink;
        }

        public static class Main
        {
            private static readonly Color lightGrey = new Color(181, 188, 192);

            public static readonly Color BackgroundColor = nearlyWhite;

            public static readonly Color SpiderNetColor = new Color(0, 0, 0);

            public static readonly Color CardBorder = new Color(232, 232, 232);

            public static readonly Color CurrentTimeEntryClientColor = new Color(94, 91, 91);

            public static readonly Color SuggestionsTitle = new Color(94, 91, 91);

            public static readonly Color SpiderHinge = new Color(50, 50, 50);

            public static readonly Color WelcomeBackText = new Color(181, 188, 192);

            public static readonly Color PullToRefresh = lightGrey;
            public static readonly Color Syncing = lightGrey;
            public static readonly Color Offline = lightGrey;
            public static readonly Color SyncFailed = lightGrey;
            public static readonly Color SyncCompleted = lightishPink;
        }

        public static class TimeEntriesLog
        {
            public static readonly Color ClientColor = new Color(163, 163, 163);

            public static readonly Color AddDescriptionTextColor = pinkishGrey;

            public static readonly Color SectionFooter = new Color(250, 251, 252);

            public static readonly Color DeleteSwipeActionBackground = new Color(247, 64, 73);

            public static readonly Color ContinueSwipeActionBackground = lightishPink;

            public static class Grouping
            {
                public static class Collapsed
                {
                    public static readonly Color Border = new Color(232, 232, 232);
                    public static readonly Color Background = new Color(255, 255, 255);
                    public static readonly Color Text = brownishGrey;
                }

                public static class Expanded
                {
                    public static readonly Color Border = new Color(255, 255, 255, 0);
                    public static readonly Color Background = whiteTwo;
                    public static readonly Color Text = lightishPink
       ;
                }

                public static class GroupedTimeEntry
                {
                    public static readonly Color Background = paleGreyTwo;
                }
            }
        }

        public static class StartTimeEntry
        {
            public static readonly Color Cursor = lightishPink;

            public static readonly Color Placeholder = pinkishGrey;

            public static readonly Color ActiveButton = lightishPink;

            public static readonly Color BoldQuerySuggestionColor = new Color(181, 188, 192);

            public static readonly Color InactiveButton = new Color(181, 188, 192);

            public static readonly Color SeparatorColor = new Color(181, 188, 192);

            public static readonly Color TokenText = new Color(94, 91, 91);

            public static readonly Color TokenBorder = new Color(232, 232, 232);

            public static readonly Color AddIconColor = lightishPink;
        }

        public static class DurationField
        {
            public static readonly Color Cursor = lightishPink;
        }

        public static class NoWorkspace
        {
            public static readonly Color ActivityIndicator = lightishPink;
            public static readonly Color DisabledCreateWorkspaceButton = new Color(255, 255, 255, 122);
        }

        public static class EditTimeEntry
        {
            public static readonly Color ClientText = new Color(94, 91, 91);
            public static readonly Color DescriptionCharacterCounter = new Color(247, 64, 73);
        }

        public static class EditDuration
        {
            public static readonly Color SetButton = lightishPink;
            public static readonly Color EditedTime = lightishPink;
            public static readonly Color NotEditedTime = black;

            public static class Wheel
            {
                public static readonly Color Background = new Color(244, 244, 244);
                public static readonly Color Shadow = new Color(94, 91, 91);
                public static readonly Color CapBackground = new Color(181, 188, 192);
                public static readonly Color Cap = new Color(255, 255, 255);
                public static readonly Color ThickMinuteSegment = new Color(188, 196, 199);
                public static readonly Color ThinMinuteSegment = new Color(230, 235, 237);

                public static readonly Color[] Rainbow =
                {
                    new Color("F1F2F3"),
                    new Color("DFC3E6"),
                    new Color("CA99D7"),
                    new Color("8799E5"),
                    new Color("5168C5"),
                    new Color("55BBDF"),
                    new Color("95D0E5"),
                    new Color("BFDBD7"),
                    new Color("7FC5BC"),
                    new Color("E5F0BA"),
                    new Color("F8DAB8"),
                    new Color("F0D06C"),
                    new Color("EFBA7A"),
                    new Color("F1ACAE")
                };
            }
        }

        public static class ModalDialog
        {
            public static readonly Color BackgroundOverlay = new Color(181, 188, 192);
        }

        public static class Suggestions
        {
            public static readonly Color ClientColor = new Color(94, 91, 91);
            public static readonly Color HeaderText = new Color(94, 91, 91);
        }

        public static class Settings
        {
            public static readonly Color SectionHeaderText = silver;
            public static readonly Color SyncStatusText = new Color(144, 146, 147);
            public static readonly Color Background = nearlyWhite;
            public static readonly Color DetailLabel = brownishGrey;
            public static readonly Color SeparatorColor = new Color(232, 232, 232);
        }

        public static class Feedback
        {
            public static readonly Color Cursor = lightishPink;
            public static readonly Color ActivityIndicator = lightishPink;
        }

        public static class Common
        {
            public static readonly Color PlaceholderText = pinkishGrey;
            public static readonly Color Transparent = new Color(0, 0, 0, alpha: 0);
            public static readonly Color LightGray = pinkishGrey;
            public static readonly Color Disabled = pinkishGrey;
            public static readonly Color TextColor = black;
        }

        public static class Reports
        {
            public static readonly Color PercentageActivated = lightishPink;

            public static readonly Color PercentageActivatedBackground = new Color(6, 170, 245, 61);

            public static readonly Color PercentageDisabled = new Color(181, 188, 192, 30);

            public static readonly Color TotalTimeActivated = lightishPink;

            public static readonly Color DayNotInMonth = new Color(149, 149, 149);

            public static readonly Color Disabled = new Color(181, 188, 192);

            public static class BarChart
            {
                public static readonly Color Legend = silver;

                public static readonly Color Billable = darkishPink;

                public static readonly Color NonBillable = new Color(71, 195, 252);

                public static readonly Color EmptyBar = brownishGrey;
            }

            public static readonly Color OtherProjectsSegmentBackground = pinkishGrey;

            public static class Donut
            {
                public static readonly Color InnerCircle = White;
            }

            public static class Loading
            {
                public static readonly Color LightColor = new Color(250, 251, 252);

                public static readonly Color DarkColor = new Color(236, 240, 242);
            }
        }

        public static class Licenses
        {
            public static readonly Color Border = new Color(232, 232, 232);
        }

        public static class Calendar
        {
            public static readonly Color EnableCalendarAction = lightishPink;
        }

        public static readonly Color[] DefaultProjectColors =
            Toggl.Core.Helper.Colors.DefaultProjectColors.Select(hex => new Color(hex)).ToArray();
    }
}
