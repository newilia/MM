using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{
    public class UniversalTrigger : Trigger
    {

        public class UniversalTriggerSaveData : TriggerSaveData
        {

            public int currentState { get; private set; }
            public int switchCount { get; private set; }

            public UniversalTriggerSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, bool done, bool reloading, int currentState, int switchCount) : base(id, pos, rot, active, enabled, done, reloading)
            {
                this.currentState = currentState;
                this.switchCount = switchCount;
            }

        }

        public enum SensorModes { Simple, Interactive };
        public enum ConditionModes { None, Item };
        public enum ActuationModes { Basic, Switch };
        public enum SwitchModes { Loop, PingPong, Once };

        [System.Serializable]
        public class Actuator
        {
            [SerializeField]
            private float delay = 0;
            public float Delay => delay;
            [SerializeField]
            private Trigger[] triggers = new Trigger[0];
            public Trigger[] Triggers => triggers;
            [SerializeField, Space]
            private UnityEvent onTrigger = default;
            public UnityEvent OnTrigger => onTrigger;
        }

        [SerializeField, Tooltip("Simple collision or interactive player input?")]
        private SensorModes sensorMode = SensorModes.Simple;
        [SerializeField]
        private GameObject handIK = null;

        [SerializeField, Tooltip("Condition to fire the trigger")]
        private ConditionModes conditionMode = ConditionModes.None;
        [SerializeField, Tooltip("Target item in the player inventory.")]
        private string item = "";
        [SerializeField]
        private int requiredItemCount = 1;

        [SerializeField, Tooltip("Basic actuation or actuate different states?")]
        private ActuationModes actuationMode = ActuationModes.Basic;
        public ActuationModes ActuationMode => actuationMode;
        [SerializeField]
        private SwitchModes switchMode = SwitchModes.Loop;
        [SerializeField]
        private int initState = 0;
        [SerializeField]
        private Actuator basicActuator = new Actuator();
        public Actuator BasicActuator => basicActuator;
        [SerializeField]
        private Actuator[] switchStates = new Actuator[2];
        public Actuator[] SwitchStates => switchStates;

        private int currentState;
        public int CurrentState
        {
            get => currentState;
            set
            {
                currentState = value;

                if (currentState >= switchStates.Length)
                {
                    switch (switchMode)
                    {
                        case SwitchModes.Loop:
                            currentState = 0;
                            break;

                        case SwitchModes.PingPong:
                            switchCount *= -1;
                            currentState = switchStates.Length - 2;
                            break;

                        case SwitchModes.Once:
                            done = true;
                            currentState = switchStates.Length - 1;
                            break;
                    }
                }
                else if (currentState < 0 && switchMode == SwitchModes.PingPong)
                {
                    currentState = 1;
                    switchCount *= -1;
                }
            }
        }

        public bool actuating { get; private set; } = false;
        private int switchCount = 1;

        private void Awake()
        {
            if (handIK)
                handIK.SetActive(false);

            currentState = initState;
        }

        private void OnValidate()
        {
            if (initState >= switchStates.Length)
                initState = switchStates.Length - 1;
            else if (initState < 0)
                initState = 0;
        }

        protected override void OnTriggerEnter(Collider other)
        {
            if (sensorMode == SensorModes.Simple)
            {
                base.OnTriggerEnter(other);
            }
            else
            {
                if (tags.Contains(other.tag) && !Player.interaction.Contains(this))
                    Player.interaction.Add(this);
            }
        }

        protected override void OnTriggerStay(Collider other)
        {
            if (sensorMode == SensorModes.Simple)
                base.OnTriggerStay(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (sensorMode == SensorModes.Simple)
            {
                base.OnTriggerEnter(other);
            }
            else
            {
                if (tags.Contains(other.tag))
                    Player.interaction.Remove(this);
            }
        }

        private void DisableHandIK()
        {
            if (handIK)
                handIK.SetActive(false);
        }

        private IEnumerator DelayedActuation(float delay, System.Action action)
        {
            actuating = true;

            yield return new WaitForSeconds(delay);
            action.Invoke();

            actuating = false;
        }

        private void Actuate(Transform other)
        {
            if (actuationMode == ActuationModes.Basic)
            {
                StartCoroutine(DelayedActuation(basicActuator.Delay, () =>
                {
                    foreach (Trigger trigger in basicActuator.Triggers)
                        trigger?.Activate(other);

                    basicActuator.OnTrigger?.Invoke();
                }));
            }
            else
            {
                StartCoroutine(DelayedActuation(switchStates[currentState].Delay, () =>
                {
                    foreach (Trigger trigger in switchStates[currentState].Triggers)
                        trigger?.Activate(other);

                    switchStates[currentState].OnTrigger?.Invoke();

                    CurrentState += switchCount;
                }));
            }

            if (sensorMode == SensorModes.Interactive && handIK)
            {
                handIK.SetActive(true);
                Invoke("DisableHandIK", 0.5f);
            }
        }

        protected override void TriggerAction(Transform other)
        {
            if (actuating)
                return;

            if (conditionMode == ConditionModes.Item)
            {
                if (Player.Instance.Inventory.HasItem(item, requiredItemCount))
                {
                    ConsumeItem();
                    base.TriggerAction(other);
                    Actuate(other);
                }
                else
                {
                    ShowMessage();
                }
            }
            else
            {
                Actuate(other);
            }
        }

        public override bool CanActivate()
        {
            return base.CanActivate() && !actuating;
        }

        ///<inheritdoc/>
        public override SaveData Save()
        {
            return new UniversalTriggerSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, done, reloading, currentState, switchCount);
        }

        ///<inheritdoc/>
        public override void Load(SaveData data)
        {
            base.Load(data);
            if (data is UniversalTriggerSaveData saveData)
            {
                currentState = saveData.currentState;
                switchCount = saveData.switchCount;
            }
        }

        /// <summary>
        /// Use player's item.
        /// </summary>
        public void ConsumeItem()
        {
            Player.Instance?.Inventory.ConsumeItem(item, requiredItemCount);
        }

        /// <summary>
        /// Show consumption item.
        /// </summary>
        public void ShowMessage()
        {
            GameUI.Instance().ShowMessage(string.Format("You need {0}x of {1}", requiredItemCount, item));
            StopCoroutine("EndCollision");
            done = false;
            colliding = false;
            reloading = true;
            Invoke("Reload", Random.Range(reloadTime.x, reloadTime.y));
        }
    }
}