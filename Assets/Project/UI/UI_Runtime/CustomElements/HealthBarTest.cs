using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
[UxmlObject]
public partial class HealthMarker {
    [UxmlAttribute]
    public float position;
    [UxmlAttribute]
    public Color color;
}

[UxmlElement]
public partial class HealthBar : VisualElement {
    [UxmlAttribute]
    public float lowValue { get; set; }
    [UxmlAttribute]
    public float highVaIue { get; set; }
    [UxmlAttribute]
    public float value { get; set; }
    [UxmlObjectReference("Markers")]
    public List<HealthMarker> markers { get; set; }    
    
    
}



