using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Yu.UI.Controls;

public partial class NumericBox : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericBox), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(NumericBox), new PropertyMetadata(int.MinValue, OnLimitChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(NumericBox), new PropertyMetadata(int.MaxValue, OnLimitChanged));

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Coerce(value));
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public NumericBox()
    {
        InitializeComponent();
        Loaded += (_, _) => ClampValue();
        PART_Text.LostFocus += (_, _) => ClampValue();
        PART_Text.PreviewTextInput += (_, e) =>
        {
            // allow digits and leading minus
            foreach (var ch in e.Text)
            {
                if (!char.IsDigit(ch) && ch != '-')
                {
                    e.Handled = true;
                    return;
                }
            }
        };
        PART_Text.TextChanged += (_, _) =>
        {
            if (PART_Text.IsFocused)
            {
                if (int.TryParse(PART_Text.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                {
                    Value = v;
                }
            }
        };
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumericBox box) return;
        var newValue = box.Coerce((int)e.NewValue);
        if (newValue != (int)e.NewValue)
        {
            box.SetCurrentValue(ValueProperty, newValue);
            return;
        }

        if (box.PART_Text != null && !box.PART_Text.IsFocused)
        {
            box.PART_Text.Text = newValue.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static void OnLimitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericBox box) box.ClampValue();
    }

    private void Up_Click(object sender, RoutedEventArgs e)
    {
        Value = Coerce(Value + 1);
        PART_Text.Text = Value.ToString(CultureInfo.InvariantCulture);
    }

    private void Down_Click(object sender, RoutedEventArgs e)
    {
        Value = Coerce(Value - 1);
        PART_Text.Text = Value.ToString(CultureInfo.InvariantCulture);
    }

    private void ClampValue() => Value = Coerce(Value);

    private int Coerce(int value)
    {
        if (value < Minimum) return Minimum;
        if (value > Maximum) return Maximum;
        return value;
    }
}
