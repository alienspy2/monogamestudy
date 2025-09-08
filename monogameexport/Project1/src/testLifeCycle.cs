
using MGAlienLib;

namespace MGAlienLib
{
    public class testLifeCycle : ComponentBase
    {
        public override void Awake()
        {
            Logger.Log($"{name} Awake");
        }

        public override void Start()
        {
            Logger.Log($"{name} Start");
        }

        bool firstUpdate = true;
        public override void Update()
        {
            if (firstUpdate)
            {
                Logger.Log($"{name} Update");
                firstUpdate = false;
            }
        }

        public override void OnEnable()
        {
            Logger.Log($"{name} OnEnable");
        }

        public override void OnDisable()
        {
            Logger.Log($"{name} OnDisable");
        }
        public override void OnDestroy()
        {
            Logger.Log($"{name} OnDestroy");
        }
    }
}
