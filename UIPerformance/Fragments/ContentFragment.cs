using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Java.Lang;

namespace UIPerformance.Fragments
{
    public delegate void ViewCreatedEventHandler(ContentFragment fragment);
    public class ContentFragment : Fragment
    {
        private int _resource;

        public int ElapsedTime { get; private set; }
        public decimal ElapsedMemory { get; private set; }
        public decimal ElapsedJavaMemory { get; private set; }

        public event ViewCreatedEventHandler ViewCreated;

        public ContentFragment(int resource)
        {
            _resource = resource;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Runtime runtime = Runtime.GetRuntime();
            Console.WriteLine("Total - " + runtime.TotalMemory());
            Console.WriteLine("Free - " + runtime.FreeMemory());
            Console.WriteLine("Max - " + runtime.MaxMemory());
            var timeStart = DateTime.UtcNow;
            var view = inflater.Inflate(_resource, container, false);
            ElapsedMemory =  GC.GetTotalMemory(true);
            ElapsedJavaMemory = (runtime.TotalMemory() - runtime.FreeMemory());
            ElapsedTime = (DateTime.UtcNow - timeStart).Milliseconds;
            ViewCreated?.Invoke(this);
            return view;
        }
    }
}
