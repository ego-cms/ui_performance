using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;

namespace UIPerformance.Fragments
{
    public delegate void ViewCreatedEventHandler(ContentFragment fragment);
    public class ContentFragment : Fragment
    {
        private int _resource;

        public int ElapsedTime { get; private set; }
        public long ElapsedMemory { get; private set; }

        public event ViewCreatedEventHandler ViewCreated;

        public ContentFragment(int resource)
        {
            _resource = resource;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var timeStart = DateTime.UtcNow;
            var view = inflater.Inflate(_resource, container, false);
            ElapsedMemory =  GC.GetTotalMemory(true) / 1024;
            ElapsedTime = (DateTime.UtcNow - timeStart).Milliseconds;
            ViewCreated?.Invoke(this);
            return view;
        }
    }
}
