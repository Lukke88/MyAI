using UnityEngine;

public class UIWallButton : MonoBehaviour
{
    public WallAttachmentManager wallManager;

    public void ToggleAttachment()
    {
        wallManager.IsActivatedAttachmentToWall = !wallManager.IsActivatedAttachmentToWall;
    }
}