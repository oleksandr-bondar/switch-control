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
        public Thickness KnobPadding { get; private set; } = new Thickness(5);
        private Point? knobLastPos;
        private Color knobBackColor;

        public SwitchControl()
        {
            this.InitializeComponent();

            knob.PointerPressed += Knob_PointerPressed;
            knob.PointerReleased += Knob_PointerReleased;
            PointerReleased += Knob_PointerReleased;
            PointerExited += Knob_PointerReleased;

            RegisterPropertyChangedCallback(Rectangle.WidthProperty, tbTagChangedCallback);
            RegisterPropertyChangedCallback(Rectangle.HeightProperty, tbTagChangedCallback);
            RegisterPropertyChangedCallback(Rectangle.RadiusXProperty, tbTagChangedCallback);
            RegisterPropertyChangedCallback(Rectangle.RadiusYProperty, tbTagChangedCallback);

            grid.PointerMoved += Grid_PointerMoved;
            //grid.SizeChanged += Grid_PointerMoved;
            grid.PointerEntered += Rectangle_PointerEntered;
            grid.PointerExited += Rectangle_PointerExited;
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currPos = e.GetCurrentPoint(grid).Position;

            if (knobLastPos.HasValue && knobLastPos.Value.X != currPos.X)
            {
                double diff = knobLastPos.Value.X - currPos.X;

                double knobX = currPos.X - knob.ActualWidth / 2;
                double minKnobX = KnobPadding.Left;
                double maxKnobX = grid.ActualWidth - (KnobPadding.Right + knob.ActualWidth);

                if (knobX < minKnobX)
                    knobX = minKnobX;
                else if (knobX > maxKnobX)
                    knobX = maxKnobX;

                var margin = knob.Margin;
                margin.Left = knobX;
                knob.Margin = margin;

                knobLastPos = currPos;
            }
        }

        private void Knob_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CaptureKnob(e.GetCurrentPoint(grid).Position);
        }

        private void Knob_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            FreeKnob();
        }

        private void FreeKnob()
        {
            if (knobLastPos.HasValue)
            {
                double knobX = knobLastPos.Value.X - knob.ActualWidth / 2;
                double minKnobX = KnobPadding.Left;
                double maxKnobX = grid.ActualWidth - (KnobPadding.Right + knob.ActualWidth);

                if (knobX < minKnobX)
                    knobX = minKnobX;
                else if (knobX > maxKnobX)
                    knobX = maxKnobX;

                //knob.Opacity = 1.0;
                knobLastPos = null;
                (knob.Fill as SolidColorBrush).Color = knobBackColor;

                double value = 100 * knobX / maxKnobX;

                if (value >= 50)
                    knobX = maxKnobX;
                else
                    knobX = minKnobX;

                var margin = knob.Margin;
                margin.Left = knobX;
                knob.Margin = margin;
            }
        }

        private void CaptureKnob(Point point)
        {
            if (!knobLastPos.HasValue)
            {
                //knob.Opacity = 0.5;
                var rectColor = (grid.Background as SolidColorBrush).Color;
                var knobColor = (knob.Fill as SolidColorBrush).Color;
                knobBackColor = knobColor;

                Color newColor = Color.FromArgb(255,
                    (byte)((rectColor.R + knobColor.R) / 2),
                    (byte)((rectColor.G + knobColor.G) / 2),
                    (byte)((rectColor.B + knobColor.B) / 2));

                (knob.Fill as SolidColorBrush).Color = newColor;
                knobLastPos = point;
            }
        }

        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //myStoryboard.Stop();
            myStoryboard.Begin();
        }

        private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //myStoryboard.SkipToFill();
            myStoryboard.Stop();
        }

        private void InitLayout()
        {

        }

        private void tbTagChangedCallback(DependencyObject sender, DependencyProperty dp)
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
    }
}
