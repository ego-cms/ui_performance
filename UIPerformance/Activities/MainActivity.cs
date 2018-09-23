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
using System.Timers;
using Android.Graphics;

namespace UIPerformance
{
    [Activity(Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        private Timer _timer;
        private int _contIterations;
        private LayoutType _currentType = LayoutType.RelativeLayout;
        private List<ResultModel> _relativeResults = new List<ResultModel>();
        private List<ResultModel> _linearResults = new List<ResultModel>();
        private Android.Support.V7.Widget.Toolbar _toolbar;
        private TextView _linearCount;
        private TextView _linearTime;
        private TextView _linearSumTime;
        private TextView _linearMemory;
        private TextView _relativeCount;
        private TextView _relativeTime;
        private TextView _relativeSumTime;
        private TextView _relativeMemory;
        private TextView _label;
        private Button _startButton;

        private int _relativeResourceId = Resource.Layout.empty_relative_layout;
        private int _linearResourceId = Resource.Layout.empty_linear_layout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;

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
            _linearSumTime = FindViewById<TextView>(Resource.Id.linear_sumtime);
            _linearMemory = FindViewById<TextView>(Resource.Id.linear_memory);
            _relativeCount = FindViewById<TextView>(Resource.Id.relative_count);
            _relativeTime = FindViewById<TextView>(Resource.Id.relative_time);
            _relativeSumTime = FindViewById<TextView>(Resource.Id.relative_sumtime);
            _relativeMemory = FindViewById<TextView>(Resource.Id.relative_memory);
            _label = FindViewById<TextView>(Resource.Id.label);

            _startButton = FindViewById<Button>(Resource.Id.startButton);

            _timer = new Timer(500)
            {
                AutoReset = true
            };

            _timer.Elapsed += delegate
            {
                if(_contIterations == 20)
                {
                    RunOnUiThread(() => 
                    {
                        _startButton.SetTextColor(Color.Black);
                        _startButton.Clickable = true;
                        _startButton.Text = "start";
                        _contIterations = 0;
                        _timer.Stop();
                    });
                }
                else
                {
                    _currentType = _currentType == LayoutType.LinearLayout ? LayoutType.RelativeLayout : LayoutType.LinearLayout;
                    HandleChangeLayout(_currentType);

                    _contIterations++;
                }
            };

            _startButton.Click += delegate
            {
                ClearResults();
                _startButton.SetTextColor(Color.Gray);
                _startButton.Clickable = false;
                _startButton.Text = "processing...";
                _timer.Start();
            };

            ShowFragment(new ContentFragment(Resource.Layout.empty_layout));
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
            switch(item.ItemId)
            {
                case Resource.Id.zeroDimentions:
                    _relativeResourceId = Resource.Layout.empty_relative_layout;
                    _linearResourceId = Resource.Layout.empty_linear_layout;
                    break;
                case Resource.Id.fiveDimentions:
                    _relativeResourceId = Resource.Layout.relative_fragment_layout5;
                    _linearResourceId = Resource.Layout.linear_fragment_layout5;
                    break;
                case Resource.Id.tenDimentions:
                    _relativeResourceId = Resource.Layout.relative_fragment_layout10;
                    _linearResourceId = Resource.Layout.linear_fragment_layout10;
                    break;
                case Resource.Id.fiftingDimentions:
                    _relativeResourceId = Resource.Layout.relative_fragment_layout15;
                    _linearResourceId = Resource.Layout.linear_fragment_layout15;
                    break;
                case Resource.Id.twentyDimentions:
                    _relativeResourceId = Resource.Layout.relative_fragment_layout20;
                    _linearResourceId = Resource.Layout.linear_fragment_layout20;
                    break;
            }

            ShowFragment(new ContentFragment(Resource.Layout.empty_layout));

            ClearResults();

            var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        private void ClearResults()
        {
            RunOnUiThread(() =>
            {
                _relativeResults.Clear();
                _linearResults.Clear();
                _linearCount.Text = "0";
                _linearTime.Text = "0";
                _linearSumTime.Text = "0";
                _linearMemory.Text = "0";
                _relativeCount.Text = "0";
                _relativeTime.Text = "0";
                _relativeSumTime.Text = "0";
                _relativeMemory.Text = "0";
            });
        }

        private void HandleChangeLayout(LayoutType type)
        {
            var id = -1;
            var model = new ResultModel();
            
            switch(type)
            {
                case LayoutType.LinearLayout:
                    id = _linearResourceId;
                    model.Type = ViewType.LinearLayout;
                    RunOnUiThread(() => _label.Text = Constants.LinearLayout);
                    break;
                case LayoutType.RelativeLayout:
                    id = _relativeResourceId;
                    model.Type = ViewType.RelativeLayout;
                    RunOnUiThread(() => _label.Text = Constants.RelativeLayout);
                    break;
            }

            var fragment = new ContentFragment(id);

            void handler(ContentFragment sender)
            {
                fragment.ViewCreated -= handler;
                model.ElapsedTime = sender.ElapsedTime;
                model.ElapsedMemory = sender.ElapsedMemory;
                UpdateResults(model);
            }

            fragment.ViewCreated += handler;

            ShowFragment(fragment);
        }

        private void ShowFragment(ContentFragment fragment)
        {
            var transition = SupportFragmentManager.BeginTransaction();
            transition.Replace(Resource.Id.container, fragment);
            transition.Commit();
        }

        private void UpdateResults(ResultModel model)
        {
            if(model.Type == ViewType.LinearLayout)
            {
                _linearResults.Add(model);
                UpdateUIResults(_linearCount, _linearTime, _linearSumTime, _linearMemory, _linearResults);
            }
            else
            {
                _relativeResults.Add(model);
                UpdateUIResults(_relativeCount, _relativeTime, _relativeSumTime, _relativeMemory, _relativeResults);
            }
        }

        private void UpdateUIResults(TextView countTextView, TextView timeTextView, TextView sumtimeTextView, TextView memoryTextView, List<ResultModel> models)
        {
            RunOnUiThread(() =>
            {
                countTextView.Text = models.Count.ToString();
                timeTextView.Text = models.Count > 0 ? models.Select(item => item.ElapsedTime).Average().ToString("0.0") : "0";
                sumtimeTextView.Text = models.Count > 0 ? models.Select(item => item.ElapsedTime).Sum().ToString("0.0") : "0";
                memoryTextView.Text = models.Count > 0 ? models.Select(item => item.ElapsedMemory).Average().ToString("0.000") : "0";
            });
        }
    }
}
