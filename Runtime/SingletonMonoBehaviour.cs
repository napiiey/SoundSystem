using UnityEngine;

namespace Acfeel.SoundSystem
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;
        static bool isApplicationQuitting;
        static bool isDestroyingInstance;

        protected static T Instance
        {
            get
            {
                if (isApplicationQuitting || isDestroyingInstance)
                {
                    return null;
                }

                if (instance == null)
                {
                    instance = (T)FindAnyObjectByType(typeof(T));

                    if (instance == null && !isApplicationQuitting && !isDestroyingInstance)
                    {
                        var go = new GameObject(typeof(T).Name);
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticState()
        {
            instance = null;
            isApplicationQuitting = false;
            isDestroyingInstance = false;
        }

        protected virtual void Awake()
        {
            isDestroyingInstance = false;
            Application.quitting -= HandleApplicationQuitting;
            Application.quitting += HandleApplicationQuitting;

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

        protected virtual void OnDestroy()
        {
            if (instance == this as T)
            {
                isDestroyingInstance = true;
                instance = null;
            }
        }

        static void HandleApplicationQuitting()
        {
            isApplicationQuitting = true;
        }
    }
}
