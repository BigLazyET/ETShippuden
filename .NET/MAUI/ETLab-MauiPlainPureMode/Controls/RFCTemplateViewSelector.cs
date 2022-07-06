namespace ETLab_MauiPlainPureMode.Controls
{
    public class RFCTemplateViewSelector : DataTemplateSelector
    {
        public ControlTemplate RFC3489TemplateView { get; set; }

        public ControlTemplate RFC5780TemplateView { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            // 返回只能是DataTemplate，所以Controlemplate不适用
            return null;
        }
    }
}
