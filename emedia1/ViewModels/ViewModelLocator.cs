using emedia1.Views;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;

namespace emedia1.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var navi = CreateNavigation();
            SimpleIoc.Default.Register( () => navi);
            SimpleIoc.Default.Register<MainPageViewModel>(true);
            SimpleIoc.Default.Register<DetailPageViewModel>(true);
        }

        public MainPageViewModel Main => ServiceLocator.Current.GetInstance<MainPageViewModel>();
        public DetailPageViewModel Detail => ServiceLocator.Current.GetInstance<DetailPageViewModel>();

        INavigationService CreateNavigation()
        {
            var navi = new NavigationService();
            navi.Configure("MainPage", typeof(Views.MainPage));
            navi.Configure("DetailPage", typeof(DetailPage));
            return navi;
        }
    }
}
