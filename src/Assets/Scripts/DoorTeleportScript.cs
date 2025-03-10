using System.Collections;
using UnityEngine;

public class DoorTeleportScript : MonoBehaviour
{
    [SerializeField] public Vector3 Destination;
    private CanvasGroup canvasGroup;
    private GameObject Player;
    private PlayerInputScript playerInputScript;

    private void Awake()
    {
        Player = GameObject.Find("Player");
        canvasGroup = GameObject.Find("BlackBG").GetComponent<CanvasGroup>(); // Find the CanvasGroup component
        //Co m
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
        yield return StartCoroutine(Fade(canvasGroup, 2f, FadeEnum.In));
        Player.transform.position = Destination;
        yield return StartCoroutine(Fade(canvasGroup, 2f, FadeEnum.Out));
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
}

public enum FadeEnum
{
    In,
    Out
}
