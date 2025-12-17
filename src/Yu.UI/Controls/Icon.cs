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
        // Window Controls
        ["CheckCircleOutline"] = "✓",
        ["CloseCircleOutline"] = "×",
        ["Close"] = "×",
        ["Minus"] = "–",
        ["Maximize"] = "□",

        // File Operations
        ["Folder"] = "📁",
        ["FolderOpen"] = "📂",
        ["Save"] = "💾",
        ["Eye"] = "👁",
        ["Image"] = "🖼",
        ["OpenInBrowser"] = "⤴",
        ["DropSaver"] = "💾",
        ["ShowOutline"] = "👁",

        // Basic Operations
        ["Settings"] = "⚙",
        ["User"] = "👤",
        
        // Color & Palette
        ["Color"] = "🎨",
        ["ColorPalette"] = "🎨",
        ["ColorFill"] = "🖌",
        
        // Filters & Blur
        ["Blur"] = "◌",
        ["BlurRadial"] = "◍",
        ["Filter"] = "⊚",
        ["FilterOutline"] = "⊙",
        ["ImageFilterBlackWhite"] = "⚙",
        
        // Morphology & Shapes
        ["ShapeCirclePlus"] = "⊕",
        ["ArrowExpand"] = "⤢",
        ["ArrowCollapse"] = "⤡",
        ["Gradient"] = "▦",
        ["CircleOutline"] = "○",
        ["Circle"] = "●",
        ["ChartLine"] = "📈",
        ["HatFedora"] = "🎩",
        
        // Threshold & Contrast
        ["Contrast"] = "◐",
        ["InvertColors"] = "◑",
        ["AutoFix"] = "✨",
        
        // Edge Detection & Borders
        ["VectorLine"] = "╱",
        ["BorderOutside"] = "▢",
        ["BorderAll"] = "⊞",
        ["BorderInside"] = "⌗",
        
        // Transform
        ["Transform"] = "⟲",
        ["Resize"] = "⇔",
        ["Rotate90DegreesCcw"] = "↶",
        ["VectorSquare"] = "▱",
        ["Perspective"] = "⬓",
        
        // Advanced Operations
        ["Star"] = "★",
        ["MagicWand"] = "✨",
        
        // Histogram & Charts
        ["ChartBar"] = "📊",
        ["ChartBellCurve"] = "⌢",
        ["ChartHistogram"] = "▅",
        
        // Contours & Vectors
        ["VectorPolyline"] = "⌇",
        ["VectorCurve"] = "〰",
        ["VectorRectangle"] = "▭",
        ["ShapeOutline"] = "▢",
        
        // Search & Match
        ["ImageSearch"] = "🔍",
        ["ImageMultiple"] = "🖼",
        
        // Other
        ["ImageArea"] = "▦",
        ["Analytics"] = "📊",
        ["ContactlessPaymentCircle"] = "◎",
        ["AspectRatio"] = "▭",
    };

    public static string GetGlyph(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind)) return string.Empty;
        return Map.TryGetValue(kind.Trim(), out var glyph) ? glyph : "•";
    }
}
