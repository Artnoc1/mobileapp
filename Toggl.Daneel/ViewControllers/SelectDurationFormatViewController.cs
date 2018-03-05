﻿using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public partial class SelectDurationFormatViewController
        : MvxViewController<SelectDurationFormatViewModel>
    {
        public SelectDurationFormatViewController()
            : base(nameof(SelectDurationFormatViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new DurationFormatsTableViewSource(DurationFormatsTableView);
            DurationFormatsTableView.Source = source;

            var bindingSet = this.CreateBindingSet<SelectDurationFormatViewController, SelectDurationFormatViewModel>();

            bindingSet.Bind(source).To(vm => vm.DurationFormats);

            bindingSet.Bind(BackButton).To(vm => vm.CloseCommand);

            bindingSet.Bind(source)
                      .For(v => v.SelectionChangedCommand)
                      .To(vm => vm.SelectDurationFormatCommand);

            bindingSet.Apply();
        }
    }
}

