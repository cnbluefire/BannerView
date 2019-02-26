using BannerView.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace BannerView
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            var list = new ObservableCollection<Uri>();

            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201802/20/20180220190934_4dUPY.jpeg"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201802/12/20180212191436_cEMAv.thumb.700_0.png"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201705/13/20170513161114_sBZQt.thumb.700_0.jpeg"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201801/10/20180110235519_hPWxd.thumb.700_0.jpeg"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201608/26/20160826143514_FsNrd.thumb.700_0.jpeg"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201705/13/20170513135021_KSBix.thumb.700_0.png"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201709/07/20170907202246_LdAJj.thumb.700_0.jpeg"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201802/06/2018020615514_Utj3A.thumb.700_0.jpeg"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201802/06/2018020615123_UEUMa.thumb.700_0.jpeg"));
            list.Add(new Uri("https://b-ssl.duitang.com/uploads/item/201802/06/2018020615123_EechF.thumb.700_0.jpeg"));

            List = new CycleCollectionProvider<Uri>(list);
        }

        CycleCollectionProvider<Uri> List { get; set; }
    }
}
