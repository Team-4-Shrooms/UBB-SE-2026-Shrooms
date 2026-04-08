using System;
using System.Reflection;

namespace MovieShop.Tests
{
    public static class TestServiceHelper
    {
        public static void SetAppServices(IServiceProvider serviceProvider)
        {
            var servicesProperty = typeof(MovieShop.App).GetProperty("Services", BindingFlags.Public | BindingFlags.Static);
            if (servicesProperty != null)
            {
                servicesProperty.SetValue(null, serviceProvider);
            }
        }
    }
}
