using System.Collections;
using UnityEngine;

public class DoorTeleportScript : MonoBehaviour
{
    [SerializeField] private Vector3 Destination;
    [SerializeField] private GameObject Player;
    [SerializeField] private CanvasGroup canvasGroup;
    private PlayerInputScript playerInputScript;

    private void Awake()
    {
        playerInputScript = Player.GetComponent<PlayerInputScript>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == Player && playerInputScript.ShouldTeleport)
        {
            StartCoroutine(FadeAndTeleport());
        }
    }

    private IEnumerator FadeAndTeleport()
    {
        yield return StartCoroutine(Fade(canvasGroup, 2f, Fade.In));
        Player.transform.position = Destination;
        yield return StartCoroutine(Fade(canvasGroup, 2f, Fade.Out));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float time, Fade fade)
    {
        if (fade == Fade.In)
        {
            canvasGroup.alpha = 0;
            while (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime / time;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
        else if (fade == Fade.Out)
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
}

public enum Fade
{
    In,
    Out
}
