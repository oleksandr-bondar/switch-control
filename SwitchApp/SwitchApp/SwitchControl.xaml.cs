﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace SwitchApp
{
    public sealed partial class SwitchControl : UserControl
    {
        public string Text { get; set; }
        public Color BackgroundColor { get; set; }
        public Thickness KnobPadding { get; private set; } = new Thickness(5);
        public double Offset => 100 * knobTransform.X / _knobMaxX;
        public bool Checked { get; set; }

        new public double Width { get => grid.Width; set { grid.Width = value; InitLayout(); } }

        public event EventHandler StateChanged;
        public event EventHandler ValueChanged;

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private string _text;
        private bool _checked;
        private Color _backgroundColor;
        private Thickness _padding;
        private Point? _knobLastPos;

        private double _width;
        private double _height;
        private double _cornerRadius;
        private double _knobSize;
        private double _knobRadius;
        private double _knobPadding;
        private double _knobMinX;
        private double _knobMaxX;
        private Color _knobColor;

        public SwitchControl()
        {
            this.InitializeComponent();
            InitLayout();

            knob.PointerPressed += Knob_PointerPressed;

            knob.PointerReleased += Knob_PointerReleased;
            PointerReleased += Knob_PointerReleased;
            PointerExited += Knob_PointerReleased;

            PointerMoved += Grid_PointerMoved;
            //grid.SizeChanged += Grid_PointerMoved;
            //grid.PointerEntered += Rectangle_PointerEntered;
            //grid.PointerExited += Rectangle_PointerExited;
            Loaded += SwitchControl_Loaded;
        }

        private void SwitchControl_Loaded(object sender, RoutedEventArgs e)
        {
            storyboardShine.Begin();

            RegisterPropertyChangedCallback(WidthProperty, tbChangedCallback);
            RegisterPropertyChangedCallback(HeightProperty, tbChangedCallback);
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currPos = e.GetCurrentPoint(grid).Position;
            var x = currPos.X;

            if (_knobLastPos.HasValue && _knobLastPos.Value.X != currPos.X)
            {
                var diff = _knobLastPos.Value.X - x;

                SetKnobPosition(knobTransform.X - diff);
                _knobLastPos = currPos;
            }
        }

        private void SetKnobPosition(double x)
        {
            if (x < _knobMinX)
                x = _knobMinX;
            else if (x > _knobMaxX)
                x = _knobMaxX;

            knobTransform.X = x;
        }

        private void SetKnobPositionAnimation(double value)
        {
            storyboardMove.Stop();
            storyboardMoveAnim.From = knobTransform.X;
            storyboardMoveAnim.To = (value >= 50 ? _knobMaxX : _knobMinX);
            //storyboardMoveAnim2.From = knob.Width;
            //storyboardMoveAnim2.To = knob.Width + (_width - (knob.Width + _knobPadding * 2 + knobTransform.X));
            storyboardMove.Begin();
        }

        private void Knob_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CaptureKnob(e);
        }

        private void Knob_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            FreeKnob();
        }

        private void FreeKnob()
        {
            if (_knobLastPos.HasValue)
            {
                _knobLastPos = null;

                double value = 100 * knobTransform.X / _knobMaxX;

                SetKnobPositionAnimation(value);
                //SetKnobPosition(value >= 50 ? _knobMaxX : _knobMinX);
            }
        }

        private void CaptureKnob(PointerRoutedEventArgs prea)
        {
            if (!_knobLastPos.HasValue)
            {
                var point = prea.GetCurrentPoint(grid).Position;

                _knobLastPos = point;
            }
        }

        private void InitLayout()
        {
            _width = grid.Width;
            _height = _width / 3;
            _cornerRadius = _height / 2;

            _knobSize = _height / 1.25;
            _knobRadius = _knobSize / 2;
            _knobPadding = (_height - _knobSize) / 2;
            _knobMinX = 0.0;// _knobPadding;
            _knobMaxX = _width - (_knobSize + _knobPadding * 2);
            _knobColor = knobGS2.Color;

            grid.Height = _height;
            grid.CornerRadius = new CornerRadius(_cornerRadius);

            knob.Width = knob.Height = _knobSize;
            knob.RadiusX = knob.RadiusY = _knobRadius;
            knob.Margin = KnobPadding = new Thickness(_knobPadding);

            shineRect.Width = shineRect.Height = _height;
            shineRect.Margin = new Thickness(-_height, 0.0, 0.0, 0.0);
            shineKeyFrame.Value = _width + _height;
            textBlock.FontSize = Math.Max(10, _width / 7.5);
        }

        private void tbChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == Rectangle.WidthProperty)
            {

            }
            else if (dp == Rectangle.HeightProperty)
            {

            }
            else if (dp == Rectangle.RadiusXProperty)
            {

            }
            else if (dp == Rectangle.RadiusYProperty)
            {

            }
        }

        private void knob_Loaded(object sender, RoutedEventArgs e)
        {
            //storyboardTest.Begin();storyboardMove
        }

        #region Filling/Unfilling knob animation

        private void knob_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Point knobPnt = e.GetCurrentPoint(knob).Position;
            double knobOffsetX = knobPnt.X / knob.Width;
            double knobOffsetY = knobPnt.Y / knob.Height;

            Point startPnt = new Point(knobOffsetX, knobOffsetY);
            knobGradient.StartPoint = startPnt;

            Point leftTop = new Point(0, 0);
            Point leftRight = new Point(1, 0);
            Point bottomLeft = new Point(0, 1);
            Point bottomRight = new Point(1, 1);

            List<Point> pnts = new List<Point>()
            {
                leftTop, leftRight,
                bottomLeft, bottomRight,
                //new Point(0.5, 0), new Point(0.5, 1),
                //new Point(0, 0.5), new Point(1, 0.5)
            };

            Point endPnt;
            double maxDistance = 0;

            foreach (var pnt in pnts)
            {
                double dx = startPnt.X - pnt.X;
                double dy = startPnt.Y - pnt.Y;
                double distance = dx * dx + dy * dy;

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    endPnt = pnt;
                }
            }

            knobGradient.EndPoint = endPnt;

            Color rectColor = (grid.Background as SolidColorBrush).Color;
            Color knobColor = _knobColor;

            Color newColor = Color.FromArgb(255,
                (byte)((rectColor.R + knobColor.R) / 2),
                (byte)((rectColor.G + knobColor.G) / 2),
                (byte)((rectColor.B + knobColor.B) / 2));

            knobGS1.Color = newColor;
            knobGS2.Color = _knobColor;

            storyboardFillingColorAnim.From = _knobColor;
            storyboardFillingColorAnim.To = newColor;

            storyboardUnfilling.Stop();
            storyboardFilling.Begin();
        }

        private void knob_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            storyboardUnfillingAnim.From = knobGS1.Color;
            storyboardUnfillingAnim.To = _knobColor;

            storyboardUnfillingAnim2.From = knobGS2.Color;
            storyboardUnfillingAnim2.To = _knobColor;

            storyboardFilling.Stop();

            knobGS1.Offset = 0.0;
            knobGS2.Offset = 1.0;

            storyboardUnfilling.Begin();
        }
        
        #endregion

        //private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    //storyboardShine.Stop();
        //    storyboardShine.Begin();
        //}

        //private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        //{
        //    //storyboardShine.SkipToFill();
        //    storyboardShine.Stop();
        //}
    }
}
