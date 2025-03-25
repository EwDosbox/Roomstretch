using System.Collections;
using UnityEngine;

public class DoorTeleportScript : MonoBehaviour
{
    [SerializeField] public Vector3 Destination;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject textGO;
    private PlayerInputScript playerInputScript;
    private bool isTeleporting = false;

    private void Awake()
    {
        canvasGroup = GameObject.Find("BlackBackground").GetComponent<CanvasGroup>();
        player = GameObject.Find("Player");
        textGO = GameObject.Find("TeleportText");
        playerInputScript = player.GetComponent<PlayerInputScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            textGO.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isTeleporting && other.gameObject == player && playerInputScript.ShouldTeleport)
        {
            isTeleporting = true;
            StartCoroutine(FadeAndTeleport());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == player)
        {
            textGO.SetActive(false);
        }
    }

    private IEnumerator FadeAndTeleport()
    {
        yield return StartCoroutine(Fade(canvasGroup, 1f, FadeEnum.In));
        player.transform.position = Destination;
        yield return null;
        yield return StartCoroutine(Fade(canvasGroup, 1f, FadeEnum.Out));
        isTeleporting = false;
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float time, FadeEnum fade)
    {
        if (fade == FadeEnum.In)
        {
            canvasGroup.alpha = 0;
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime / time;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
        else if (fade == FadeEnum.Out)
        {
            canvasGroup.alpha = 1;
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime / time;
                yield return null;
            }
            canvasGroup.alpha = 0;
        }
    }
    public enum FadeEnum
    {
        In,
        Out
    }
}