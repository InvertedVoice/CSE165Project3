using UnityEngine;
using Debug = UnityEngine.Debug;

public class HandGestureController : MonoBehaviour
{
    [Header("Hand Tracking")]
    public OVRHand rightHand;
    public OVRSkeleton rightSkeleton;

    [Header("Settings")]
    public LayerMask surfaceMask;
    public AgentController agentController;

    [Header("Ray Visualization")]
    public LineRenderer lineRenderer;

    // Cached bone transforms
    private Transform wrist;
    private Transform indexTip;
    private Transform indexProximal;
    private Transform indexDistal;
    private Transform middleProximal, middleDistal, middleTip;
    private Transform ringProximal, ringDistal, ringTip;
    private Transform pinkyProximal, pinkyDistal, pinkyTip;
    private bool bonesInitialized = false;

    private Vector3 destination;
    private bool destinationSet = false;

    void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
        }
    }

    void InitializeBones()
    {
        var bones = rightSkeleton.Bones;
        if (bones == null || bones.Count == 0) return;

        foreach (var bone in bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.XRHand_Wrist) wrist = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexTip) indexTip = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexProximal) indexProximal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_IndexDistal) indexDistal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_MiddleProximal) middleProximal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_MiddleDistal) middleDistal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_MiddleTip) middleTip = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_RingProximal) ringProximal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_RingDistal) ringDistal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_RingTip) ringTip = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_LittleProximal) pinkyProximal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_LittleDistal) pinkyDistal = bone.Transform;
            if (bone.Id == OVRSkeleton.BoneId.XRHand_LittleTip) pinkyTip = bone.Transform;
        }

        if (wrist != null && indexTip != null && indexProximal != null && indexDistal != null)
            bonesInitialized = true;
    }

    bool IndexExtended()
    {
        if (!bonesInitialized) return false;
        var a = indexProximal.position;
        var b = indexDistal.position;
        var tip = indexTip.position;
        return Vector3.Dot((b - a).normalized, (tip - b).normalized) > 0.85f;
    }

    bool FingerCurled(Transform proximal, Transform distal, Transform tip)
    {
        if (proximal == null || distal == null || tip == null) return false;
        return Vector3.Dot((distal.position - proximal.position).normalized,
                           (tip.position - distal.position).normalized) < 0.5f;
    }

    void Update()
    {
        if (rightHand == null || rightSkeleton == null) return;
        if (!rightHand.IsTracked) return;
        if (rightHand.HandConfidence != OVRHand.TrackingConfidence.High) return;

        if (!bonesInitialized) InitializeBones();
        if (!bonesInitialized) return;

        bool middleCurled = FingerCurled(middleProximal, middleDistal, middleTip);
        bool ringCurled = FingerCurled(ringProximal, ringDistal, ringTip);
        bool pinkyCurled = FingerCurled(pinkyProximal, pinkyDistal, pinkyTip);

        if (IndexExtended() && middleCurled && ringCurled && pinkyCurled)
        {
            TrySetDestination();
        }
        else
        {
            lineRenderer.enabled = false;
            destinationSet = false;
        }
    }

    void TrySetDestination()
    {
        Vector3 dir = (indexTip.position - wrist.position).normalized;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, indexTip.position);

        if (Physics.Raycast(indexTip.position, dir, out RaycastHit hit, 10f, surfaceMask))
        {
            destination = hit.point;
            destinationSet = true;
            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;

            if (agentController != null)
                agentController.SetDestination(hit.point);

            Debug.Log("Destination set: " + destination);
        }
        else
        {
            lineRenderer.SetPosition(1, indexTip.position + dir * 10f);
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
    }

    public Vector3 GetDestination() => destination;
    public bool HasDestination() => destinationSet;
    public void ClearDestination() => destinationSet = false;
}