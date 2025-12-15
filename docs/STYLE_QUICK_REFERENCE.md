# UI 样式快速参考

## 🎨 按钮样式

| 样式键 | 用途 | 特点 |
|--------|------|------|
| `OpenCVLabButtonBase` | 基础按钮 | 圆角12px，悬停缩放1.02 |
| `OpenCVLabDialogPrimaryButton` | 主要操作 | 渐变背景，发光阴影 |
| `OpenCVLabDialogSecondaryButton` | 次要操作 | 透明背景，边框 |
| `OpenCVLabTitleBarButton` | 标题栏按钮 | 46×32px，圆角8px |
| `IconButton` | 图标按钮 | 40×40px，无边框 |
| `FloatingActionButton` | 浮动操作 | 56×56圆形，强阴影 |

## 📦 容器样式

| 样式键 | 用途 | 特点 |
|--------|------|------|
| `BorderRegion` | 卡片容器 | 圆角20px，悬停微缩放 |
| `ResponsiveCard` | 响应式卡片 | 自动间距和阴影 |
| `OpenCVLabDialogRootBorder` | 对话框根 | 带入场动画 |
| `OpenCVLabDialogFooterBorder` | 对话框底部 | 背景色区分 |

## 🔤 文本样式

| 样式键 | 字号 | 用途 |
|--------|------|------|
| `SectionHeader` | 20px | 区域标题 |
| `SectionSubheader` | 16px | 子标题 |
| `BodyText` | 14px | 正文 |
| `CaptionText` | 12px | 说明文字 |

## 🎛️ 控件样式

| 样式键 | 控件类型 | 特点 |
|--------|----------|------|
| `OpenCVLabListBox` | ListBox | 项目滑入动画 |
| `OpenCVLabTextBox` | TextBox | 焦点发光，占位符 |
| `OpenCVLabSlider` | Slider | 渐变填充，圆形滑块 |
| `OpenCVLabCheckBox` | CheckBox | 勾选动画 |
| `ToggleButtonSwitch` | ToggleButton | 开关样式 |

## 🌈 颜色键

### 主要
- `OpenCVLab.Brush.Accent` - 品牌蓝 #0984E3
- `OpenCVLab.Brush.Surface` - 白色表面
- `OpenCVLab.Brush.Background` - 浅灰背景 #F5F7FA

### 渐变
- `OpenCVLab.Brush.Gradient.Primary` - 蓝色渐变
- `OpenCVLab.Brush.Gradient.Accent` - 紫色渐变
- `OpenCVLab.Brush.Gradient.Ocean` - 海洋渐变

### 语义
- `OpenCVLab.Brush.Success` - 成功绿 #10B981
- `OpenCVLab.Brush.Warning` - 警告橙 #F59E0B
- `OpenCVLab.Brush.Error` - 错误红 #EF4444

## 💫 阴影

- `EffectShadow1` - 最轻
- `EffectShadow2` - 轻微（卡片）
- `EffectShadow3` - 中等（对话框）
- `EffectShadow4` - 明显
- `EffectShadowAccent` - 彩色（强调）

## 📏 间距

```
XSmall   = 4px
Small    = 8px
Medium   = 16px  ← 最常用
Large    = 24px
XLarge   = 32px
XXLarge  = 48px
```

## 🔄 动画

```xml
<!-- 淡入 -->
<EventTrigger RoutedEvent="Loaded">
    <BeginStoryboard Storyboard="{StaticResource FadeInAnimation}"/>
</EventTrigger>

<!-- 从右侧滑入 -->
<BeginStoryboard Storyboard="{StaticResource SlideInFromRightAnimation}"/>

<!-- 缩放弹出 -->
<BeginStoryboard Storyboard="{StaticResource ScalePopAnimation}"/>
```

## 💡 常用组合

### 标准对话框布局
```xml
<Border Style="{StaticResource OpenCVLabDialogRootBorder}">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>  <!-- 标题 -->
            <RowDefinition Height="*"/>     <!-- 内容 -->
            <RowDefinition Height="Auto"/>  <!-- 按钮 -->
        </Grid.RowDefinitions>
        
        <ui:Title MainTitle="标题" SubTitle="Subtitle"/>
        
        <StackPanel Grid.Row="1" 
                    Margin="{StaticResource Spacing.Large}">
            <!-- 内容 -->
        </StackPanel>
        
        <Border Grid.Row="2" 
                Style="{StaticResource OpenCVLabDialogFooterBorder}">
            <StackPanel Orientation="Horizontal" 
                        HorizontalAlignment="Right" 
                        Spacing="{StaticResource Spacing.Small}">
                <Button Style="{StaticResource OpenCVLabDialogSecondaryButton}" 
                        Content="取消"/>
                <Button Style="{StaticResource OpenCVLabDialogPrimaryButton}" 
                        Content="确定"/>
            </StackPanel>
        </Border>
    </Grid>
</Border>
```

### 表单字段
```xml
<StackPanel Spacing="{StaticResource Spacing.Small}">
    <TextBlock Style="{StaticResource SectionSubheader}" 
               Text="字段名称"/>
    <TextBox Style="{StaticResource OpenCVLabTextBox}" 
             Tag="提示文本..."/>
    <TextBlock Style="{StaticResource CaptionText}" 
               Text="帮助说明"/>
</StackPanel>
```

### 卡片内容
```xml
<Border Style="{StaticResource ResponsiveCard}">
    <StackPanel>
        <TextBlock Style="{StaticResource SectionHeader}" 
                   Text="卡片标题"/>
        <Border Style="{StaticResource Divider}"/>
        <TextBlock Style="{StaticResource BodyText}" 
                   Text="卡片内容..."/>
    </StackPanel>
</Border>
```
