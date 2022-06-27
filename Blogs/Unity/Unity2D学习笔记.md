写在前面：
1.创建项目时，可以指定以2D还是3D模式启动Unity，但是也可以在打开后随时切换2D和3D模式
2.切换2D和3D模式：Edit -> Project Settings -> Editor -> Inspector面板 -> Default Behavior Mode
3.切换脚本编辑器：Edit -> Preferences -> External Tools -> External Script Editor
4.Unity默认输入管理配置：Edit -> Project Settings -> Input
5.Unity调试：MonoDevelop两种调试方式：最好用Run -> Attach to Process
6.游戏运行分辨率设置：Edit -> Project Settings -> Player -> Resolution
7.修改FixUpdate的时间间隔：Eidt -> Project Settings -> Time -> Fixed Timestep

Project面板中项目结构参考：
1.Prefabs：预制体，可重用的游戏对象
2.Scenes：场景
3.Scripts：代码
4.Sounds：音频
5.Textures：纹理是游戏中的精灵和图像，当然2D中可以命名为Sprites
6.Resources：非常有用且独特的文件夹，允许在脚本中通过使用静态Resources类加载一个对象或者文件 - 可以用于菜单

Hierarchy面板中对象结构参考：
1.Scripts：记住Hierarchy面板中都是对象，所以这里的Scripts是一个对象，只不过附加在上面的是全局脚本
2.Render：放置摄像机及光线对象
3.Level：里面可以添加三个空的子对象：Background，Middleground，Foreground

基础：
2D项目的分类：
1.全2D：根本没有三维几何图像，以平面图像形式吸引到屏幕上，而游戏的相机没有视角，所以要以2D模式启动编辑器
2.带3D图形的2D游戏：环境和角色用3D几何，但是玩法限制在两个维度(x.y)，但是由于游戏本身仍然使用3D模型作为障碍物，并使用相机的3D透视图。所以游戏虽然是2D的，但是在操作3D模型构建游戏时，应该启用3D模式启动编辑器
3.2D游戏和图形，带有透视摄像头：使用2D图形，但是使用透视相机获得视差滚动效果。所有图形都是平面的，但是排列在与相机不同的距离处，在这种情况下，相机投影模式更改为透视，场景视图模式更改为3D，但是以2D模式启动编辑器

相机投影模式：
层级面板选择相机，然后在相机的Inspector面板中选择Projection，调节相机的投影模式：(Orthographic-正交模式，Perspective-透视模式)
场景视图模式：Scene面板中右上角gizmo标记，点击下面的文字在Persp和Iso之间来回切换


2D模式：
1.导入的任何图像都假定为2D图像(sprites)，并设置为Sprite模式
2.Sprite Packer已启用
3.场景视图（Scene面板）设置为2D
4.默认的游戏对象没有实时的方向灯
5.相机模式位置 0，0 ，-10
6.相机设置为正交模式（3D模式下是透视-近大远小）
7.其他天空盒不能使用等等
8.2D模式最明显的特征时场景视图工具栏中2D视图模式。当启用2D模式时，将设置为正字视图；相机沿着z轴看，Y轴上增加。
9.2D图形对象称为Sprites，精灵本质上只是标准纹理。在开发过程中有特殊的技术来组合和管理纹理。
10.Unity提供了一个内置的Sprite编辑器，可以从较大的图像中提取精灵图形。这可以让你在图像编辑器中编辑单个纹理中的多个组件图像（例如可以使用它来将角色的胳膊，腿和身体作为单独的元素保存在一幅图像中）
11.2D中使用Sprite渲染器组件渲染精灵，；3D中使用网格渲染器渲染纹理
12.Component -> Rendering -> Sprite Renderer，添加到GameObject中；直接使用已经附加的Sprite Renderer创建一个GameObject:GameObject->2D Object -> Sprite
13.可以使用Sprite Creator工具制作占位符2D图像
14.Unity像3D那样已经帮我们预制了一些精灵，比如square,diamond等等，在项目面板右击->Create -> Sprites -> 可以选择你需要的精灵
15.改变场景视图中的精灵，点击场景中的精灵 -> 在Inspector面板里找到Sprite Renderering中的Sprite选项，点击右边的小圆点可以更换精灵

2D物理：
Unity有一套单独的物理引擎来处理2D物理。
1.2D物理组件对应着3D物理组件，所以明明也都差不多，只不过以2D结尾：Rigidbody 2D，Box Collider 2D和Hinge Joint 2D


精灵：
一、Sprite编辑器：
有时候精灵纹理只包含一个图形元素，但是将多个相关图形组合成单个图像通常更方便。就像车轮独立于车身一样。Sprite编辑器的目的就是很轻松的从合成图像中提取元素
0.导入精灵：直接将图像拖入Assets文件夹中，或者Assets -> Import new Asset -> 选择图像
1.注意确保要编辑的图形的纹理类型设置为Sprite(2D and UI)
2.注意图像有多个元素时，请在纹理导入Inspector，将Sprite模式设置为多个(这个的意思就是你导入的图像中本身就已经包含多个元素，后期可以通过切片Slice进行元素分割)
3.打开Sprite编辑器：从项目视图中选择要编辑的2D图像(注意无法直接编辑在场景视图(scene面板)中的精灵)；点击对应Texture Import Inspector中的sprite editor按钮，显示出Sprite Editor
（所选图像上的纹理类型(texture type）为sprite(2D和UI)，就只能看到Sprite Editor按钮
4,结合3，4点，如果图像中有多个元素，将sprite mode设置为multiple
5.Sprite面板的控件：缩放，图像显示类型，纹理像素化，Slice切片菜单，Trim裁剪菜单，Apply保存修改菜单，Revert放弃修改菜单
6.编辑器相关的数据设定，比如Name,Postition,Border,Pivot等等
7.重要：Slice切片菜单单击出现Slice面板，Type - 手动或i自动提取元素。
8.重要：Sprite Edite Outline，编辑精灵轮廓，轮廓是用来干嘛的呢：a.尽量去掉透明区域，是圈中我们所要的图像区域，提高性能。b.有利于后期精灵之间的碰撞，让碰撞检测更为精确
9.移动轮廓控制点，和按住移动轮廓控制线两种操作可以操作轮廓区域
10.轮廓公差：Outliine Telerance，越高越精确。设置完成后在Scene面板，模式选择Wireframe模式就能看到轮廓控制点如何圈住图像的

二、精灵
1.分类精灵：对精灵进行排序，有四种透明度排列模式：
a.Default：那么就根据相机的投影模式设置为透视还是正交，进行排序
b.Perspective：透视，基于透视图的排序，根据从相机位置到Sprite中心的		距离进行排序
c.正交，根据沿视图方向的距离
d.根据在透明度排序轴中设置的给定轴进行排序
最后当然你也亦可以使用脚本对每个摄像机的sprites进行排序		(TransparencySortMode和TransparencySortAxis)
2.Sprite Packer：精灵纹理往往在各个图形元素之间留有很多空白，虽然可以采用上面的轮廓公差以及你的耐心来提供精灵轮廓切割，但是并不是最好的方法。为了获得最佳性能，最好将几个精灵纹理的图形包装在一个称为地图集的纹理中(注意，这里说明图集也就是一个纹理，只不过这个纹理里包括各种精灵元素而已)。而Unity因此就提供了一个Sprite Packer实用程序。图集可以自动打包，一旦生成精灵对象图形将从图集中按需获取。
3.Sprite Packer模式禁用，但可以开启：
   Edit -> Project Settings -> Editor -> Sprite Packer
4.打开Sprite Packer窗口：Window -> Sprite Pcker
5.在Sprite Import（纹理导入器）-> 即点击在项目面板里的精灵出来的inspector面板里找到关于打包的几个设置：Packing Tag - 包装标签
具有相同包装标签的所有精灵将被打包在相同的地图集中。
6.Sprite Packer可自定制：https://docs.unity3d.com/Manual/SpritePacker.html
7.排序：