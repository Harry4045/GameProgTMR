using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MathQuestionManager : MonoBehaviour {

    public static MathQuestionManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject _questionPanel;
    [SerializeField] private TMP_Text _questionText;
    [SerializeField] private TMP_Text _inputDisplayText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private Image _timerBar;

    [Header("Settings")]
    [SerializeField] private float _timeLimit = 8f;
    [SerializeField] private float _penaltyDuration = 5f;
    [SerializeField] private float _penaltyMultiplier = 0.2f;

    private int _correctAnswer;
    private string _typedAnswer = "";
    private float _timeRemaining;
    private bool _isQuestionActive = false;

    private CarController _playerCar;

    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() {
        _questionPanel.SetActive(false);
    }

    private void Update() {
        if (!_isQuestionActive) return;

        // --- Timer ---
        _timeRemaining -= Time.deltaTime;
        _timerText.text = Mathf.CeilToInt(_timeRemaining).ToString();
        if (_timerBar != null)
            _timerBar.fillAmount = _timeRemaining / _timeLimit;

        if (_timeRemaining <= 0f) {
            OnTimerExpired();
            return;
        }

        // --- Number Key Input (both top row and numpad) ---
        for (int i = 0; i <= 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) ||
                Input.GetKeyDown(KeyCode.Keypad0 + i)) {
                _typedAnswer += i.ToString();
                UpdateInputDisplay();
            }
        }

        // Minus sign (for negative answers)
        if (_typedAnswer.Length == 0) {
            if (Input.GetKeyDown(KeyCode.Minus) ||
                Input.GetKeyDown(KeyCode.KeypadMinus)) {
                _typedAnswer = "-";
                UpdateInputDisplay();
            }
        }

        // Backspace
        if (Input.GetKeyDown(KeyCode.Backspace) && _typedAnswer.Length > 0) {
            _typedAnswer = _typedAnswer.Substring(0, _typedAnswer.Length - 1);
            UpdateInputDisplay();
        }

        // Submit with Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) {
            CheckAnswer();
        }
    }

    public void TriggerQuestion() {
        if (_isQuestionActive) return; // Don't stack questions

        // Find the player car if not cached
        if (_playerCar == null && GameManager.Instance.player != null)
            _playerCar = GameManager.Instance.player.GetComponent<CarController>();

        GenerateQuestion();
        _typedAnswer = "";
        _timeRemaining = _timeLimit;
        _isQuestionActive = true;
        _questionPanel.SetActive(true);
        UpdateInputDisplay();
    }

    private void GenerateQuestion() {
        int type = Random.Range(0, 4); // 0=add, 1=sub, 2=mul, 3=div
        int a, b;

        switch (type) {
            case 0: // Addition
                a = Random.Range(1, 21);
                b = Random.Range(1, 21);
                _questionText.text = $"{a}  +  {b}  =  ?";
                _correctAnswer = a + b;
                break;

            case 1: // Subtraction (always positive result)
                a = Random.Range(1, 21);
                b = Random.Range(1, a + 1);
                _questionText.text = $"{a}  -  {b}  =  ?";
                _correctAnswer = a - b;
                break;

            case 2: // Multiplication (times tables up to 12)
                a = Random.Range(1, 13);
                b = Random.Range(1, 13);
                _questionText.text = $"{a}  ×  {b}  =  ?";
                _correctAnswer = a * b;
                break;

            case 3: // Division (clean whole number results only)
                b = Random.Range(2, 11);
                _correctAnswer = Random.Range(1, 11);
                a = b * _correctAnswer;
                _questionText.text = $"{a}  ÷  {b}  =  ?";
                break;

            default:
                _questionText.text = "1 + 1 = ?";
                _correctAnswer = 2;
                break;
        }
    }

   // NEW
    private void CheckAnswer() {
        if (_typedAnswer == "" || _typedAnswer == "-") return;

        if (int.TryParse(_typedAnswer, out int playerAnswer)) {
            if (playerAnswer == _correctAnswer) {
                DismissQuestion(); // Correct — no penalty
            } else {
                OnWrongAnswer();   // Wrong — penalize and dismiss
            }
        }
    }

    private void OnWrongAnswer() {
        if (_playerCar != null)
            _playerCar.ApplySpeedPenalty(_penaltyDuration, _penaltyMultiplier);

        DismissQuestion();
    }

    private void OnTimerExpired() {
        if (_playerCar != null)
            _playerCar.ApplySpeedPenalty(_penaltyDuration, _penaltyMultiplier);

        DismissQuestion();
    }

    private void DismissQuestion() {
        _isQuestionActive = false;
        _questionPanel.SetActive(false);
        _typedAnswer = "";
    }

    private void UpdateInputDisplay() {
        _inputDisplayText.text = _typedAnswer.Length > 0 ? _typedAnswer : "_";
    }
}