## MudBlazor

### 一、Breakpoints断点控制

参考文档：[breakpoints](https://mudblazor.com/features/breakpoints#breakpoints)

#### 1.1 Breakpoints

|Code|Range|
|---|---|
|xs|<600px|
|sm|600px><960px|
|md|960px><1280px|
|lg|1280px><1920px|
|xl|1920px><2560px|
|xx|>=2560px|

#### 1.2 用途

* 页面12点网格系统Grid + breakpoints，实现flex布局
[相关链接](https://mudblazor.com/components/grid#grid-with-breakpoints)

```
<MudGrid>
    <MudItem xs="12">
        <MudPaper Class="d-flex align-center justify-center mud-width-full py-8">xs=12</MudPaper>
    </MudItem>
    <MudItem xs="12" sm="6">
        <MudPaper Class="d-flex align-center justify-center mud-width-full py-8">xs=12 sm=6</MudPaper>
    </MudItem>
</MudGrid>
```

* MudHidden + breakpoints，实现断点触发隐藏与显示
[相关链接](https://mudblazor.com/components/hidden#how-it-works)


### 二、CSS库

对既有常用的css进行封装

#### 2.1 Display

* d-block 对标 display: block
```
<div class="d-block">d-block</div>
<div style="display: block">d-block</div>
```
* 其余还有d-inline，d-flex，d-inline-block，d-none等等同理
* **可以结合Breakpoints来使用**

|class|description|
|---|---|
|d-none|hide on all|
|d-none d-sm-flex|hide only on xs **这个非常重要**|
|d-md-none d-lg-flex|hide only on md|
|d-lg-none|hide only on lg|
|d-none d-md-flex d-lg-none|visible only md **这个非常重要**|

#### 2.2 Z-Index
* z-0 对标 z-index: 0
* z-10 对标 z-index: 10

#### 2.3 Overflow
* overflow-auto 对标 overflow: auto
* overflow-x-auto 对标 overflow-x: auto

#### 2.4 Visibility
* invisible 对标 visibility: hidden
* visible 对标 visibility: visible

#### 2.5 Position
* absolute 对标 position: absolute

### 三、Flexbox

#### 3.1 控制flex元素定位

必读链接：[flex布局中align-items 和align-content的区别](https://blog.csdn.net/sinat_27088253/article/details/51532992)

* Justify Content - 设置在父flex Container，对Container中的直接子级元素产生作用，定位相对于父Container的主轴
* Align Content - 设置在父flex Container，**只对多行起作用，作为一个整体奏效**，定位相对于父Container的交叉轴
* Align Items - 设置在父flex Container，**对每个flex元素起作用**，对Container中的直接子级元素(flex)产生作用，定位相对于父Container的交叉轴
* Align Self - 设置在自身，只负责自身相对于父Container的交叉轴的定位

#### 3.2 控制flex元素大小

必读链接：[深入理解flex布局的flex-grow、flex-shrink、flex-basis](https://zhuanlan.zhihu.com/p/39052660)

### 四、Spacing：Margin，Padding

#### 4.1 属性简写

* m - margin
* p - padding

#### 4.2 方位简写

* t - top；b - bottom；l - left；r - right
* s - left (LTR Mode) right (RTL Mode)；e - right（LTR Mode）left ( RTL Mode)
* x - left and right
* y - top and bottom
* a - left, right, top, bottom

#### 4.3 大小简写

**以1对应4px的比例**

* 1 - 4px
* n1 - -4px

#### 4.4 可以结合Breakpoints

#### 4.5 综上

|Code|Description|
|---|---|
|pa-6|padding left&right&top&bottom 6*4px|
|pa-md-6|在屏幕尺寸为medium的情况下，padding left&right&top&bottom 6*4px|
|mt-n12|margin top -12*4px|

### 四、有用的服务

#### 4.1 IResizeService

用于监听屏幕大小的变化
```
[Inject] IResizeService ResizeService { get; set; }
_subscriptionId = await ResizeService.Subscribe((size) =>
{
	width = size.Width;
	height = size.Height;
	InvokeAsync(StateHasChanged);
}, new ResizeOptions
{
	ReportRate = 50,
	NotifyOnBreakpointOnly = false,
});

public async ValueTask DisposeAsync() => await ResizeService.Unsubscribe(_subscriptionId);
```
