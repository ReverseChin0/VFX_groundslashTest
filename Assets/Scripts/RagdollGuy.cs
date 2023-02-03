using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollGuy : MonoBehaviour
{
    private class BoneTransform
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
    }

    private enum GuyState
    {
        Walking,
        Ragdoll,
        StandingUp,
        ResettingBones
    }

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private string _standUpFromBackState, _standUpFromFrontState;
    [SerializeField]
    private string _standUpFromBackClipName, _standUpFromFrontClipName;
    [SerializeField]
    private float _timeToResetBones;

    private Rigidbody[] _ragdollRigidBodies;
    private GuyState _currentState = GuyState.Walking;
    private Animator _animator;
    private CharacterController _characterController;

    /*Stand up*/
    private float _timeToWakeUp;
    private Transform _hipsBone;

    private BoneTransform[] _standUpBoneTransforms;
    private BoneTransform[] _ragdollBoneTransforms;
    private Transform[] _bones;
    private float _elapsedResetBonesTime;

    private void Awake()
    {
        _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        _hipsBone = _animator.GetBoneTransform(HumanBodyBones.Hips);

        _bones = _hipsBone.GetComponentsInChildren<Transform>();
        _standUpBoneTransforms = new BoneTransform[_bones.Length];
        _ragdollBoneTransforms = new BoneTransform[_bones.Length];

        for (int boneIndex = 0; boneIndex < _bones.Length; boneIndex++)
        {
            _standUpBoneTransforms[boneIndex] = new BoneTransform();
            _ragdollBoneTransforms[boneIndex] = new BoneTransform();
        }

        PopulateAnimationStartBoneTransforms(_standUpFromBackClipName, _standUpBoneTransforms);

        DisableRagdoll();
    }

    private void Update()
    {
        switch (_currentState)
        {
            case GuyState.Walking:
                WalkingBehaviour();
                break;
            case GuyState.Ragdoll:
                RagdollBehaviour();
                break;
            case GuyState.StandingUp:
                StandingUpBehaviour();
                break;
            case GuyState.ResettingBones:
                ResettingBonesBehaviour();
                break;
        }
    }

    void DisableRagdoll()
    {
        foreach(Rigidbody rb in _ragdollRigidBodies)
        {
            rb.isKinematic = true;
        }

        _animator.enabled = true;
        _characterController.enabled = true;
    }

    void EnableRagdoll()
    {
        foreach (Rigidbody rb in _ragdollRigidBodies)
        {
            rb.isKinematic = false;
        }
        _animator.enabled = false;
        _characterController.enabled = false;
    }

    void WalkingBehaviour()
    {
        Vector3 direction = _camera.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, 20 * Time.deltaTime); //wey gira a 20 grados por segundo
    }
    public void TriggerRagdoll(Vector3 force, Vector3 hitPoint)
    {
        EnableRagdoll();
        Rigidbody hittedRigidBody = FindHittedRigidBody(hitPoint);
        
        hittedRigidBody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
        //enable projection en el character joint nos ayuda a mejorar la estabilidad del modelo al ser impulsado
        //por una gran fuerza

        _currentState = GuyState.Ragdoll;
        _timeToWakeUp = Random.Range(3, 7);
    }

    private Rigidbody FindHittedRigidBody(Vector3 hitPoint)
    {
        Rigidbody closest = null;
        float closestDistance = 0;
        foreach (Rigidbody rb in _ragdollRigidBodies)
        {
            float distance = Vector3.SqrMagnitude(hitPoint - rb.position);
            if(closest == null || distance < closestDistance)
            {
                closestDistance = distance;
                closest = rb;
            }
        }

        return closest;
    }

    void RagdollBehaviour()
    {
        _timeToWakeUp -= Time.deltaTime;
        if(_timeToWakeUp <= 0)
        {
            AlignRotationToHips();
            AlignPositionToHips();

            PopulateBoneTransforms(_ragdollBoneTransforms);

            _currentState = GuyState.ResettingBones;
            _elapsedResetBonesTime = 0;
        }
    }

    void AlignPositionToHips()
    {
        Vector3 originialHipsPos = _hipsBone.position;
        transform.position = _hipsBone.position;

        Vector3 posOffset = _standUpBoneTransforms[0].Position; //hips
        posOffset.y = 0;
        posOffset = transform.rotation * posOffset;
        transform.position -= posOffset;

        if(Physics.Raycast(transform.position,Vector3.down, out RaycastHit hitInfo))
        {
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
        }

        _hipsBone.position = originialHipsPos;
    }

    void AlignRotationToHips()
    {
        Vector3 originalHipsPos = _hipsBone.position;
        Quaternion originalHipsRot = _hipsBone.rotation;

        Vector3 desiredDirection = _hipsBone.up * -1; /*direccion a los pies del personaje*/
        desiredDirection.y = 0;
        desiredDirection.Normalize();

        Quaternion fromToRotation = Quaternion.FromToRotation(transform.forward, desiredDirection);
        transform.rotation *= fromToRotation;

        _hipsBone.position = originalHipsPos;
        _hipsBone.rotation = originalHipsRot;
    }

    void StandingUpBehaviour()
    {
        if(_animator.GetCurrentAnimatorStateInfo(0).IsName(_standUpFromBackState) == false) //si esto es falso, la animacion debe haber terminado
        {
            _currentState = GuyState.Walking;
        }
    }

    void ResettingBonesBehaviour()
    {
        _elapsedResetBonesTime += Time.deltaTime;
        float elapsedPercentage = _elapsedResetBonesTime / _timeToResetBones;

        for (int boneIndex = 0; boneIndex < _bones.Length; boneIndex++)
        {
            _bones[boneIndex].localPosition = Vector3.Lerp(
                _ragdollBoneTransforms[boneIndex].Position,
                _standUpBoneTransforms[boneIndex].Position, 
                elapsedPercentage);

            _bones[boneIndex].localRotation = Quaternion.Lerp(
                _ragdollBoneTransforms[boneIndex].Rotation,
                _standUpBoneTransforms[boneIndex].Rotation,
                elapsedPercentage);
        }

        if(elapsedPercentage >= 1)
        {
            _currentState = GuyState.StandingUp;
            DisableRagdoll();

            _animator.Play(_standUpFromBackState);
        }
    }

    void PopulateBoneTransforms(BoneTransform[] boneTransforms)
    {
        for (int boneIndex = 0; boneIndex < _bones.Length; boneIndex++)
        {
            boneTransforms[boneIndex].Position = _bones[boneIndex].localPosition;
            boneTransforms[boneIndex].Rotation = _bones[boneIndex].localRotation;
        }
        //Obteniendo una snapshot de la posicion actual en la lista que pasemos
    }

    void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
    {
        Vector3 posBeforeSampling = transform.position;
        Quaternion rotBeforeSampling = transform.rotation;

        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if(clip.name == clipName)
            {
                clip.SampleAnimation(gameObject, 0);
                PopulateBoneTransforms(_standUpBoneTransforms);
                break;
            }
        }

        transform.position = posBeforeSampling;
        transform.rotation = rotBeforeSampling;
    }

}
