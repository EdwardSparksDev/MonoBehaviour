using UnityEngine;


namespace PhysicsExtensions
{
    public static class Extensions
    {
        /// <summary>
        /// Method used for raycasting a cicle collider in a certain direction
        /// </summary>
        /// <param name="rigidbody">The rigidbody casting the ray</param>
        /// <param name="radius">The circlecast radius</param>
        /// <param name="direction">The circlecast direction</param>
        /// <param name="distance">The circlecast distance</param>
        /// <param name="layerMask">The layers the be considered in the circlecast</param>
        /// <returns></returns>
        public static bool Raycast(this Rigidbody2D rigidbody, float radius, Vector2 direction, float distance, LayerMask layerMask)
        {
            if (rigidbody.isKinematic) return false;

            RaycastHit2D hit = Physics2D.CircleCast(rigidbody.position, radius, direction.normalized, distance, layerMask);
            return hit.collider != null && hit.rigidbody != rigidbody;
        }


        /// <summary>
        /// Method used for performing dot products between two transforms positions and the selected direction
        /// </summary>
        /// <param name="transform">The first transform</param>
        /// <param name="other">The second transform</param>
        /// <param name="direction">The dot product direction</param>
        /// <param name="maxValidAngle">The maximum dot product valid angle</param>
        /// <returns></returns>
        public static bool Dot(this Transform transform, Transform other, Vector2 direction, float maxValidAngle)
        {
            Vector2 otherDirection = (other.position - transform.position).normalized;
            return Vector2.Dot(otherDirection, direction.normalized) > Mathf.Cos(maxValidAngle);
        }


        /// <summary>
        /// Method used for checking if an object is colliding on the upper face of a boxCollider2D
        /// </summary>
        /// <param name="col">The boxCollider2D</param>
        /// <param name="other">The object transform</param>
        /// <returns></returns>
        public static bool IsUpwardFaceContact(this BoxCollider2D col, Transform other)
        {
            float length = Mathf.Sqrt(Mathf.Pow(col.size.x / 2f, 2) + Mathf.Pow(col.size.y / 2f, 2));
            float angle = Mathf.Acos(col.size.y / 2 / length) * Mathf.Rad2Deg;
            float projectileHitAngle = Mathf.Acos(Vector3.Dot((other.position - col.transform.position).normalized, Vector3.up)) * Mathf.Rad2Deg;

            /* DEBUG
            Debug.DrawRay(col.transform.position, Quaternion.Euler(0f, 0f, angle) * Vector3.up * length, Color.magenta, 10f);
            Debug.DrawRay(col.transform.position, Quaternion.Euler(0f, 0f, -angle) * Vector3.up * length, Color.magenta, 10f);
            Debug.Log(projectileHitAngle + " < " + angle + " ?");
            */

            if (projectileHitAngle < angle)
                return true;
            return false;
        }
    }
}


