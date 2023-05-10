﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OutBlock
{

    /// <summary>
    /// This class allows you to create simple moving platform.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Moveable : MonoBehaviour, ISaveable
    {

        public enum MovementTypes { Simple, Bezier };
        public enum StepTypes { Move, Wait };

        public class MoveableSaveData : SaveData
        {

            public struct MoveableRuntimeData
            {
                public bool Paused { get; set; }
                public bool Stopped { get; set; }
                public bool Done { get; set; }
                public StepTypes CurrentStep { get; set; }
                public float T { get; set; }
                public int PosIndex { get; set; }
                public int TickCount { get; set; }
            }

            public MoveableRuntimeData RuntimeData { get; private set; }

            public MoveableSaveData(int id, Vector3 pos, Vector3 rot, bool active, bool enabled, MoveableRuntimeData runtimeData) : base(id, pos, rot, active, enabled)
            {
                RuntimeData = runtimeData;
            }
        }

        /// <summary>
        /// UnityEvents.
        /// </summary>
        [System.Serializable]
        public class Events
        {

            [SerializeField]
            private UnityEvent onNextStep = default;
            /// <summary>
            /// When platform getting to the next step.
            /// </summary>
            public UnityEvent OnNextStep => onNextStep;
            [SerializeField]
            private UnityEvent onMove = default;
            /// <summary>
            /// When platform starts moving.
            /// </summary>
            public UnityEvent OnMove => onMove;
            [SerializeField]
            private UnityEvent onWait = default;
            /// <summary>
            /// When platform stops and waits until the next step.
            /// </summary>
            public UnityEvent OnWait => onWait;
            [SerializeField]
            private UnityEvent onDone = default;
            /// <summary>
            /// When platform stopped.
            /// </summary>
            public UnityEvent OnDone => onDone;

        }

        [SerializeField]
        private MovementTypes movementType = MovementTypes.Simple;
        [SerializeField, Header("Start transform")]
        private Vector3 startPos = Vector3.zero;
        [SerializeField]
        private Vector3 startRot = Vector3.zero;
        [SerializeField, Header("End transform")]
        private Vector3 endPos = Vector3.forward;
        [SerializeField]
        private Vector3 endRot = Vector3.zero;
        [SerializeField, Header("Bezier")]
        private BezierSpline spline = null;
        [SerializeField]
        private SplineWalkerMode mode = SplineWalkerMode.Loop;
        [SerializeField, Header("Timing")]
        private float waitTime = 1;
        [SerializeField]
        private float moveTime = 1;
        [SerializeField, Header("Ticks"), Tooltip("How many steps the object will do. 0 - infinite")]
        private int ticks = 0;
        [SerializeField]
        private Events events = null;

        //Debug values
        public bool paused { get; private set; }
        public bool stopped { get; private set; }
        public StepTypes currentStep { get; private set; }
        public bool done { get; private set; }
        public bool infinite { get; private set; }
        public int tickCount { get; private set; }
        public int posIndex { get; private set; }
        public float t { get; private set; }
        public float targetTime { get; private set; }

        private Vector3 stepPos;
        private Vector3 targetPos;
        private Quaternion stepRot;
        private Quaternion targetRot;
        //private Rigidbody rigid;

        private Rigidbody rigid;
        private Rigidbody Rigid
        {
            get
            {
                if (rigid == null)
                {
                    rigid = GetComponent<Rigidbody>();
                    rigid.isKinematic = true;
                    rigid.constraints = RigidbodyConstraints.FreezeRotation;
                }
                return rigid;
            }
        }

        public int Id { get; set; } = -1;

        public GameObject GO => gameObject;

        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void OnValidate()
        {
            if (ticks < 0)
                ticks = 0;
        }

        private void FixedUpdate()
        {
            if (paused || stopped)
                return;

            if (movementType == MovementTypes.Simple)
            {
                if (Rigid.position != targetPos || Rigid.rotation != targetRot)
                {
                    Vector3 newPos = Vector3.Lerp(stepPos, targetPos, t / targetTime);
                    Quaternion newRot = Quaternion.Lerp(stepRot, targetRot, t / targetTime);

                    Rigid.MovePosition(newPos);
                    Rigid.MoveRotation(newRot);
                }
            }
            else if (spline)
            {
                if (currentStep == StepTypes.Move)
                {
                    if (mode == SplineWalkerMode.Loop)
                    {
                        Rigid.MovePosition(spline.GetPoint(t / targetTime));
                    }
                    else
                    {
                        float tN = t / targetTime;
                        float tBezier = posIndex == 1 ? tN : 1f - tN;
                        Rigid.MovePosition(spline.GetPoint(tBezier));
                    }
                }
            }
        }

        private void Update()
        {
            if (!done && !paused && !stopped)
            {
                t += Time.deltaTime;

                if (t >= targetTime)
                    NextStep();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                return;

            if (movementType == MovementTypes.Bezier)
                return;

            Vector3 a = startPos;
            Vector3 b = endPos;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(a, 0.25f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(b, 0.25f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(a, b);
        }

        private void Init()
        {
            currentStep = StepTypes.Wait;
            t = 0;

            if (movementType == MovementTypes.Simple)
            {
                posIndex = 0;
                transform.position = startPos;
            }
            else if (spline)
            {
                transform.position = spline.GetPoint(0);
            }

            tickCount = ticks + 1;
            infinite = ticks == 0;
            UpdateTarget();
        }

        private void NextStep()
        {
            currentStep = currentStep == StepTypes.Wait ? StepTypes.Move : StepTypes.Wait;

            if (currentStep == StepTypes.Move)
            {
                if (!infinite)
                {
                    tickCount--;

                    if (tickCount <= 0)
                    {
                        done = true;
                        events.OnDone?.Invoke();
                        return;
                    }
                }

                posIndex++;
            }

            if (posIndex > 1)
            {
                posIndex = 0;
            }

            t = 0;

            UpdateTarget();
            events.OnNextStep?.Invoke();
            if (currentStep == StepTypes.Wait)
                events.OnWait?.Invoke();
            else events.OnMove?.Invoke();
        }

        private void UpdateTarget()
        {
            targetTime = currentStep == StepTypes.Wait ? waitTime : moveTime;

            if (movementType == MovementTypes.Simple)
            {
                stepPos = Rigid.position;
                stepRot = Rigid.rotation;

                if (currentStep == StepTypes.Wait)
                {
                    targetPos = Rigid.position;
                    targetRot = Rigid.rotation;
                }
                else
                {
                    targetPos = posIndex == 0 ? startPos : endPos;
                    targetRot = posIndex == 0 ? Quaternion.Euler(startRot) : Quaternion.Euler(endRot);
                }
            }
        }

        private void MoveToStart()
        {
            if (movementType == MovementTypes.Simple)
            {
                Rigid.position = startPos;
                Rigid.rotation = Quaternion.Euler(startRot);
            }
            else if (spline)
            {
                Rigid.position = spline.GetPoint(0);
            }
            Init();
        }

        /// <summary>
        /// Pause/Unpause movement.
        /// </summary>
        public void Pause()
        {
            paused = !paused;
        }

        /// <summary>
        /// Stop movement and return to the initial transform.
        /// </summary>
        public void Stop()
        {
            stopped = true;
            MoveToStart();
        }

        /// <summary>
        /// Stop and play again.
        /// </summary>
        public void Restart()
        {
            Stop();
            Play();
        }

        /// <summary>
        /// Resume movement.
        /// </summary>
        public void Play()
        {
            paused = false;
            stopped = false;
        }

        #region SaveLoad
        public void Register()
        {
            Init();
            SaveLoad.Add(this);
        }

        public void Unregister()
        {
            SaveLoad.Remove(this);
        }

        public SaveData Save()
        {
            MoveableSaveData.MoveableRuntimeData runtimeData = new MoveableSaveData.MoveableRuntimeData()
            {
                CurrentStep = currentStep,
                Done = done,
                Paused = paused,
                PosIndex = posIndex,
                Stopped = stopped,
                T = t,
                TickCount = tickCount
            };
            return new MoveableSaveData(Id, transform.position, transform.localEulerAngles, gameObject.activeSelf, enabled, runtimeData);
        }

        public void Load(SaveData data)
        {
            paused = true;
            SaveLoadUtils.BasicLoad(this, data);
            Rigid.position = data.pos;
            if (transform.parent)
                Rigid.rotation = transform.parent.rotation * Quaternion.Euler(data.rot);
            else
                Rigid.rotation = Quaternion.Euler(data.rot);
            if (data is MoveableSaveData saveData)
            {
                currentStep = saveData.RuntimeData.CurrentStep;
                done = saveData.RuntimeData.Done;
                paused = saveData.RuntimeData.Paused;
                posIndex = saveData.RuntimeData.PosIndex;
                stopped = saveData.RuntimeData.Stopped;
                t = saveData.RuntimeData.T;
                tickCount = saveData.RuntimeData.TickCount;
            }
            paused = false;
            UpdateTarget();
        }
        #endregion
    }
}