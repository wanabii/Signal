using UnityEngine;

public class FootstepSurface : MonoBehaviour
{
    [SerializeField] private FootstepSurfaceType surfaceType = FootstepSurfaceType.Grass;
    public FootstepSurfaceType SurfaceType => surfaceType;
}