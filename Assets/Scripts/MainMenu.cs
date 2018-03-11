using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    //Constant var
    private const float CAMERA_ROTATE_SPEED = 3.0f;

    public GameObject btnLevelPrefab;
    public GameObject levelButtonContainer;
    public GameObject btnShopPrefab;
    public GameObject shopItemContainer;

    public Text currencyText; // currency amount display text in shop

    public Material playerMaterial;

    private Transform cameraTrans;
    private Transform cameraDesiredLookAt;

    private void Start()
    {
        //PlayerPrefs.DeleteAll();
        changePlayerSkin(GameManager.Instance.currentSkinIndex);
        cameraTrans = Camera.main.transform;
        currencyText.text = "Currency: " + GameManager.Instance.currency; // change display of currency in shop panel
        Sprite[] thumbnails = Resources.LoadAll<Sprite>("Levels");

        // Sets the image of each button with each img in the levels resouces folder
        foreach(Sprite thumbnail in thumbnails)
        {
            GameObject container = Instantiate(btnLevelPrefab) as GameObject;
            container.GetComponent<Image>().sprite = thumbnail;
            container.transform.SetParent(levelButtonContainer.transform, false);

            string sceneName = thumbnail.name;

            container.GetComponent<Button>().onClick.AddListener(() => loadLevel(sceneName));
        }

        int textureIndex = 0;
        Sprite[] textures = Resources.LoadAll<Sprite>("Player");
        foreach (Sprite texture in textures)
        {
            GameObject container = Instantiate(btnShopPrefab) as GameObject;
            container.GetComponent<Image>().sprite = texture;
            container.transform.SetParent(shopItemContainer.transform, false);

            int index = textureIndex;
            container.GetComponent<Button>().onClick.AddListener(() => changePlayerSkin(index));
            if (((GameManager.Instance.skinAvailability & 1 << index) == 1 << index))
            {
                container.transform.GetChild(0).gameObject.SetActive(false);
            }
            textureIndex++;
        }
    }

    private void Update()
    {
        // rotate camera if not null
        if ( cameraDesiredLookAt != null)
        {
            cameraTrans.rotation = Quaternion.Slerp(cameraTrans.rotation, cameraDesiredLookAt.rotation,  CAMERA_ROTATE_SPEED * Time.deltaTime);
        }
    }

    private void loadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void lookAtMenu(Transform menuTrans)
    {
        cameraDesiredLookAt = menuTrans;
    }

    private void changePlayerSkin(int index)
    {
        // Used for bit flaging
        // ex
        //grey   1 -
        //purple 2
        //red    4 -
        //black  8
        //
        // skin aval = 5 
        if ((GameManager.Instance.skinAvailability & 1 << index) == 1 << index) // have skin
        {
            float x = ((int)index % 4) * .25f; // 4 = number of rows
            float y = .75f - (((int)index / 4) * .25f); // 4 = number of col

            playerMaterial.SetTextureOffset("_MainTex", new Vector2(x, y));
            GameManager.Instance.currentSkinIndex = index;
            GameManager.Instance.Save();
        }
        else // do not have skin
        {
            int cost = 150;

            if (GameManager.Instance.currency >= cost)
            {
                GameManager.Instance.currency -= cost;
                GameManager.Instance.skinAvailability += 1 << index;
                GameManager.Instance.Save();

                currencyText.text = "Currency: " + GameManager.Instance.currency.ToString();
                shopItemContainer.transform.GetChild(index).GetChild(0).gameObject.SetActive(false);
                changePlayerSkin(index);
            }
        }
    }

}
