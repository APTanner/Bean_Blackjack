using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup m_mainMenu;
    [SerializeField] private CanvasGroup m_cardBacks;
    [SerializeField] private CanvasGroup m_rules;
    [SerializeField] private CanvasGroup m_game;

    public GameManager Game;

    private CanvasGroup m_activeGroup;

    private void Awake()
    {
        m_activeGroup = m_mainMenu;
        m_mainMenu.alpha = 1.0f;

        m_cardBacks.alpha = m_rules.alpha = m_game.alpha = 0f;
        m_cardBacks.gameObject.SetActive(false);
        m_rules.gameObject.SetActive(false);
        m_game.gameObject.SetActive(false);

        Transform camTransform = Camera.main.transform;
        camTransform.SetPositionAndRotation(Globals.CAM_DEFAULT_POS, Quaternion.Euler(Globals.CAM_DEFAULT_ROT));
    }

    public void OnPlay()
    {
        StartCoroutine(Transition(Globals.CAM_PLAY_POS, Globals.CAM_PLAY_ROT, m_game, m_activeGroup));
        m_activeGroup = m_game;
    }

    public void OnCardBacks()
    {
        if (Game.IsInGame)
        {
            return;
        }
        StartCoroutine(Transition(Globals.CAM_BACKS_POS, Globals.CAM_BACKS_ROT, m_cardBacks, m_activeGroup));
        m_activeGroup = m_cardBacks;
    }

    public void OnRules()
    {
        StartCoroutine(Transition(Globals.CAM_RULES_POS, Globals.CAM_RULES_ROT, m_rules, m_activeGroup));
        m_activeGroup = m_rules;
    }

    public void OnMainMenu()
    {
        StartCoroutine(Transition(Globals.CAM_DEFAULT_POS, Globals.CAM_DEFAULT_ROT, m_mainMenu, m_activeGroup));
        m_activeGroup = m_mainMenu;
    }

    private IEnumerator Transition(Vector3 pos, Vector3 rot, CanvasGroup fadeIn, CanvasGroup fadeOut)
    {
        StartCoroutine(FadeOutCoroutine(fadeOut));
        StartCoroutine(MoveCamera(pos, rot));

        yield return new WaitForSeconds(Globals.MOVE_TRANSITION_TIME - Globals.FADE_TRANSITION_TIME);

        StartCoroutine(FadeInCoroutine(fadeIn));
    }

    private IEnumerator FadeOutCoroutine(CanvasGroup canvas)
    {
        float startAlpha = canvas.alpha;
        float elapsedTime = 0f;

        float fadeDuration = Globals.FADE_TRANSITION_TIME;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvas.alpha = 0f;
        canvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeInCoroutine(CanvasGroup canvas)
    {
        canvas.gameObject.SetActive(true);

        float startAlpha = canvas.alpha;
        float elapsedTime = 0f;

        float fadeDuration = Globals.FADE_TRANSITION_TIME;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvas.alpha = 1f;
    }

    private IEnumerator MoveCamera(Vector3 pos, Vector3 rot)
    {
        float time = 0f;
        float moveTime = Globals.MOVE_TRANSITION_TIME;
        
        Transform t = Camera.main.transform;
        Vector3 originalPos = t.position;
        Quaternion originalRot = t.rotation;
        Quaternion newRot = Quaternion.Euler(rot);

        while (time < moveTime)
        {
            time += Time.deltaTime;
            t.position = Vector3.Lerp(originalPos, pos, time / moveTime);
            t.rotation = Quaternion.Slerp(originalRot, newRot, time / moveTime);
            yield return null;
        }

        t.position = pos;
        t.eulerAngles = rot;
    }

    
}
