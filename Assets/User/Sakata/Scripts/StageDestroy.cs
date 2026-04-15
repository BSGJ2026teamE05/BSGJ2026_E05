// ---------------------------------------------------------
// StageDestroy.cs
// 作成日:  2026/4/13
// 作成者:  坂田
// 概要:　ステージオブジェクトの破壊
// ---------------------------------------------------------
using System.Collections;
using UnityEngine;

public class StageDestroy : MonoBehaviour
{
    [SerializeField] private float _collisionForce = 10f;
    [SerializeField] private Vector3 _forceDirection = new Vector3(0f, 0.5f, 1f);
    [SerializeField] private float _destroyDelay = 3f;

    private Rigidbody _rb;
    private BoxCollider _col;
    private bool _isDestroyed = false;

    private void Awake()
    {
        _col = GetComponent<BoxCollider>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDestroyed) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            _isDestroyed = true;
            _rb.constraints = RigidbodyConstraints.None;
            _rb.AddForce(_forceDirection.normalized * _collisionForce, ForceMode.Impulse);
            //_col.isTrigger = true;
            Physics.IgnoreCollision(_col, collision.collider);
            StartCoroutine(DestroyObject());
        }
    }

    private IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(_destroyDelay);
        //Destroy(gameObject);
        gameObject.SetActive(false);
    }
}