// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.iOS.Views.Tag
{
	[Register ("NewTagViewCell")]
	partial class NewTagViewCell
	{
		[Outlet]
		UIKit.UIView CheckedImageView { get; set; }

		[Outlet]
		UIKit.UILabel TextLabel { get; set; }

		[Outlet]
		UIKit.UIImageView UncheckedImageView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (UncheckedImageView != null) {
				UncheckedImageView.Dispose ();
				UncheckedImageView = null;
			}

			if (CheckedImageView != null) {
				CheckedImageView.Dispose ();
				CheckedImageView = null;
			}

			if (TextLabel != null) {
				TextLabel.Dispose ();
				TextLabel = null;
			}
		}
	}
}
