using UnityEngine;
namespace PixelEngine{
    public static class SingletonBehaviourHelper<T> where T : MonoBehaviour
    { 
        static bool isShuttingDown;
        static readonly object lockObject = new object();
        static T instance; 

        public static T Instance
        {
            get
            {
                if (isShuttingDown)
                    return null;

                lock (lockObject)
                {
                    if (instance)
                        return instance;

                    instance = (T)Object.FindFirstObjectByType(typeof(T));
                    return instance;
                }
            }
        }

        public static void NotifyApplicationQuit()
        {
            isShuttingDown = true;
        }

        public static void Reset()
        {
            lock (lockObject)
            {
                instance = null;
                isShuttingDown = false;
            }
        }
    }

}
