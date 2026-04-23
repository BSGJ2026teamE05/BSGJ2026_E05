// ---------------------------------------------------------
// HaiHaiAnime.cs
// 作成日:  2026/4/24
// 作成者:  佐々木
// 概要:はいはいアニメ
// ---------------------------------------------------------
using System.Collections;
using UnityEngine;
public class HaiHaiAnime : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Vector3 _haiHaiPosition;
    [SerializeField] private Vector3 _idlePosition;
    [SerializeField] private Vector3 _haiHaiRotation;
    [SerializeField] private Vector3 _idleRotation;
    private Coroutine _stopCoroutine;
    private bool _isMoving = false;
    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }
    public void OnInput()
    {
        if (!_isMoving)
        {
            _isMoving = true;
            _animator.SetBool("HaiHai", true);
            transform.rotation = Quaternion.Euler(_haiHaiRotation);
            transform.localPosition = _haiHaiPosition;
        }
        if (_stopCoroutine != null)
        {
            StopCoroutine(_stopCoroutine);
            _stopCoroutine = null;
        }
        _stopCoroutine = StartCoroutine(StopDelay());
    }
    private void Update()
    {
    }
    private IEnumerator StopDelay()
    {
        yield return new WaitForSeconds(1f);
        _animator.SetBool("HaiHai", false);
        _isMoving = false;
        _stopCoroutine = null;
        transform.rotation = Quaternion.Euler(_idleRotation);
        transform.localPosition = _idlePosition;
    }
}