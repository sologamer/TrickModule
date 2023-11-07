using UnityEngine;

namespace TrickModule.Randomizer
{
    public static class TransformExtensions
    {
        public static Vector3 GetRandomPointInsideCollider(this BoxCollider boxCollider, IRandomizer randomizer)
        {
            Vector3 extents = boxCollider.size / 2f;
            Vector3 point = new Vector3(
                randomizer.Next(-extents.x, extents.x),
                randomizer.Next(-extents.y, extents.y),
                randomizer.Next(-extents.z, extents.z)
            ) + boxCollider.center;
            return boxCollider.transform.TransformPoint(point);
        }
    }
}