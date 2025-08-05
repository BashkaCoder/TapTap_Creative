using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class SimpleDeformOnCollision : MonoBehaviour
{
    [Header("Deformation Settings")]
    public float deformationRadius = 1.0f;
    public float maxDeformation = 0.3f;
    public float forceMultiplier = 1f;

    private Mesh _mesh;
    private Vector3[] _originalVertices;
    private Vector3[] _modifiedVertices;

    private void Awake()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _originalVertices = _mesh.vertices;
        _modifiedVertices = _mesh.vertices;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 collisionPoint = collision.contacts[0].point;
        Vector3 localPoint = transform.InverseTransformPoint(collisionPoint);
        float force = collision.relativeVelocity.magnitude * forceMultiplier;

        for (int i = 0; i < _modifiedVertices.Length; i++)
        {
            float distance = Vector3.Distance(_modifiedVertices[i], localPoint);
            if (distance < deformationRadius)
            {
                float deformationAmount = (1 - (distance / deformationRadius)) * maxDeformation * force;
                Vector3 direction = (_modifiedVertices[i] - localPoint).normalized;
                _modifiedVertices[i] += direction * deformationAmount;
            }
        }

        ApplyDeformation();
    }

    private void ApplyDeformation()
    {
        _mesh.vertices = _modifiedVertices;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        // Обновляем коллайдер (если нужно)
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = _mesh;
    }

    public void DeformAtPoint(Vector3 worldPoint, float force)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        for (int i = 0; i < _modifiedVertices.Length; i++)
        {
            float distance = Vector3.Distance(_modifiedVertices[i], localPoint);
            if (distance < deformationRadius)
            {
                float deformationAmount = (1 - (distance / deformationRadius)) * maxDeformation * force * forceMultiplier;
                Vector3 direction = (_modifiedVertices[i] - localPoint).normalized;
                _modifiedVertices[i] += direction * deformationAmount;
            }
        }

        ApplyDeformation();
    }
    
    [ContextMenu("Reset Mesh")]
    public void ResetMesh()
    {
        _modifiedVertices = (Vector3[])_originalVertices.Clone();
        ApplyDeformation();
    }
}
