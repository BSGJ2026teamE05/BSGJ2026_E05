// ---------------------------------------------------------
// HaiHaiAnime.cs
// 作成日:  2026/4/24
// 作成者:  佐々木
// 概要:はいはいアニメ
// ---------------------------------------------------------
//using System.Collections;
//using UnityEngine;
//public class HaiHaiAnime : MonoBehaviour
//{
//    [SerializeField] private Animator _animator;
//    [SerializeField] private Vector3 _haiHaiPosition;
//    [SerializeField] private Vector3 _idlePosition;
//    [SerializeField] private Vector3 _haiHaiRotation;
//    [SerializeField] private Vector3 _idleRotation;
//    private Coroutine _stopCoroutine;
//    private bool _isMoving = false;
//    private void Awake()
//    {
//        if (_animator == null)
//            _animator = GetComponent<Animator>();
//    }
//    public void OnInput()
//    {
//        if (!_isMoving)
//        {
//            _isMoving = true;
//            _animator.SetBool("HaiHai", true);
//            transform.rotation = Quaternion.Euler(_haiHaiRotation);
//            transform.localPosition = _haiHaiPosition;
//        }
//        if (_stopCoroutine != null)
//        {
//            StopCoroutine(_stopCoroutine);
//            _stopCoroutine = null;
//        }
//        _stopCoroutine = StartCoroutine(StopDelay());
//    }
//    private void Update()
//    {
//    }
//    private IEnumerator StopDelay()
//    {
//        yield return new WaitForSeconds(1f);
//        _animator.SetBool("HaiHai", false);
//        _isMoving = false;
//        _stopCoroutine = null;
//        transform.rotation = Quaternion.Euler(_idleRotation);
//        transform.localPosition = _idlePosition;
//    }
//}
using UnityEngine;
using System.Collections;

public class HaiHaiAnime : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _modelOffset; // 中間親オブジェクト(Model_Offset)をアサイン

    [Header("ハイハイ時の補正値")]
    [SerializeField] private Vector3 _movingRotation = new Vector3(90f, 0f, 0f); // 仰け反りを打ち消す角度
    [SerializeField] private float _movingHeight = 0.5f; // 地面から持ち上げる高さ

    private Coroutine _stopCoroutine;
    private bool _isMoving = false;

    public void OnInput()
    {
        if (!_isMoving)
        {
            _isMoving = true;
            _animator.SetBool("HaiHai", true);

            // 【親子構造での制御】動いた瞬間に親の角度と高さを変える
            if (_modelOffset != null)
            {
                _modelOffset.localRotation = Quaternion.Euler(_movingRotation);
                _modelOffset.localPosition = new Vector3(0, _movingHeight, 0);
            }
        }

        if (_stopCoroutine != null) StopCoroutine(_stopCoroutine);
        _stopCoroutine = StartCoroutine(StopDelay());
    }

    private IEnumerator StopDelay()
    {
        yield return new WaitForSeconds(1f);
        _animator.SetBool("HaiHai", false);
        _isMoving = false;

        // 【親子構造での制御】止まったら元に戻す
        if (_modelOffset != null)
        {
            _modelOffset.localRotation = Quaternion.identity;
            _modelOffset.localPosition = Vector3.zero;
        }
        _stopCoroutine = null;
    }
}