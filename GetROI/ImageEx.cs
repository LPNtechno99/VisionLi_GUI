using CLR_LPN;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GUI_WPF
{
    public class ImageEx : Image
    {
        private bool _drag;
        private bool _successGetROI;
        private bool _getROI;
        private bool _choosingROI;
        private Size _dragSize;
        private Point _dragStart;
        private Point _dragStartOffset;
        private Rect _dragRect;
        private AnchorPoint _dragAnchor = AnchorPoint.None;
        private Rect _rect;
        private Rect _rectTransform;
        private Point _centerPoint;
        private Point _centerPointReal;
        private Point _offsetRect;
        private Single _rectRotation;
        private ScrollViewerEx _parent;

        //private Point origin;
        //private Point start;

        public delegate void delegateGetROI(System.Drawing.Bitmap bmpROI, bool successGetROI);
        public event delegateGetROI OnGetROI;

        HitType MouseHitType = HitType.None;
        public void SetHitType(MouseEventArgs e)
        {
            // Compute a Screen to Rectangle transform 

            var mat = new Matrix();
            mat.RotateAt(_rectRotation, _centerPoint.X, _centerPoint.Y);
            mat.Translate(_offsetRect.X, _offsetRect.Y);
            mat.Invert();

            // Mouse point in Rectangle's space. 
            var point = mat.Transform(new Point(e.GetPosition(this).X, e.GetPosition(this).Y));

            var rect = _rect;
            var rectTopLeft = new Rect(_rect.Left - 10f, _rect.Top - 10f, 20f, 20f);
            var rectTopRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top - 10f, 20f, 20f);
            var rectBottomLeft = new Rect(_rect.Left - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
            var rectBottomRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
            var rectMidTop = new Rect(_rect.Left + _rect.Width / 2 - 10f, _rect.Top - 10f, 20f, 20f);
            var rectMidBottom = new Rect(_rect.Left + _rect.Width / 2 - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
            var rectMidLeft = new Rect(_rect.Left - 10f, _rect.Top + _rect.Height / 2 - 10f, 20f, 20f);
            var rectMidRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top + _rect.Height / 2 - 10f, 20f, 20f);
            var ellipse = new EllipseGeometry(new Point(_rect.Left + _rect.Width / 2, _rect.Top - 55), 10d, 10d);
            if (rectTopLeft.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.TopLeft;
                SetMouseCusor();
            }
            else if (rectTopRight.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.TopRight;
                SetMouseCusor();
            }
            else if (rectBottomLeft.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.BottomLeft;
                SetMouseCusor();
            }
            else if (rectBottomRight.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.BottomRight;
                SetMouseCusor();
            }
            else if (rectMidTop.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.MidTop;
                SetMouseCusor();
            }
            else if (rectMidBottom.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.MidBottom;
                SetMouseCusor();
            }
            else if (rectMidLeft.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.MidLeft;
                SetMouseCusor();
            }
            else if (rectMidRight.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.MidRight;
                SetMouseCusor();
            }
            else if (ellipse.FillContains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.Rotate;
                SetMouseCusor();
            }
            else if (rect.Contains(point))
            {
                _choosingROI = true;
                MouseHitType = HitType.Body;
                SetMouseCusor();
            }
            else
            {
                _choosingROI = false;
                MouseHitType = HitType.None;
                SetMouseCusor();
            }
        }
        public void SetMouseCusor()
        {
            Cursor desired_cursor = Cursors.Arrow;
            switch (MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.SizeAll;
                    break;
                case HitType.Rotate:
                    desired_cursor = Cursors.UpArrow;
                    break;
                case HitType.TopLeft:
                case HitType.BottomRight:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.TopRight:
                case HitType.BottomLeft:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.MidTop:
                case HitType.MidBottom:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.MidLeft:
                case HitType.MidRight:
                    desired_cursor = Cursors.SizeWE;
                    break;
                default:
                    break;
            }
            // Display the desired cursor.
            if (Cursor != desired_cursor) Cursor = desired_cursor;
        }
        public ImageEx()
        {
            _rect = new Rect(new Point(100, 100), new Size(220, 160));
            _rectTransform = _rect;
            _offsetRect = new Point(0, 0);
            _rectRotation = 0;
            _centerPoint = new Point(_rect.Left + _rect.Width / 2, _rect.Top + _rect.Height / 2);
            _centerPointReal = new Point(_centerPoint.X + _offsetRect.X, _centerPoint.Y + _offsetRect.Y);

            TransformGroup group = new TransformGroup();
            ScaleTransform st = new ScaleTransform();
            group.Children.Add(st);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            this.RenderTransform = group;
            this.RenderTransformOrigin = new Point(0.0, 0.0);

            this.MouseDown += ImageEx_MouseDown;
            this.MouseMove += ImageEx_MouseMove;
            this.MouseUp += ImageEx_MouseUp;
            this.MouseWheel += ImageEx_MouseWheel;

            this.MouseLeftButtonUp += ImageEx_MouseLeftButtonUp;
            this.MouseLeftButtonDown += ImageEx_MouseLeftButtonDown;
            
            this.PreviewMouseRightButtonDown += ImageEx_PreviewMouseRightButtonDown;
        }

        

        public void ResetImageEx()
        {
            _rect = new Rect(new Point(100, 100), new Size(220, 160));
            _rectTransform = _rect;
            _offsetRect = new Point(0, 0);
            _rectRotation = 0;
            _centerPoint = new Point(_rect.Left + _rect.Width / 2, _rect.Top + _rect.Height / 2);
            _centerPointReal = new Point(_centerPoint.X + _offsetRect.X, _centerPoint.Y + _offsetRect.Y);
            //if (_getROI)
            //    this.InvalidateVisual();
        }
        private void ImageEx_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_getROI)
                return;

            var mat = new Matrix();
            mat.RotateAt(_rectRotation, _centerPoint.X, _centerPoint.Y);
            mat.Translate(_offsetRect.X, _offsetRect.Y);
            mat.Invert();

            // Mouse point in Rectangle's space. 
            var point = mat.Transform(new Point(e.GetPosition(this).X, e.GetPosition(this).Y));

            if (_rect.Contains(point))
            {
                //Create contextmenu get ROI
                ContextMenu context = new ContextMenu();

                MenuItem menuItem = new MenuItem();
                menuItem.Header = "Get ROI";
                menuItem.Name = "mnuGetROI";
                menuItem.Click += MenuItem_Click;
                menuItem.FontFamily = new FontFamily("Consolas");
                menuItem.FontWeight = FontWeights.Bold;
                menuItem.FontSize = 12;

                context.Items.Add(menuItem);
                context.PlacementTarget = this;
                context.IsOpen = true;

            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Bitmap _bmpROI = BridgeViLi.ExtractROI(MainWindow._bmpSrcImage, new System.Drawing.SizeF((float)_rect.Width, (float)_rect.Height),
                        new System.Drawing.PointF((float)_centerPointReal.X, (float)_centerPointReal.Y), _rectRotation);

            if(OnGetROI!=null)
            {
                this.Dispatcher.Invoke(new Action(()=> { OnGetROI(_bmpROI, true); }));
            }
        }

        public void Reset()
        {

            // reset zoom
            var st = GetScaleTransform(this);
            st.ScaleX = 1.0;
            st.ScaleY = 1.0;

            // reset pan
            var tt = GetTranslateTransform(this);
            tt.X = 0.0;
            tt.Y = 0.0;

        }
        private void ImageEx_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void ImageEx_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            this.Cursor = Cursors.Arrow;
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }
        private void ImageEx_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //var st = GetScaleTransform(this);
            //var tt = GetTranslateTransform(this);

            //double zoom = e.Delta > 0 ? .2 : -.2;
            //if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
            //    return;

            //Point relative = e.GetPosition(this);
            //double absoluteX;
            //double absoluteY;

            //absoluteX = relative.X * st.ScaleX + tt.X;
            //absoluteY = relative.Y * st.ScaleY + tt.Y;

            //st.ScaleX += zoom;
            //st.ScaleY += zoom;

            //tt.X = absoluteX - relative.X * st.ScaleX;
            //tt.Y = absoluteY - relative.Y * st.ScaleY;
        }

        private void ImageEx_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_getROI)
                return;
            _drag = false;
        }

        private void ImageEx_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_getROI)
                return;
            SetHitType(e);
            if (!_drag)
                return;

            var mat = new Matrix();
            mat.RotateAt(_rectRotation, _centerPoint.X, _centerPoint.Y);
            mat.Translate(_offsetRect.X, _offsetRect.Y);
            mat.Invert();

            var point = mat.Transform(new Point(e.GetPosition(this).X, e.GetPosition(this).Y));

            Point offsetSize;
            Point clamped;

            switch (_dragAnchor)
            {
                case AnchorPoint.TopLeft:

                    clamped = new Point(Math.Min(_rect.BottomRight.X - 10d, point.X),
                        Math.Min(_rect.BottomRight.Y - 10d, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left + offsetSize.X,
                        _dragRect.Top + offsetSize.Y,
                        _dragRect.Width - offsetSize.X,
                        _dragRect.Height - offsetSize.Y);

                    break;

                case AnchorPoint.TopRight:
                    clamped = new Point(Math.Max(_rect.BottomLeft.X - 10d, point.X),
                        Math.Min(_rect.BottomLeft.Y - 10d, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left,
                        _dragRect.Top + offsetSize.Y,
                        _dragRect.Width + offsetSize.X,
                        _dragRect.Height - offsetSize.Y);

                    break;

                case AnchorPoint.BottomLeft:
                    clamped = new Point(Math.Min(_rect.TopRight.X - 10d, point.X),
                        Math.Max(_rect.TopRight.Y + 10d, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left + offsetSize.X,
                        _dragRect.Top,
                        _dragRect.Width - offsetSize.X,
                        _dragRect.Height + offsetSize.Y);

                    break;

                case AnchorPoint.BottomRight:
                    clamped = new Point(Math.Max(_rect.TopLeft.X + 10d, point.X),
                        Math.Max(_rect.TopLeft.Y + 10d, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left,
                        _dragRect.Top,
                        _dragRect.Width + offsetSize.X,
                        _dragRect.Height + offsetSize.Y);

                    break;

                case AnchorPoint.MidTop:
                    clamped = new Point(Math.Min(_rect.BottomRight.X - 10d, point.X),
                        Math.Min(_rect.BottomRight.Y - 10d, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left,
                        _dragRect.Top + offsetSize.Y,
                        _dragRect.Width,
                        _dragRect.Height - offsetSize.Y);
                    break;
                case AnchorPoint.MidBottom:
                    clamped = new Point(Math.Min(_rect.TopRight.X - 10d, point.X),
                        Math.Max(_rect.TopRight.Y + 10d, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left,
                        _dragRect.Top,
                        _dragRect.Width,
                        _dragRect.Height + offsetSize.Y);
                    break;
                case AnchorPoint.MidLeft:
                    clamped = new Point(Math.Min(_rect.TopRight.X - 10d, point.X),
                        Math.Max(_rect.TopRight.Y, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left + offsetSize.X,
                        _dragRect.Top,
                        _dragRect.Width - offsetSize.X,
                        _dragRect.Height);
                    break;
                case AnchorPoint.MidRight:
                    clamped = new Point(Math.Max(_rect.TopLeft.X + 10d, point.X),
                        Math.Max(_rect.TopLeft.Y, point.Y));
                    offsetSize = new Point(clamped.X - _dragStart.X, clamped.Y - _dragStart.Y);
                    _rect = new Rect(
                        _dragRect.Left,
                        _dragRect.Top,
                        _dragRect.Width + offsetSize.X,
                        _dragRect.Height);
                    break;

                case AnchorPoint.Rotation:
                    //var vecX = (point.X);
                    //var vecY = (-point.Y);

                    var vecX = (point.X - _centerPoint.X);
                    var vecY = (_centerPoint.Y - point.Y);

                    var len = Math.Sqrt(vecX * vecX + vecY * vecY);

                    var normX = vecX / len;
                    var normY = vecY / len;

                    //In rectangles's space, 
                    //compute dot product between, 
                    //Up and mouse-position vector
                    var dotProduct = (0 * normX + 1 * normY);
                    var angle = Math.Acos(dotProduct);

                    if (vecX < 0)
                        angle = -angle;

                    // Add (delta-radians) to rotation as degrees
                    _rectRotation += (float)((180 / Math.PI) * angle);
                    if (_rectRotation > 360 || _rectRotation < -360)
                        _rectRotation = 0;

                    break;

                case AnchorPoint.Center:
                    //move this in screen-space
                    _offsetRect = new Point(e.GetPosition(this).X - _dragStartOffset.X,
                        e.GetPosition(this).Y - _dragStartOffset.Y);

                    break;
                default:
                    //if (child.IsMouseCaptured)
                    //{
                    //    var tt = GetTranslateTransform(child);
                    //    Vector v = start - e.GetPosition(this);
                    //    tt.X = origin.X - v.X;
                    //    tt.Y = origin.Y - v.Y;
                    //}
                    break;
            }
            this.InvalidateVisual();
        }

        private void ImageEx_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_getROI)
                return;

            // Compute a Screen to Rectangle transform 

            var mat = new Matrix();
            mat.RotateAt(_rectRotation, _centerPoint.X, _centerPoint.Y);
            mat.Translate(_offsetRect.X, _offsetRect.Y);
            mat.Invert();

            // Mouse point in Rectangle's space. 
            var point = mat.Transform(new Point(e.GetPosition(this).X, e.GetPosition(this).Y));

            var rect = _rect;
            var rectTopLeft = new Rect(_rect.Left - 10f, _rect.Top - 10f, 20f, 20f);
            var rectTopRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top - 10f, 20f, 20f);
            var rectBottomLeft = new Rect(_rect.Left - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
            var rectBottomRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
            var rectMidTop = new Rect(_rect.Left + _rect.Width / 2 - 10f, _rect.Top - 10f, 20f, 20f);
            var rectMidBottom = new Rect(_rect.Left + _rect.Width / 2 - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
            var rectMidLeft = new Rect(_rect.Left - 10f, _rect.Top + _rect.Height / 2 - 10f, 20f, 20f);
            var rectMidRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top + _rect.Height / 2 - 10f, 20f, 20f);
            var ellipse = new EllipseGeometry(new Point(_rect.Left + _rect.Width / 2, _rect.Top - 55), 10d, 10d);

            if (!_drag)
            {
                //We're in Rectangle space now, so its simple box-point intersection test
                if (rectTopLeft.Contains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.TopLeft;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rectTopRight.Contains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.TopRight;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rectBottomLeft.Contains(point))
                {

                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.BottomLeft;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rectBottomRight.Contains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.BottomRight;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rectMidTop.Contains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.MidTop;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rectMidBottom.Contains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.MidBottom;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rectMidLeft.Contains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.MidLeft;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rectMidRight.Contains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.MidRight;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (ellipse.FillContains(point))
                {
                    _drag = true;
                    _dragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.Rotation;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _choosingROI = true;
                }
                else if (rect.Contains(point))
                {
                    _drag = true;
                    //imageEx.DragStart = new Point(point.X, point.Y);
                    _dragAnchor = AnchorPoint.Center;
                    _dragRect = new Rect(_rect.Left, _rect.Top, _rect.Width, _rect.Height);
                    _dragStartOffset = new Point(e.GetPosition(this).X - _offsetRect.X, e.GetPosition(this).Y - _offsetRect.Y);
                    _choosingROI = true;
                }
                else
                {
                    //var tt = GetTranslateTransform(this);
                    //start = e.GetPosition(this);
                    //origin = new Point(tt.X, tt.Y);
                    //this.Cursor = Cursors.Hand;
                    //this.CaptureMouse();
                    _choosingROI = false;
                }
            }
        }
        public bool ChoosingROI
        {
            get { return _choosingROI; }
            set { _choosingROI = value; }
        }
        public ScrollViewerEx Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public bool GetROI
        {
            get { return _getROI; }
            set { _getROI = value; }
        }
        public Point CenterPoint
        {
            get { return _centerPoint; }
            set { _centerPoint = value; }
        }
        public Point CenterPointReal
        {
            get { return _centerPointReal; }
            set { _centerPointReal = value; }
        }
        public bool Drag
        {
            get { return _drag; }
            set { _drag = value; }
        }
        public bool SuccessGetROI
        {
            get { return _successGetROI; }
            set { _successGetROI = value; }
        }

        public Size DragSize
        {
            get { return _dragSize; }
            set { _dragSize = value; }
        }
        public Point DragStart
        {
            get { return _dragStart; }
            set { _dragStart = value; }
        }
        public Point DragStartOffset
        {
            get { return _dragStartOffset; }
            set { _dragStartOffset = value; }
        }
        public Rect DragRect
        {
            get { return _dragRect; }
            set { _dragRect = value; }
        }
        public AnchorPoint DragAnchor
        {
            get { return _dragAnchor; }
            set { _dragAnchor = value; }
        }
        public Rect Rect
        {
            get { return _rect; }
            set { _rect = value; }
        }
        public Rect RectTransform
        {
            get { return _rectTransform; }
            set { _rectTransform = value; }
        }
        public Point OffsetRect
        {
            get { return _offsetRect; }
            set { _offsetRect = value; }
        }
        public Single RectRotation
        {
            get { return _rectRotation; }
            set { _rectRotation = value; }
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_getROI)
            {
                if (!_successGetROI)
                {
                    //calculate center point
                    _centerPoint = new Point(_rect.Left + _rect.Width / 2, _rect.Top + _rect.Height / 2);
                    _centerPointReal.X = _centerPoint.X + _offsetRect.X;
                    _centerPointReal.Y = _centerPoint.Y + _offsetRect.Y;

                    string puttext = "Angle: " + _rectRotation + "\n" + "Offset X: " + _offsetRect.X
                        + " , " + "Offset Y: " + _offsetRect.Y + "\n"
                        + "Width: " + _rect.Width
                        + " , " + "Height: " + _rect.Height + "\n"
                        + "Center Point: " + "X = " + _centerPoint.X + " , " + "Y = " + _centerPoint.Y + "\n"
                        + "Center Point Real: " + "X = " + _centerPointReal.X + " , " + "Y = " + _centerPointReal.Y;

                    FormattedText formattedText = new FormattedText(puttext, new System.Globalization.CultureInfo(2),
                    FlowDirection.LeftToRight, new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Bold, FontStretches.ExtraExpanded), 10, Brushes.DarkBlue, 0.8);
                    dc.DrawText(formattedText, new Point(10, 10));

                    var mat = new Matrix();
                    mat.RotateAt(_rectRotation, _centerPoint.X, _centerPoint.Y);
                    mat.Translate(_offsetRect.X, _offsetRect.Y);

                    MatrixTransform matrixTransform = new MatrixTransform(mat);
                    dc.PushTransform(matrixTransform);

                    dc.PushOpacity(0.55);

                    // All out gizmo rectangles are defined in Rectangle Space
                    var rectTopLeft = new Rect(_rect.Left - 10f, _rect.Top - 10f, 20f, 20f);
                    var rectTopRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top - 10f, 20f, 20f);
                    var rectBottomLeft = new Rect(_rect.Left - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
                    var rectBottomRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
                    var rectMidTop = new Rect(_rect.Left + _rect.Width / 2 - 10f, _rect.Top - 10f, 20f, 20f);
                    var rectMidBottom = new Rect(_rect.Left + _rect.Width / 2 - 10f, _rect.Top + _rect.Height - 10f, 20f, 20f);
                    var rectMidLeft = new Rect(_rect.Left - 10f, _rect.Top + _rect.Height / 2 - 10f, 20f, 20f);
                    var rectMidRight = new Rect(_rect.Left + _rect.Width - 10f, _rect.Top + _rect.Height / 2 - 10f, 20f, 20f);
                    var rectCenter = new Rect(_rect.Left + _rect.Width / 2 - 10f, _rect.Top + _rect.Height / 2 - 10f, 20f, 20f);

                    //3 point draw line and ellipse
                    Point pointLine1 = new Point(_rect.Left + _rect.Width / 2, _rect.Top - 10f);
                    Point pointLine2 = new Point(_rect.Left + _rect.Width / 2, _rect.Top - 55);
                    Point pointCenterEllipse = new Point(_rect.Left + _rect.Width / 2, _rect.Top - 55);

                    dc.DrawRectangle(Brushes.GreenYellow, new Pen(Brushes.LightYellow, 2), _rect);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectTopLeft);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectTopRight);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectBottomLeft);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectBottomRight);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectMidTop);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectMidBottom);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectMidLeft);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Blue, 1), rectMidRight);
                    dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.Black, 1), rectCenter);

                    //draw center
                    dc.DrawLine(new Pen(Brushes.Red, 1), new Point(_centerPoint.X - 15d,
                        _centerPoint.Y), new Point(_centerPoint.X + 15d, _centerPoint.Y));
                    dc.DrawLine(new Pen(Brushes.Red, 1), new Point(_centerPoint.X,
                        _centerPoint.Y - 15d), new Point(_centerPoint.X, _centerPoint.Y + 15d));
                    dc.DrawEllipse(Brushes.Red, null, _centerPoint, 2d, 2d);

                    //draw line rotate
                    dc.DrawLine(new Pen(Brushes.Black, 1.5), pointLine1, pointLine2);

                    //draw ellipse rotate
                    dc.DrawEllipse(Brushes.Blue, new Pen(Brushes.Black, 1.5), pointCenterEllipse, 10d, 10d);



                    dc.Pop();
                }

            }

        }
    }
}
