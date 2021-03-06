﻿using System;
using System.Linq;
using Foundation;
using ObjCRuntime;
using Toggl.iOS.Extensions;
using UIKit;

namespace Toggl.iOS
{
    public sealed partial class OnboardingPageView : UIView
    {
        private VideoView videoView;
        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                configureMessageAppearance();
            }
        }

        public static OnboardingPageView Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(OnboardingPageView), null, null);
            return Runtime.GetNSObject<OnboardingPageView>(arr.ValueAt(0));
        }

        protected OnboardingPageView(IntPtr handle) : base(handle)
        {
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            videoView?.RestartVideo();
        }

        public void SetVideo(NSUrl url)
        {
            if (videoView != null)
                throw new Exception("A video already exists for this onboarding page");

            videoView = new VideoView(url);
            SetContentView(videoView);
        }

        public void RestartAnimations()
        {
            videoView?.RestartVideo();
            if (ContentView.Subviews.Any() && ContentView.Subviews[0] is PeriscopeView)
            {
                (ContentView.Subviews[0] as PeriscopeView)?.restartAnimation(null);
            }
        }

        public void SetImageView(UIImage image)
        {
            var imageView = new UIImageView(image);
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            SetContentView(imageView);
        }

        public void SetContentView(UIView contentView)
        {
            if (ContentView.Subviews.Any())
                throw new Exception("A content view already exists for this onboarding page");

            contentView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.AddSubview(contentView);
            contentView.TopAnchor.ConstraintEqualTo(ContentView.TopAnchor).Active = true;
            contentView.BottomAnchor.ConstraintEqualTo(ContentView.BottomAnchor).Active = true;
            contentView.LeadingAnchor.ConstraintEqualTo(ContentView.LeadingAnchor).Active = true;
            contentView.TrailingAnchor.ConstraintEqualTo(ContentView.TrailingAnchor).Active = true;
        }

        public override void RemoveFromSuperview()
        {
            base.RemoveFromSuperview();
            ContentView.Subviews.ToList().ForEach(subview => subview?.Dispose());
        }

        private void configureMessageAppearance()
        {
            MessageLabel.SetTextWithOnboardingAppearance(message);
        }
    }
}
