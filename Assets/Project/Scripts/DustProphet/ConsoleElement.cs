using UnityEngine.UIElements;

[UxmlElement]
public partial class ConsoleElement : VisualElement
{
    VisualElement TargetHeader => this.Q("target-header");
    VisualElement Console => this.Q("console");

    public void Init() {
        
    }

    public ConsoleElement() {
        
    }
}
