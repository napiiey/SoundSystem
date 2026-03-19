using UnityEngine;

namespace Acfeel.SoundSystem
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;
        static bool isApplicationQuitting;
        static int destroyedFrame = -1;

        protected static T Instance
        {
            get
            {
                if (isApplicationQuitting)
                {
                    return null;
                }

                if (ReferenceEquals(instance, null))
                {
                    instance = (T)FindAnyObjectByType(typeof(T));
                }

                if (!instance)
                {
                    if (Application.isPlaying && destroyedFrame == Time.frameCount)
                    {
                        return null;
                    }

                    instance = (T)FindAnyObjectByType(typeof(T));

                    if (ReferenceEquals(instance, null))
                    {
                        var go = new GameObject(typeof(T).Name);
                        instance = go.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticStateOnSubsystemRegistration()
        {
            ResetStaticState();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetStaticStateBeforeSceneLoad()
        {
            ResetStaticState();
        }

        static void ResetStaticState()
        {
            instance = null;
            isApplicationQuitting = false;
            destroyedFrame = -1;
        }

        protected virtual void Awake()
        {
            destroyedFrame = -1;
            Application.quitting -= HandleApplicationQuitting;
            Application.quitting += HandleApplicationQuitting;

            if (ReferenceEquals(instance, null) || !instance)
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
                destroyedFrame = Application.isPlaying ? Time.frameCount : -1;
            }
        }

        static void HandleApplicationQuitting()
        {
            isApplicationQuitting = true;
        }
    }
}
