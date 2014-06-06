﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Data.Html;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
// “WebView 应用程序”模板在 http://go.microsoft.com/fwlink/?LinkID=391641 上有介绍
using App3.Common;
using HtmlAgilityPack;
using WincnApp.Core;

namespace WincnApp
{

    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        private CoreDispatcher simpleDispatcher;                    // 刷新UI用
        private bool isSlided = false;                              // 侧边状态
        private int page = 1;                                       // 当前页
        private ObservableCollection<ArticleItems> _articleList;    // 文件列表
        private StatusBar statusBar;                                // 状态栏

        private string _currentCategory;

        public MainPage()
        {
            CurrentCategory = "首页";
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            LoadBindingContent();
            simpleDispatcher = Window.Current.Dispatcher;
            LoadStatusBar();

        }

        /// <summary>
        /// 设置绑定元素的DataContent
        /// </summary>
        private void LoadBindingContent()
        {
            AppTitle.DataContext = this;
            MyListView.DataContext = this;
        }
        /// <summary>
        /// 加载状态栏
        /// </summary>
        private void LoadStatusBar()
        {
            statusBar = StatusBar.GetForCurrentView();
            // 显示StatusBar
            statusBar.ShowAsync();
            statusBar.BackgroundColor = Colors.DodgerBlue;
            statusBar.BackgroundOpacity = 1;
            statusBar.ProgressIndicator.Text = new ResourceLoader().GetString("AppName");
            statusBar.ProgressIndicator.ShowAsync();
            statusBar.ProgressIndicator.ProgressValue = 0;

            // 隐藏StatusBar
            // await statusBar.HideAsync();
        }

        public ObservableCollection<ArticleItems> ArticleLists
        {
            get { return _articleList; }
            set
            {
                _articleList = value;
                NotifyPropertyChanged("ArticleLists");
            }
        }

        public string CurrentCategory
        {
            get { return _currentCategory; }
            set
            {
                _currentCategory = value;
                NotifyPropertyChanged("CurrentCategory");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ArticleLists = new ObservableCollection<ArticleItems>();

            LoadHtmlContent("http://wincn.net/");

        }

        /// <summary>
        /// 加载Html数据
        /// </summary>
        /// <param name="url"></param>
        private void LoadHtmlContent(string url)
        {
            ProgressBar.Visibility = Visibility.Visible;
            new Task(() =>
            {
                HtmlHelper.CreateInstance().httpGet(url);
                while (true)
                {
                    string html = HtmlHelper.HtmlString;
                    if (!string.IsNullOrEmpty(html))
                    {
                        simpleDispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            HtmlDocument document = new HtmlDocument();
                            document.LoadHtml(html);
                            AnewHelper.IndexItemParse(ArticleLists, document);
                        });
                        break;
                    }
                    ProgressBar.Visibility = Visibility.Collapsed;
                }
            }).Start();
            page++;//继续加载下一页
        }

        /// <summary>
        /// 在离开此页时调用。
        /// </summary>
        /// <param name="e">描述如何导航此页的事件数据。</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        /// <summary>
        /// 控制侧边菜单展开或隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlideAppBarButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!isSlided)
            {
                StoryboardLeft.Begin();
            }
            else
            {
                StoryboardRight.Begin();
            }
            isSlided = !isSlided;
        }

        private void UIElement_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double tx = e.Delta.Translation.X;
            if (tx < 0)
            {
                StoryboardLeft.Begin();
                isSlided = true;
            }
            else
            {
                StoryboardRight.Begin();
                isSlided = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void MyListView_OnLayoutUpdated(object sender, object e)
        {
            double totalHeight = MyScrollViewer.ExtentHeight;
            double factHeight = MyScrollViewer.ActualHeight + MyScrollViewer.VerticalOffset;
            if (totalHeight.Equals(factHeight))
            {
                LoadHtmlContent(String.Format("http://wincn.net/page/{0}/", page));
            }
        }
    }
}
