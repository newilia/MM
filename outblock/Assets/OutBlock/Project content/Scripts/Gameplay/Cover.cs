using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace OutBlock
{

    /// <summary>
    /// Cover for the AIs.
    /// </summary>
    public class Cover : MonoBehaviour
    {

        [SerializeField]
        private Mesh viewMesh = null;
        [SerializeField]
        private Vector2 timeInCover = new Vector2(2, 4);
        [SerializeField]
        private int id = 0;

        private Vector3? navMeshPos = null;
        public Vector2 TimeInCover => timeInCover;

        public bool Taken { get; private set; }

        /// <summary>
        /// List of the covers.
        /// </summary>
        public static List<Cover> covers { get; private set; } = new List<Cover>();

        private void Start()
        {
            bool success = false;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3, NavMesh.AllAreas))
            {
                if (Mathf.Abs(hit.position.y - transform.position.y) < 0.75f)
                {
                    success = true;
                    navMeshPos = hit.position;
                }
            }

            if (!success)
            {
                Debug.LogError(string.Format("Cover \"{0}\" is not on the NavMesh! It will be ignored.", transform.name));
            }
        }

        private void OnEnable()
        {
            covers.Add(this);
        }

        private void OnDisable()
        {
            covers.Remove(this);
        }

        private void OnDrawGizmos()
        {
            if (!Taken)
                Gizmos.color = Color.yellow * 0.75f;
            else Gizmos.color = Color.red * 0.75f;
            if (navMeshPos == null && Application.isPlaying)
                Gizmos.color = Color.gray;
            Gizmos.DrawMesh(viewMesh, transform.position, transform.rotation);

            if (navMeshPos == null)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(navMeshPos.Value, 0.5f);
        }

        /// <summary>
        /// Take this cover. Don't use it carelessly, better to use the <see cref="GetCover(Vector3, Vector3, float, out Cover)"/>.
        /// </summary>
        public void Take()
        {
            Taken = true;
        }

        /// <summary>
        /// Free this cover.
        /// </summary>
        public void Free()
        {
            Taken = false;
        }

        /// <summary>
        /// Find closest cover.
        /// </summary>
        /// <param name="pos">Entity position.</param>
        /// <param name="target">Entity target/enemy.</param>
        /// <param name="maxDist">Max distance for the cover.</param>
        /// <param name="cover">Found cover. Can be null.</param>
        public static bool GetCover(Vector3 pos, Vector3 target, float maxDist, int targetId, out Cover cover)
        {
            cover = null;
            if (covers.Count <= 0)
            {
                return false;
            }

            List<Cover> availableCovers = covers.Where(x => !x.Taken && x.id == targetId && x.navMeshPos != null).ToList();
            if (availableCovers.Count <= 0)
            {
                return false;
            }

            Dictionary<Cover, float> finalCovers = new Dictionary<Cover, float>();

            Vector3 targetDir = pos - target;
            for (int i = 0; i < availableCovers.Count; i++)
            {
                Vector3 dir = target - availableCovers[i].transform.position;
                if (Vector3.Dot(targetDir, dir) > 0)
                    continue;

                float dist = Vector3.Distance(availableCovers[i].transform.position, pos);
                if (dist <= maxDist)
                {
                    finalCovers.Add(availableCovers[i], dist);
                }
            }

            if (finalCovers.Count <= 0)
                return false;

            finalCovers = finalCovers.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            NavMeshPath path = new NavMeshPath();
            foreach(KeyValuePair<Cover, float> pair in finalCovers)
            {
                bool found = NavMesh.CalculatePath(pos, pair.Key.navMeshPos.Value, NavMesh.AllAreas, path);
                if (found && path.status == NavMeshPathStatus.PathComplete)
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        pair.Key.Take();
                        cover = pair.Key;
                        return true;
                    }
                    else if (path.status == NavMeshPathStatus.PathPartial)
                    {
                        float dist = Vector3.Distance(pos, path.corners[path.corners.Length - 1]);
                        if (dist <= 3)
                        {
                            pair.Key.Take();
                            cover = pair.Key;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}