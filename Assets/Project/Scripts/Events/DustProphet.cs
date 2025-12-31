using System;
using UnityEngine;

namespace UIEvents {
    public static class DustProphet {
        // Notify subscribers that presenter and screen are ready
        public static Action SetupComplete;
        public static Action DrillingViewShown;
        
        public static Action<int> FooterButtonClicked;
        public static Action<int> FooterButtonHighlighted;
        public static Action<int> FooterButtonEntered;
    }
}
