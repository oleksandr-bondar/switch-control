using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace SwitchApp
{
    public sealed partial class SwitchControl : UserControl
    {
        #region Properties

        public string Text { get => textBlock.Text; set => textBlock.Text = value; }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (value == _backgroundColor)
                    return;

                _backgroundColor = value;
                grid.Background = new SolidColorBrush(_backgroundColor);
            }
        }

        public double KnobPadding
        {
            get => _knobPadding;
            set
            {
                if (_knobPadding == value)
                    return;

                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(KnobPadding), value, "KnobPadding value must be greater than 1.");
                else if (value > _height / 3)
                    throw new ArgumentOutOfRangeException(nameof(KnobPadding), value, "KnobPadding value must be less than a one third of height of the SwitchControl.");

                _knobPadding = value;
                _knobSize = _height - _knobPadding * 2;

                _knobRadius = _knobSize / 2;
                _knobMaxX = _width - (_knobSize + _knobPadding * 2);
                _knobMinWidth = _knobSize;
                _knobMaxWidth = _width - (_knobPadding * 2);

                knob.Margin = new Thickness(_knobPadding);
                knob.Width = knob.Height = _knobSize;
                knob.RadiusX = knob.RadiusY = _knobRadius;

                if (DropShadowPanel.IsSupported)
                {
                    double shadowOffset = Math.Max(1.0, _knobSize / 5.0) + _knobPadding;
                    knobShadow.OffsetX = shadowOffset;
                    knobShadow.OffsetY = shadowOffset;
                    knobShadow.BlurRadius = Math.Max(9, _knobSize / 3.0);
                }

                SetKnobPositionByChecked();
            }
        }

        /// <summary>
        /// Get or set a control state (true or false).
        /// </summary>
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value)
                    return;

                _checked = value;
                SetKnobPositionByChecked();
            }
        }

        /// <summary>
        /// Get a knob position in control (value from 0.0 to 100.0).
        /// </summary>
        public double KnobOffset => 100 * knobTransform.X / _knobMaxX;

        public Color KnobColor
        {
            get => _knobColor;
            set
            {
                if (_knobColor == value)
                    return;

                _knobColor = value;
                knobGS2.Color = value;
            }
        }

        new public double Width
        {
            get => grid.Width;
            set
            {
                if (grid.Width == value)
                    return;

                if (value < 30)
                    throw new ArgumentOutOfRangeException(nameof(Width), value, "Width must be greater or equal than 30.");

                grid.Width = value;
                InitLayout();
            }
        }

        new public double Height { get => _height; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs, when control state changed (checked: true or false).
        /// </summary>
        public event EventHandler StateChanged;
        /// <summary>
        /// Occurs, when knob position in control changed (Offset value from 0.0 to 100.0).
        /// </summary>
        public event EventHandler ValueChanged;

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Fields

        //private string _text;
        private bool _checked;
        private Color _backgroundColor;
        //private Thickness _padding;
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

        private double _knobMinWidth;
        private double _knobMaxWidth;

        private bool _animLeft;
        private bool _animRight;


        private static readonly TimeSpan AnimDurationMoveKnob = TimeSpan.FromSeconds(0.1);
        private static readonly TimeSpan AnimDurationFreeKnob = TimeSpan.FromSeconds(0.5);

        private double CurrKnobMaxWidth => _knobMaxWidth - knobTransform.X;

        #endregion

        #region Private Methods

        private void SetKnobPositionByChecked()
        {
            if (_checked)
                knobTransform.X = _knobMaxX;
            else
                knobTransform.X = _knobMinX;
        }



        #endregion

        #region Event Handlers

        #endregion

        #region Constructors

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
            //Loaded += SwitchControl_Loaded;

            storyboardShine.Begin();
        }

        private void InitLayout()
        {
            _width = grid.Width;
            _height = _width / 3;
            _cornerRadius = _height / 2;

            _knobSize = _height / 1.25;
            _knobRadius = _knobSize / 2;
            _knobPadding = (_height - _knobSize) / 2;
            _knobMinX = 0.0;
            _knobMaxX = _width - (_knobSize + _knobPadding * 2);
            _knobColor = knobGS2.Color;

            _knobMinWidth = _knobSize;
            _knobMaxWidth = _width - (_knobPadding * 2);

            grid.Height = _height;
            grid.CornerRadius = new CornerRadius(_cornerRadius);

            knob.Width = knob.Height = _knobSize;
            knob.RadiusX = knob.RadiusY = _knobRadius;
            knob.Margin = new Thickness(_knobPadding);

            shineRect.Width = shineRect.Height = _height;
            shineRect.Margin = new Thickness(-_height, 0.0, 0.0, 0.0);
            shineKeyFrame.Value = _width + _height;
            textBlock.FontSize = Math.Max(10, _width / 7.5);

            if (DropShadowPanel.IsSupported)
            {
                double shadowOffset = Math.Max(1.0, _knobSize / 5.0);
                knobShadow.OffsetX = shadowOffset;
                knobShadow.OffsetY = shadowOffset;
                knobShadow.BlurRadius = Math.Max(9, _knobSize / 3.0);
            }

            SetKnobPositionByChecked();
        }

        #endregion

        #region Initialization



        #endregion

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currPos = e.GetCurrentPoint(grid).Position;
            var x = currPos.X;

            if (_knobLastPos.HasValue && Math.Abs(_knobLastPos.Value.X - currPos.X) >= 1)
            {
                SetKnobPosition(x);
                _knobLastPos = currPos;
            }
        }

        private void SetKnobPosition(double x)
        {
            double diff = (_knobLastPos.Value.X - x);
            Debug.WriteLine($"x: {x} | diff: {diff}");

            if (Math.Abs(diff) < 1)
                return;

            if (diff < 0)
            {
                if (_animLeft)
                {
                    knob.Width = storyboardWidthAnim.To.Value;
                    storyboardWidth.Stop();
                    _animLeft = false;
                    //return;
                }

                double newWidth = knob.Width + Math.Abs(diff);

                if (newWidth > CurrKnobMaxWidth)
                    newWidth = CurrKnobMaxWidth;
                else if (newWidth < _knobMinWidth)
                    newWidth = _knobMinWidth;

                if (knob.Width == newWidth)
                    return;

                knob.Width = newWidth;

                storyboardConstrictionAnimX.From = knobTransform.X;
                storyboardConstrictionAnimX.To = knobTransform.X + (knob.Width - _knobMinWidth);

                storyboardConstrictionAnimWidth.From = knob.Width;
                storyboardConstrictionAnimWidth.To = _knobMinWidth;

                storyboardConstriction.Begin();
                _animRight = true;
            }
            else// if (diff > 0)
            {
                if (_animRight)
                {
                    knob.Width = storyboardConstrictionAnimWidth.To.Value;
                    knobTransform.X = storyboardConstrictionAnimX.To.Value;
                    storyboardConstriction.Stop();
                    _animRight = false;
                    //return;
                }

                double newWidth = knob.Width + Math.Abs(diff);
                double newX = knobTransform.X - diff;
                double oldX = knobTransform.X;

                if (newX < _knobMinX)// || newX > _knobMaxX)
                {
                    if (knobTransform.X == _knobMinX)
                        return;

                    newX = _knobMinX;
                }

                knobTransform.X = newX;
                knob.Width = newWidth;

                storyboardWidthAnim.From = knob.Width;
                storyboardWidthAnim.To = _knobMinWidth;
                storyboardWidth.Begin();
                _animLeft = true;
            }

            OnValueChanged();
            //double value = 100 * knobTransform.X / _knobMaxX;
            //Debug.WriteLine("Value: " + value.ToString());
        }

        private void SetKnobPositionAnimation(double value)
        {
            storyboardMove.Stop();
            storyboardMoveAnim.From = knobTransform.X;
            storyboardMoveAnim.To = (value >= 50 ? _knobMaxX : _knobMinX);

            //storyboardMoveAnim2.From = knobShadowTransform.X;
            //storyboardMoveAnim2.To = (value >= 50 ? _knobMaxX : _knobMinX);
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
                //knob_PointerExited(null, null);

                //storyboardConstriction.Duration = AnimDurationFreeKnob;
                //storyboardWidth.Duration = AnimDurationFreeKnob;

                //if (Offset >= 50)
                //    SetKnobPosition(_width);
                //else
                //    SetKnobPosition(0);

                _knobLastPos = null;
                //double value = 100 * knobTransform.X / _knobMaxX;

                //SetKnobPositionAnimation(value);
                //UnfillingKnob();
                //SetKnobPosition(value >= 50 ? _knobMaxX : _knobMinX);
            }
        }

        private void CaptureKnob(PointerRoutedEventArgs e)
        {
            if (!_knobLastPos.HasValue)
            {
                //storyboardConstriction.Duration = AnimDurationMoveKnob;
                //storyboardWidth.Duration = AnimDurationMoveKnob;

                var point = e.GetCurrentPoint(grid).Position;

                _knobLastPos = point;
                //FillingKnob(e);
            }
        }

        private void knob_Loaded(object sender, RoutedEventArgs e)
        {
            if (DropShadowPanel.IsSupported)
            {
                var hostVisual = ElementCompositionPreview.GetElementVisual(knob);
                var compositor = hostVisual.Compositor;
                var spriteVisual = compositor.CreateSpriteVisual();

                // Make sure size of shadow host and shadow visual always stay in sync
                var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
                bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

                spriteVisual.StartAnimation("Size", bindSizeAnimation);
            }
        }

        #region Filling/Unfilling knob animation

        private bool _isFilling;

        private void FillingKnob(PointerRoutedEventArgs e)
        {
            Debug.WriteLine("FillingKnob");

            if (_isFilling)
                return;

            _isFilling = true;

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

        private void UnfillingKnob()
        {
            Debug.WriteLine("UnfillingKnob");

            if (!_isFilling)
                return;

            _isFilling = false;

            storyboardUnfillingAnim.From = knobGS1.Color;
            storyboardUnfillingAnim.To = _knobColor;

            storyboardUnfillingAnim2.From = knobGS2.Color;
            storyboardUnfillingAnim2.To = _knobColor;

            storyboardFilling.Stop();

            knobGS1.Offset = 0.0;
            knobGS2.Offset = 1.0;

            storyboardUnfilling.Begin();
        }

        private void knob_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FillingKnob(e);
        }

        private void knob_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            UnfillingKnob();
        }

        #endregion
    }
}
