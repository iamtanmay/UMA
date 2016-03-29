using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

// this class implements the whole feet planting solution for a specific animator
public class FootPlanting
{
    private Animator m_Animator = null;

    // body and feet kinematics (position, velocity and acceleration), feet stabilization and body postion smoothing
    private AnimatorKinematics m_AnimatorKinematics = null;

    // plant feets on the ground (uncomment lines 15, 41, 57, 59 and 61 to enable foot planting, it's currently experimental).
    //private FootPlanting m_FootPlanting = null;

    // make sure this matches humanoid feet size
    //    private float m_BackFootOffset = -0.0275f;
    //    private float m_FrontFootOffset = +0.200f;

    // play steps sound
    public System.Action OnLeftFootPlantAction = null;
    public System.Action OnRightFootPlantAction = null;

    private bool useFootStepSound = true;
    //[HideInInspector]
    //    private bool useBodyPositionDamping;
    private LayerMask footFallSoundsLayerMask;

    // The different surfaces and their sounds.
    public AudioSurface[] surfaces;
    public void FootPlantStart(FootStepSounds footStepSounds)
    {
        //        m_BackFootOffset = footStepSounds.m_BackFootOffset;
        //        m_FrontFootOffset = footStepSounds.m_FrontFootOffset;
        useFootStepSound = footStepSounds.useFootStepSound;
        //        useBodyPositionDamping = footStepSounds.useBodyPositionDamping;
        footFallSoundsLayerMask = footStepSounds.footFallSoundsLayerMask;
        surfaces = footStepSounds.surfaces;

        m_Animator = footStepSounds.m_Animator;
        m_AnimatorKinematics = new AnimatorKinematics(m_Animator);
        //m_FootPlanting = new FootPlanting (m_Animator, m_BackFootOffset, m_FrontFootOffset, ~m_Animator.gameObject.layer);
        OnLeftFootPlantAction += OnLeftFootPlant;
        OnRightFootPlantAction += OnRightFootPlant;

    }

    public void FootPlantOnAnimatorIK(int layerIndex)
    {
        if (layerIndex == 0)
        {
            float deltaTime = Time.deltaTime;

            // update animator kinematics
            m_AnimatorKinematics.Update(deltaTime);

            // This is commented out because it is experimental, uncomment it at your own risk!
            // stabilize feet based on kinematics
            // m_AnimatorKinematics.StabilizeFeet();
            // foot plant from stable feet position
            // m_FootPlanting.FeetPlant(m_AnimatorKinematics.m_LeftFootStabilizer.m_Weight,m_AnimatorKinematics.m_RightFootStabilizer.m_Weight);
            // smooth body position modification induced by foot planting
            // m_AnimatorKinematics.SmoothBody(deltaTime);

            // foot plant actions for left and right
            if (m_AnimatorKinematics.m_LeftFootStabilizer.m_Stabilizing && OnLeftFootPlantAction != null)
                OnLeftFootPlantAction();
            if (m_AnimatorKinematics.m_RightFootStabilizer.m_Stabilizing && OnRightFootPlantAction != null)
                OnRightFootPlantAction();
        }
    }

    // get foot bottom position
    Vector3 GetFootPosition(bool left)
    {
        AvatarIKGoal ikGoal = left ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
        float footBottomHeight = left ? m_Animator.leftFeetBottomHeight : m_Animator.rightFeetBottomHeight;

        Vector3 footPos = m_Animator.GetIKPosition(ikGoal);
        Quaternion footRot = m_Animator.GetIKRotation(ikGoal);

        footPos += footRot * new Vector3(0, -footBottomHeight, 0);

        return footPos;
    }

    void OnLeftFootPlant()
    {
        PlayFootFallSound(true);
    }

    void OnRightFootPlant()
    {
        PlayFootFallSound(false);
    }

    void PlayFootFallSound(bool left)
    {
        Vector3 position = GetFootPosition(left);

        if (!useFootStepSound)
            return;

        RaycastHit hit;
        if (!Physics.Raycast(position + Vector3.up, -Vector3.up, out hit, 1.5f, footFallSoundsLayerMask))
            return;

        for (int i = 0; i < surfaces.Length; i++)
        {
            if (surfaces[i].tag == hit.collider.tag)
            {
                surfaces[i].PlayRandomClip();
            }
        }
    }
}

[System.Serializable]
public class AudioSurface
{
    public string tag;              // The tag on the surfaces that play these sounds.
    public AudioClip[] clips;       // The different clips that can be played on this surface.
    public AudioSource source;      // The AudioSource that will play the clips.

    //private FisherYatesRandom randomSource = new FisherYatesRandom();       // For randomly reordering clips.

    public AudioSurface(string tag)
    {
        this.tag = tag;
    }


    public void SetSource(Animator animator)
    {
        // The audio source is on a specifically named child.
        source = animator.transform.Find("PlayerAudio/FootStep" + tag).GetComponent<AudioSource>();
    }


    public void PlayRandomClip()
    {
        // If there are no clips to play return.
        if (clips == null || clips.Length == 0)
            return;

        // Find a random clip and play it.
        int index = Next(clips.Length);
        source.PlayOneShot(clips[index]);
    }


    private int[] randomIndices = null;
    private int randomIndex = 0;
    private int prevValue = -1;
    public int Next(int len)
    {
        if (len <= 1)
            return 0;

        if (randomIndices == null || randomIndices.Length != len)
        {
            randomIndices = new int[len];
            for (int i = 0; i < randomIndices.Length; i++)
                randomIndices[i] = i;
        }

        if (randomIndex == 0)
        {
            int count = 0;
            do
            {
                for (int i = 0; i < len - 1; i++)
                {
                    int j = Random.Range(i, len);
                    if (j != i)
                    {
                        int tmp = randomIndices[i];
                        randomIndices[i] = randomIndices[j];
                        randomIndices[j] = tmp;
                    }
                }
            } while (prevValue == randomIndices[0] && ++count < 10); // Make sure the new first element is different from the last one we played
        }

        int value = randomIndices[randomIndex];
        if (++randomIndex >= randomIndices.Length)
            randomIndex = 0;

        prevValue = value;
        return value;
    }
}
