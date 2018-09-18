using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using UIPerformance.Models;
using UIPerformance.Fragments;
using UIPerformance.Helpers;

namespace UIPerformance
{
    [Activity(Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private List<ResultModel> _results = new List<ResultModel>();
        private Android.Support.V7.Widget.Toolbar _toolbar;
        private TextView _linearCount;
        private TextView _linearTime;
        private TextView _linearMemory;
        private TextView _relativeCount;
        private TextView _relativeTime;
        private TextView _relativeMemory;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            _toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(_toolbar);

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            var toggle = new ActionBarDrawerToggle(this, drawer, _toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            _linearCount = FindViewById<TextView>(Resource.Id.linear_count);
            _linearTime = FindViewById<TextView>(Resource.Id.linear_time);
            _linearMemory = FindViewById<TextView>(Resource.Id.linear_memory);
            _relativeCount = FindViewById<TextView>(Resource.Id.relative_count);
            _relativeTime = FindViewById<TextView>(Resource.Id.relative_time);
            _relativeMemory = FindViewById<TextView>(Resource.Id.relative_memory);

            ShowFragment(new ContentFragment(Resource.Layout.linear_fragment_layout));
        }

        public override void OnBackPressed()
        {
            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
                drawer.CloseDrawer(GravityCompat.Start);
            else
                base.OnBackPressed();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            var resourceId = -1;
            var model = new ResultModel();

            switch(id)
            {
                case Resource.Id.linear_item:
                    resourceId = Resource.Layout.linear_fragment_layout;
                    model.Type = ViewType.LinearLayout;
                    _toolbar.Title = Constants.LinearLayout;
                    break;
                case Resource.Id.relative_item:
                    resourceId = Resource.Layout.relative_fragment_layout;
                    model.Type = ViewType.RelativeLayout;
                    _toolbar.Title = Constants.RelativeLayout;
                    break;
                default:
                    break;
            }

            var fragment = new ContentFragment(resourceId);

            if(resourceId != -1)
                ShowFragment(fragment);

            void handler(ContentFragment sender)
            {
                fragment.ViewCreated -= handler;
                model.ElapsedTime = sender.ElapsedTime;
                model.ElapsedMemory = sender.ElapsedMemory;
                UpdateResults(model);
            }

            fragment.ViewCreated += handler;

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        private void ShowFragment(ContentFragment fragment)
        {
            var transition = SupportFragmentManager.BeginTransaction();
            transition.Replace(Resource.Id.container, fragment);
            transition.Commit();
        }

        private void UpdateResults(ResultModel model)
        {
            _results.Add(model);
            var linearModels = _results.FindAll(item => item.Type == ViewType.LinearLayout);
            var relativeModels = _results.FindAll(item => item.Type == ViewType.RelativeLayout);
            UpdateUIResults(_linearCount, _linearTime, _linearMemory, linearModels);
            UpdateUIResults(_relativeCount, _relativeTime, _relativeMemory, relativeModels);
        }

        private void UpdateUIResults(TextView countTextView, TextView timeTextView, TextView memoryTextView, List<ResultModel> models)
        {
            countTextView.Text = models.Count.ToString();
            timeTextView.Text = models.Count > 0 ? models.Select(item => item.ElapsedTime).Average().ToString("0.0") : "0";
            memoryTextView.Text = models.Count > 0 ? models.Select(item => item.ElapsedMemory).Average().ToString("0.0") : "0";
        }
    }
}
