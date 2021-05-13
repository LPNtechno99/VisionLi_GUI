//using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using CLR_LPN;
using System.IO;
using System.Windows.Media;
using System;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections;
using Point = System.Windows.Point;

namespace GUI_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Bitmap _bmpSrcImage = null;
        public static Bitmap _bmpStatic = null;
        public static Bitmap _bmpStatic01 = null;
        Dictionary<string, Bitmap> _dicImage = null;

        //private bool _IsThreshold;

        public MainWindow()
        {
            InitializeComponent();

            scrollViewerEx.ImgEx = imageEx;
            imageEx.Parent = scrollViewerEx;
            scrollViewerEx.GridChild = grid;
            //scrollViewerEx.ScrollChanged += ScrollViewerEx_ScrollChanged;

            _dicImage = new Dictionary<string, Bitmap>();
            imageEx.OnGetROI += imageEx_OnGetROI;
        }

        //private void ScrollViewerEx_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
        //    {
        //        Point? targetBefore = null;
        //        Point? targetNow = null;

        //        if (!scrollViewerEx.LastMousePositionOnTarget.HasValue)
        //        {
        //            if (scrollViewerEx.LastCenterPositionOnTarget.HasValue)
        //            {
        //                var centerOfViewport = new Point(scrollViewerEx.ViewportWidth / 2, scrollViewerEx.ViewportHeight / 2);
        //                Point centerOfTargetNow = this.TranslatePoint(centerOfViewport, scrollViewerEx.ImgEx);

        //                targetBefore = scrollViewerEx.LastCenterPositionOnTarget;
        //                targetNow = centerOfTargetNow;
        //            }
        //        }
        //        else
        //        {
        //            targetBefore = scrollViewerEx.LastMousePositionOnTarget;
        //            targetNow = Mouse.GetPosition(scrollViewerEx.ImgEx);

        //            scrollViewerEx.LastMousePositionOnTarget = null;
        //        }

        //        if (targetBefore.HasValue)
        //        {
        //            double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
        //            double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

        //            double multiplicatorX = e.ExtentWidth / scrollViewerEx.ImgEx.Width;
        //            double multiplicatorY = e.ExtentHeight / scrollViewerEx.ImgEx.Height;

        //            double newOffsetX = scrollViewerEx.HorizontalOffset - dXInTargetPixels * multiplicatorX;
        //            double newOffsetY = scrollViewerEx.VerticalOffset - dYInTargetPixels * multiplicatorY;

        //            if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
        //            {
        //                return;
        //            }

        //            scrollViewerEx.ScrollToHorizontalOffset(newOffsetX);
        //            scrollViewerEx.ScrollToVerticalOffset(newOffsetY);
        //        }
        //    }
        //}

        private void imageEx_OnGetROI(Bitmap bmpROI, bool successGetROI)
        {
            AddItemsTreeView("Image ROI", bmpROI);
            _bmpStatic = bmpROI;
            imageEx.SuccessGetROI = successGetROI;
            imageEx.Source = ImageSourceForBitmap(bmpROI);
            //imageEx.Source = BitmapToImageSource(bmpROI);
        }

        void AddItemsTreeView(string key, Bitmap bitmap)
        {
            if (_dicImage.Count == 0)
            {
                _dicImage.Add(key, bitmap);
            }
            else
            {
                if (_dicImage.ContainsKey(key))
                {
                    _dicImage.Remove(key);
                    _dicImage.Add(key, bitmap);
                }
                else
                {
                    _dicImage.Add(key, bitmap);
                }
            }
            tvChildItem.ItemsSource = _dicImage.Keys;
            tvChildItem.Items.Refresh();
        }
        //async Task Delay10ms()
        //{
        //    await Task.Delay(10);
        //}
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                _bmpSrcImage = new Bitmap(ofd.FileName);
                imageEx.Source = new BitmapImage(new Uri(ofd.FileName));
                AddItemsTreeView("Image Source", _bmpSrcImage);
                _bmpStatic = _bmpSrcImage;
                imageEx.ResetImageEx();
            }
        }

        #region Convert Bitmap to ImageSource
        /// <summary>
        /// convert Bitmap to ImageSource show on Image WPF
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        /// <summary>
        /// Convert Bitmap to ImageSource
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                ImageSource newSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(handle);
                return newSource;
            }
            catch (Exception ex)
            {
                DeleteObject(handle);
                return null;
            }
        }
        #endregion
        private void btnThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (_bmpStatic == null)
                return;
            
            if (stpUCTrackBar.Children.Count == 0)
            {
                UCTrackBar.UCThreshold uCThreshold = new UCTrackBar.UCThreshold();
                stpUCTrackBar.Children.Add(uCThreshold);
                uCThreshold.ApplyValueChanged += ApplyValueChanged;
                uCThreshold.DoneEdit += UCThreshold_DoneEdit;
            }
        }

        private void UCThreshold_DoneEdit()
        {
            AddItemsTreeView("Image Threshold", _bmpStatic01);
        }

        private void ApplyValueChanged(double value)
        {
            if (_bmpStatic == null)
                return;
            bool startProcessThreshold = false;
            if (!startProcessThreshold)
            {
                _bmpStatic01 = _bmpStatic;
                startProcessThreshold = true;
            }
            _bmpStatic01 = BridgeViLi.thresholdBinary(_bmpStatic, int.Parse(value.ToString()));
            imageEx.Source = ImageSourceForBitmap(_bmpStatic01);
            //imageEx.Source = BitmapToImageSource(_bmpStatic);

        }

        private void btnThresholdInv_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGetROI_Click(object sender, RoutedEventArgs e)
        {
            if (!imageEx.GetROI)
            {
                imageEx.GetROI = true;
                btnGetROI.Header = "Deselect ROI";
                imageEx.InvalidateVisual();
            }
            else
            {
                imageEx.GetROI = false;
                btnGetROI.Header = "Select ROI";
                imageEx.InvalidateVisual();
            }
        }

        private void tvImage_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tv = sender as TreeView;
            string key = (tv.SelectedItem).ToString();
            if (_dicImage.ContainsKey(key))
            {
                _dicImage.TryGetValue(key, out _bmpStatic);
                if (key == "Image Source")
                    imageEx.SuccessGetROI = false;
                else
                    imageEx.SuccessGetROI = true;
                imageEx.Source = ImageSourceForBitmap(_bmpStatic);
                //imageEx.Source = BitmapToImageSource(_bmpView);
                imageEx.Reset();
            }
        }
        //private void mniGetROI_Click(object sender, RoutedEventArgs e)
        //{
        //    System.Drawing.Bitmap _bmpROI = BridgeViLi.ExtractROI(_bmpImage, new System.Drawing.SizeF((float)imageEx.Rect.Width, (float)imageEx.Rect.Height),
        //               new System.Drawing.PointF((float)imageEx.CenterPointReal.X, (float)imageEx.CenterPointReal.Y), imageEx.RectRotation);

        //    imageEx_OnGetROI(_bmpROI, true);
        //}
    }
}
