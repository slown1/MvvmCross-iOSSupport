﻿using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Support.Presenters;
using MvvmCross.iOS.Views;
using UIKit;

namespace MvvmCross.iOS.Support.Views
{
	public class MvxBaseTabBarViewController : MvxTabBarViewController, IMvxTabBarViewController
	{
		private int _tabsCount = 0;

		public MvxBaseTabBarViewController() : base()
		{
			// WORKAROUND: UIKit makes a first ViewDidLoad call, because a TabViewController expects it's view (tabs) to be drawn 
			// on construction. Therefore we need to call ViewDidLoad "manually", otherwise ViewModel will be null
			ViewDidLoad();
		}

		public MvxBaseTabBarViewController(IntPtr handle) : base(handle)
		{
		}

		public virtual void ShowTabView(UIViewController viewController, bool wrapInNavigationController, IDictionary<string, string> presentationValues)
		{
			// get parameter values
			var title = string.Empty;
			var iconName = string.Empty;

			// TODO: remove hard-coded keys! To do so, there must be consts defined at Core level
			presentationValues.TryGetValue("title", out title);
			presentationValues.TryGetValue("icon_name", out iconName);

			// setup Tab
			this.SetTitleAndTabBarItem(viewController, title, iconName);

			// add Tab
			var currentTabs = new List<UIViewController>();
			if(ViewControllers != null)
			{
				currentTabs = this.ViewControllers.ToList();
			}

			if(wrapInNavigationController)
			{
				var navController = new UINavigationController();

				navController.NavigationBar.Translucent = false;

				navController.PushViewController(viewController, true);
				currentTabs.Add(navController);
			}
			else
			{
				currentTabs.Add(viewController);
			}
			// update current Tabs
			ViewControllers = currentTabs.ToArray();
		}

		protected virtual void SetTitleAndTabBarItem(UIViewController viewController, string title, string iconName)
		{
			viewController.Title = title;
			viewController.TabBarItem = new UITabBarItem(title, UIImage.FromBundle(iconName), this._tabsCount);
			_tabsCount++;
		}

		public virtual bool ShowChildView(UIViewController viewController)
		{
			var navigationController = (UINavigationController)SelectedViewController;

			// if the current selected ViewController is not a NavigationController, then a child cannot be shown
			if(navigationController == null)
			{
				return false;
			}

			navigationController.PushViewController((UIViewController)viewController, true);

			return true;
		}

		public virtual bool CloseChildViewModel(IMvxViewModel viewModel)
		{
			// current implementation assumes the ViewModel to close is the currently shown ViewController 
			var navController = SelectedViewController as UINavigationController;
			if(navController != null)
			{
				navController.PopViewController(true);
				return true;
			}
			return false;
		}

		public void PresentViewControllerWithNavigation(UIViewController controller, bool animated = true, Action completionHandler = null)
		{
			PresentViewController(new UINavigationController(controller), animated, completionHandler);
		}
	}

	public abstract class MvxBaseTabBarViewController<TViewModel> : MvxBaseTabBarViewController where TViewModel : IMvxViewModel
	{
		public new TViewModel ViewModel
		{
			get { return (TViewModel)base.ViewModel; }
			set { base.ViewModel = value; }
		}

		public virtual UIViewController VisibleUIViewController
		{
			get
			{
				var topViewController = (SelectedViewController as UINavigationController).TopViewController;
				if(topViewController.PresentedViewController != null)
				{
					var presentedNavigationController = (topViewController.PresentedViewController as UINavigationController);
					if(presentedNavigationController != null)
					{
						return presentedNavigationController.TopViewController;
					}
					else
					{
						return topViewController.PresentedViewController;
					}
				}
				else
				{
					return topViewController;
				}
			}
		}

		public MvxBaseTabBarViewController() : base()
		{
		}

		public MvxBaseTabBarViewController(IntPtr handle) : base(handle)
		{
		}
	}
}
