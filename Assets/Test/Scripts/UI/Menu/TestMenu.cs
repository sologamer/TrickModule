using System.Collections;
using System.Collections.Generic;
using TrickModule.Game;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TestMenu : UIMenu
{
    [FormerlySerializedAs("TestImageReference"), SerializeField]
    private string testImageReference = "3574c7cf6ce2817b85effc43fdb526d5353091543a5ae66bc5add468d1241b27";

    [FormerlySerializedAs("TestImageFullReference"), SerializeField]
    private string testImageFullReference = "https://njuka.s3.nl-ams.scw.cloud/images/3574c7cf6ce2817b85effc43fdb526d5353091543a5ae66bc5add468d1241b27.png";

    [FormerlySerializedAs("RemoteImageTest"), SerializeField]
    private Image remoteImageTest;

    [FormerlySerializedAs("RemoteImageTest2"), SerializeField]
    private Image remoteImageTest2;

    [FormerlySerializedAs("RemoteAudioSource"), SerializeField]
    private AudioSource remoteAudioSource;

    [FormerlySerializedAs("RemoteAudioTest"), SerializeField]
    private string remoteAudioTest = "https://kid-learning.s3.nl-ams.scw.cloud/words/nl/Laura/aan.mp3";

    protected override void MenuInitialize()
    {
        base.MenuInitialize();

        Debug.Log("TestMenu.MenuInitialize");
    }

    protected override void MenuStart()
    {
        base.MenuStart();

        Debug.Log("TestMenu.MenuStart");
    }

    public override UIMenu Show()
    {
        Debug.Log("TestMenu.Show");
        remoteImageTest.SetSprite(testImageReference);
        remoteImageTest2.SetSprite(testImageFullReference, callback: sprite =>
        {
            if (sprite != null)
                AssetManager.Instance.RemoveAsset(testImageFullReference);
        });
        remoteAudioSource.SetAudio(remoteAudioTest);
        return base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        remoteImageTest.sprite = null;
        remoteImageTest2.sprite = null;
        Debug.Log("TestMenu.Hide");
    }
}