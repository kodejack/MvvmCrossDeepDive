﻿using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Fragging;
using Cirrious.MvvmCross.Droid.Fragging.Fragments;
using Cirrious.MvvmCross.ViewModels;
using MvvmCrossMenu.Core.ViewModels;
using MvvmCrossMenu.Droid.Helpers;
using MvvmCrossMenu.Droid.Views.Fragments;
using MvvmCrossMenu.Models;

namespace MvvmCrossMenu.Droid.Views
{
	[Activity (Label = "MvvmCrossMenu", LaunchMode = LaunchMode.SingleTop, Theme = "@style/MyTheme")]	
	public class RootView : MvxFragmentActivity, IFragmentHost
	{
		private DrawerLayout _drawer;
		private CustomActionBarDrawerToggle _drawerToggle;
		private string _drawerTitle;
		private string _title;
		private View _drawerList;

		private RootViewModel _viewModel;
		public new RootViewModel ViewModel
		{
			get { return this._viewModel ?? (this._viewModel = base.ViewModel as RootViewModel); }
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView(Resource.Layout.RootView);

			this._title = this._drawerTitle = this.Title;
			this._drawer = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			this._drawerList = this.FindViewById<View>(Resource.Id.left_drawer);

			this._drawer.SetDrawerShadow(Resource.Drawable.drawer_shadow_dark, (int)GravityFlags.Start);

			this.ActionBar.SetDisplayHomeAsUpEnabled(true);
			this.ActionBar.SetHomeButtonEnabled(true);

			//DrawerToggle is the animation that happens with the indicator next to the
			//ActionBar icon. You can choose not to use this.
			this._drawerToggle = new CustomActionBarDrawerToggle(this, this._drawer,
				Resource.Drawable.ic_drawer_light,
				Resource.String.drawer_open,
				Resource.String.drawer_close);

			//You can alternatively use _drawer.DrawerClosed here
			this._drawerToggle.DrawerClosed += delegate
			{
				this.ActionBar.Title = this._title;
				this.InvalidateOptionsMenu();
			};


			//You can alternatively use _drawer.DrawerOpened here
			this._drawerToggle.DrawerOpened += delegate
			{
				this.ActionBar.Title = this._drawerTitle;
				this.InvalidateOptionsMenu();
			};

			this._drawer.SetDrawerListener(this._drawerToggle);


			this.RegisterForDetailsRequests();

			if (null == bundle)
			{
				this.ViewModel.SelectMenuItemCommand.Execute(this.ViewModel.MenuItems[0]);
			}
		}



		/// <summary>
		/// Use the custom presenter to determine if we can navigate forward.
		/// </summary>
		private void RegisterForDetailsRequests()
		{
			var customPresenter = Mvx.Resolve<ICustomPresenter>();
			customPresenter.Register(typeof(FirstViewModel), this);
//			customPresenter.Register(typeof(BrowseViewModel), this);
//			customPresenter.Register(typeof(ProfileViewModel), this);

		}

		/// <summary>
		/// Read all about this, but this is a nice way if there were multiple
		/// fragments on the screen for us to decide what and where to show stuff
		/// See: http://enginecore.blogspot.ro/2013/06/more-dynamic-android-fragments-with.html
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public bool Show(MvxViewModelRequest request)
		{
			try
			{
				MvxFragment frag = null;
				var title = string.Empty;
				var section = this.ViewModel.GetSectionForViewModelType(request.ViewModelType);

			    switch (section)
			    {
			        case MenuType.FirstView:
			        {
			            if (this.SupportFragmentManager.FindFragmentById(Resource.Id.content_frame) as BrowseView != null)
			            {
			                return true;
			            }

			            frag = new BrowseView();
			            title = Resources.GetString(Resource.String.FirstView);
			        }
			            break;
			    }

			    var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
				var viewModel = loaderService.LoadViewModel(request, null /* saved state */);

				frag.ViewModel = viewModel;

				// TODO - replace this with extension method when available

				//Normally we would do this, but we already have it
				this.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag).Commit();
				//this._drawerList.SetItemChecked(this.ViewModel.MenuItems.FindIndex(m=>m.Id == (int)section), true);
				this.ActionBar.Title = this._title = title;

				this._drawer.CloseDrawer(this._drawerList);

				return true;
			}
			finally
			{
				this._drawer.CloseDrawer(this._drawerList); 
			}
		}

		protected override void OnPostCreate(Bundle bundle)
		{
			base.OnPostCreate(bundle);
			this._drawerToggle.SyncState();
		}


		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			this._drawerToggle.OnConfigurationChanged(newConfig);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			//MenuInflater.Inflate(Resource.Menu.main, menu);
			return base.OnCreateOptionsMenu(menu);
		}

		public override bool OnPrepareOptionsMenu(IMenu menu)
		{
			var drawerOpen = this._drawer.IsDrawerOpen(this._drawerList);
			//when open down't show anything
			for (int i = 0; i < menu.Size(); i++)
				menu.GetItem(i).SetVisible(!drawerOpen);


			return base.OnPrepareOptionsMenu(menu);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (this._drawerToggle.OnOptionsItemSelected(item))
				return true;

			return base.OnOptionsItemSelected(item);
		}

	}
}

