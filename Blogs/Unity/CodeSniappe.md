一、UGUI - RaycastTarget Checker - 投射法检查区域触摸或点击
https://blog.csdn.net/serenahaven/article/details/80972601
UGUI默认会打开一些组件的RaycastTarget属性。事实上，绝大部分的Ui组件，是不需要响应Raycast的。出于性能优化考虑，这些不需要响应Raycast的Ui组件，应该去掉其Raycast Target选项的勾选。
检查Ui组件Raycast Target属性的工具，在脚本放在Project下任意的Editor目录下即可

使用方法很简单，打开Tools->RaycastTarget Checker，弹出主窗口，窗口下会显示场景下所有包含Raycast Target属性的组件。可以根据需要显示或隐藏Scene视图下的边框，以及设置边框颜色；也可以根据需要隐藏已经被取消勾选的Ui组件
(里面涉及了如何自定义窗口等知识，值得看，[MenuItem("Tools/RaycastTarget Checker")])


```csharp
using UnityEditor;
using UnityEngine.UI;
using UnityEngine;

public class RaycastTargetChecker : EditorWindow
{
    private MaskableGraphic[] graphics;
    private bool hideUnchecked = false;
    private bool showBorders = true;
    private Color borderColor = Color.blue;
    private Vector2 scrollPosition = Vector2.zero;

    private static RaycastTargetChecker instance = null;

    [MenuItem ("Tools/RaycastTarget Checker")]
    private static void Open ()
    {
        instance = instance ?? EditorWindow.GetWindow<RaycastTargetChecker> ("RaycastTargets");
        instance.Show ();
    }

    void OnDestroy ()
    {
        instance = null;
    }

    void OnGUI ()
    {
        using (EditorGUILayout.HorizontalScope horizontalScope = new EditorGUILayout.HorizontalScope ()) {
            showBorders = EditorGUILayout.Toggle ("Show Gizmos", showBorders, GUILayout.Width (200));
            borderColor = EditorGUILayout.ColorField (borderColor);
        }
        hideUnchecked = EditorGUILayout.Toggle ("Hide Unchecked", hideUnchecked);

        GUILayout.Label ("-------------------------------------------------------------------------------" +
        "------------------------------------------------------------------------------------------------" +
        "------------------------------------------------------------------------------------------------" +
        "------------------------------------------------------------------------------------------------" +
        "------------------------------------------------------------------------------------------------");

        graphics = GameObject.FindObjectsOfType<MaskableGraphic> ();
        using (GUILayout.ScrollViewScope scrollViewScope = new GUILayout.ScrollViewScope (scrollPosition)) {
            scrollPosition = scrollViewScope.scrollPosition;
            for (int i = 0; i < graphics.Length; i++) {
                MaskableGraphic graphic = graphics [i];
                if (hideUnchecked == false || graphic.raycastTarget == true) {
                    DrawElement (graphic);
                } 
            }
        }
    }

    private void DrawElement (MaskableGraphic graphic)
    {
        using (EditorGUILayout.HorizontalScope horizontalScope = new EditorGUILayout.HorizontalScope ()) {
            graphic.raycastTarget = EditorGUILayout.Toggle (graphic.raycastTarget, GUILayout.Width (20));
            EditorGUILayout.ObjectField (graphic, typeof(MaskableGraphic), true);
        }
    }

    [DrawGizmo (GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawGizmos (MaskableGraphic source, GizmoType gizmoType)
    {
        if (instance != null && instance.showBorders == true && source.raycastTarget == true) {
            Vector3[] corners = new Vector3[4];
            source.rectTransform.GetWorldCorners (corners);
            Gizmos.color = instance.borderColor;
            for (int i = 0; i < 4; i++) {
                Gizmos.DrawLine (corners [i], corners [(i + 1) % 4]);
            }
            if (Selection.activeGameObject == source.gameObject) {
                Gizmos.DrawLine (corners [0], corners [2]);
                Gizmos.DrawLine (corners [1], corners [3]);
            }
        }
        SceneView.RepaintAll ();
    }
}
```