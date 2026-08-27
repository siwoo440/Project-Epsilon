using UnityEngine;

namespace ProjectEpsilon.Player
{
    public sealed class SnakeContactHazard : MonoBehaviour
    {
        [SerializeField] private SnakeContactHazardType hazardType;

        public SnakeContactHazardType HazardType => hazardType;

        public void Configure(SnakeContactHazardType type)
        {
            hazardType = type;
        }
    }
}
