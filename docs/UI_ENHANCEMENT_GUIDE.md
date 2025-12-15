# OpenCVLab UI 美化指南

## 概览

本次升级为 OpenCVLab 带来了**优雅的极简主义**和**功能性设计**的完美平衡，包含：

- ✨ **柔和、清新的渐变色彩**
- 🎯 **比例恰当的留白**
- 🪶 **轻盈且沉浸式的用户体验**
- 📐 **微妙的阴影和模块化卡片布局**
- 🎨 **精致的圆角**
- 🔄 **细腻的微交互动效**
- 📱 **响应式设计**

## 新增样式文件

### 1. **OpenCVLabTheme.xaml** (增强版)
核心主题文件，包含：
- 扩展的颜色系统（新增语义颜色和多种渐变）
- 分层的阴影系统（EffectShadow1-4）
- 增强的按钮样式（带流畅动画）
- 响应式卡片容器

### 2. **EnhancedStyles.xaml** (新增)
高级控件样式：
- `OpenCVLabListBox` - 带滑入动效的列表框
- `OpenCVLabTextBox` - 带焦点效果的文本框
- `OpenCVLabSlider` - 现代滑块控件
- `OpenCVLabProgressBar` - 渐变进度条
- `OpenCVLabCheckBox` - 带勾选动画的复选框
- 预定义动画（淡入、滑入、缩放弹出）

### 3. **ResponsiveStyles.xaml** (新增)
响应式布局和组件：
- 标准化间距系统
- 字体大小系统
- 圆角半径系统
- 排版样式（标题、正文、标注）
- 特殊组件（图标按钮、浮动操作按钮、徽章、芯片）

## 颜色系统

### 基础颜色
```xml
OpenCVLab.Brush.Surface       (#FFFFFF)  - 卡片/表面
OpenCVLab.Brush.Background    (#F5F7FA)  - 页面背景
OpenCVLab.Brush.Border        (#E8ECEF)  - 边框
OpenCVLab.Brush.Text          (#1A1D1F)  - 主要文本
OpenCVLab.Brush.TextMuted     (#6B7280)  - 次要文本
OpenCVLab.Brush.TextLight     (#9CA3AF)  - 提示文本
```

### 品牌渐变
```xml
OpenCVLab.Brush.Gradient.Primary   - 蓝色渐变 (#0984E3 → #74B9FF)
OpenCVLab.Brush.Gradient.Accent    - 紫色渐变 (#667EEA → #764BA2)
OpenCVLab.Brush.Gradient.Ocean     - 海洋渐变 (#4A90E2 → #50C9E8)
```

### 语义颜色
```xml
OpenCVLab.Brush.Success   (#10B981)  - 成功状态
OpenCVLab.Brush.Warning   (#F59E0B)  - 警告状态
OpenCVLab.Brush.Error     (#EF4444)  - 错误状态
```

## 阴影系统

```xml
EffectShadow1       - 最轻微（悬停提示）
EffectShadow2       - 轻微（卡片）
EffectShadow3       - 中等（对话框）
EffectShadow4       - 明显（菜单、浮动元素）
EffectShadowAccent  - 彩色阴影（强调元素）
```

## 使用示例

### 1. 使用新按钮样式

```xml
<!-- 主要操作按钮（带渐变和发光效果）-->
<Button Style="{StaticResource OpenCVLabDialogPrimaryButton}" 
        Content="确定"/>

<!-- 次要按钮 -->
<Button Style="{StaticResource OpenCVLabDialogSecondaryButton}" 
        Content="取消"/>

<!-- 图标按钮 -->
<Button Style="{StaticResource IconButton}">
    <ui:Icon Kind="Search"/>
</Button>

<!-- 浮动操作按钮 -->
<Button Style="{StaticResource FloatingActionButton}">
    <ui:Icon Kind="Add" Foreground="White"/>
</Button>
```

### 2. 使用响应式卡片

```xml
<Border Style="{StaticResource ResponsiveCard}">
    <StackPanel>
        <TextBlock Style="{StaticResource SectionHeader}" 
                   Text="标题"/>
        <TextBlock Style="{StaticResource BodyText}" 
                   Text="内容文本..."/>
    </StackPanel>
</Border>
```

### 3. 使用增强列表框

```xml
<ListBox Style="{StaticResource OpenCVLabListBox}" 
         ItemsSource="{Binding Items}">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal" Spacing="12">
                <ui:Icon Kind="FileDocument"/>
                <TextBlock Text="{Binding Name}"/>
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>
```

### 4. 使用文本框和标签

```xml
<StackPanel Margin="{StaticResource Spacing.Medium}">
    <TextBlock Style="{StaticResource SectionSubheader}" 
               Text="输入名称"/>
    <TextBox Style="{StaticResource OpenCVLabTextBox}" 
             Tag="请输入名称..."
             Text="{Binding Name}"/>
    <TextBlock Style="{StaticResource CaptionText}" 
               Text="名称将用于识别项目"/>
</StackPanel>
```

### 5. 使用滑块和复选框

```xml
<StackPanel Spacing="{StaticResource Spacing.Medium}">
    <CheckBox Style="{StaticResource OpenCVLabCheckBox}" 
              Content="启用高级选项"/>
    
    <Slider Style="{StaticResource OpenCVLabSlider}" 
            Minimum="0" Maximum="100" Value="50"/>
</StackPanel>
```

### 6. 使用徽章和芯片

```xml
<!-- 徽章 -->
<Border Style="{StaticResource Badge}">
    <TextBlock Text="新" Foreground="White" FontSize="12"/>
</Border>

<!-- 芯片 -->
<Border Style="{StaticResource Chip}">
    <StackPanel Orientation="Horizontal" Spacing="6">
        <ui:Icon Kind="Tag" Width="14" Height="14"/>
        <TextBlock Text="标签"/>
    </StackPanel>
</Border>
```

### 7. 添加页面加载动画

```xml
<UserControl Loaded="OnLoaded">
    <UserControl.RenderTransform>
        <TranslateTransform Y="0"/>
    </UserControl.RenderTransform>
    
    <UserControl.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard Storyboard="{StaticResource FadeInAnimation}"/>
        </EventTrigger>
    </UserControl.Triggers>
    
    <!-- 页面内容 -->
</UserControl>
```

## 间距系统

使用标准化间距保持一致性：

```xml
Spacing.XSmall   (4px)   - 紧密元素
Spacing.Small    (8px)   - 相关元素
Spacing.Medium   (16px)  - 标准间距
Spacing.Large    (24px)  - 组间距
Spacing.XLarge   (32px)  - 区域间距
Spacing.XXLarge  (48px)  - 大区域间距
```

## 字体系统

```xml
FontSize.XSmall   (11px)  - 极小标注
FontSize.Small    (12px)  - 标注
FontSize.Regular  (14px)  - 正文
FontSize.Medium   (16px)  - 副标题
FontSize.Large    (20px)  - 小标题
FontSize.XLarge   (24px)  - 大标题
FontSize.Title    (28px)  - 页面标题
FontSize.Display  (36px)  - 展示文本
```

## 动画建议

所有微交互都使用：
- **持续时间**: 150-300ms（快速响应）
- **缓动函数**: CubicEase.EaseOut（自然流畅）
- **特殊效果**: BackEase（弹性动画）

## 最佳实践

1. **保持一致性**
   - 使用预定义的间距和字体大小
   - 遵循颜色系统
   - 统一圆角半径

2. **合理使用阴影**
   - 根据元素层级选择阴影深度
   - 避免过度使用

3. **动画适度**
   - 关键交互添加动画
   - 避免过度干扰用户

4. **响应式布局**
   - 使用 Grid 和 StackPanel 的响应式样式
   - 考虑不同窗口大小

5. **可访问性**
   - 保持足够的对比度
   - 提供清晰的视觉反馈

## 对话框示例完整代码

```xml
<Border Style="{StaticResource OpenCVLabDialogRootBorder}">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- 标题 -->
        <Border Background="{StaticResource OpenCVLab.Brush.Gradient.Primary}" 
                CornerRadius="20,20,0,0"
                Padding="{StaticResource Spacing.Large}">
            <TextBlock Text="设置参数" 
                       Foreground="White" 
                       FontSize="{StaticResource FontSize.Large}"
                       FontWeight="SemiBold"/>
        </Border>

        <!-- 内容 -->
        <ScrollViewer Grid.Row="1" 
                      Padding="{StaticResource Spacing.Large}">
            <StackPanel Spacing="{StaticResource Spacing.Medium}">
                <!-- 使用各种控件 -->
            </StackPanel>
        </ScrollViewer>

        <!-- 底部按钮 -->
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

## 更新日志

### 2024-12 初始发布
- ✅ 增强颜色系统和渐变
- ✅ 分层阴影效果
- ✅ 流畅的微交互动画
- ✅ 响应式设计系统
- ✅ 标准化间距和字体
- ✅ 高级控件样式
- ✅ 预定义动画
