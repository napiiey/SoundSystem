using UnityEngine;

namespace Acfeel.SoundSystem
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;
        protected static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (T)FindAnyObjectByType(typeof(T));

                    if (instance == null)
                    {
                        var go = new GameObject(typeof(T).Name);
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }

            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}