using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Yu.UI.Controls;

public sealed class Icon : TextBlock
{
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(string), typeof(Icon), new PropertyMetadata(string.Empty, OnKindChanged));

    public string Kind
    {
        get => (string)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public Icon()
    {
        VerticalAlignment = VerticalAlignment.Center;
        HorizontalAlignment = HorizontalAlignment.Center;
        FontSize = 16;
        Foreground = Brushes.Black;
        Text = string.Empty;
    }

    private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Icon icon) return;
        icon.Text = IconGlyphs.GetGlyph(icon.Kind);
    }
}

internal static class IconGlyphs
{
    // Minimal, dependency-free glyph mapping.
    // Uses common Unicode symbols; can be swapped later to a dedicated icon font.
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CheckCircleOutline"] = "✓",
        ["CloseCircleOutline"] = "×",
        ["Close"] = "×",
        ["Minus"] = "–",
        ["Maximize"] = "□",

        ["Image"] = "🖼",
        ["OpenInBrowser"] = "⤴",
        ["DropSaver"] = "💾",
        ["ShowOutline"] = "👁",
        ["User"] = "👤",
        ["Color"] = "🎨",
        ["ImageFilterBlackWhite"] = "⚙",
        ["Blur"] = "◌",
        ["ImageArea"] = "▦",
        ["Analytics"] = "📊",
        ["ContactlessPaymentCircle"] = "◎",

        ["AspectRatio"] = "▭",
        ["BorderInside"] = "⌗",
    };

    public static string GetGlyph(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind)) return string.Empty;
        return Map.TryGetValue(kind.Trim(), out var glyph) ? glyph : "•";
    }
}
