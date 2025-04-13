using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public GameObject[] charactersInScene;
    public GameObject[] characterPrefabs;
    public int characterIndex;
    string CharacterName;
    public GameObject CharacterHolder;
    public TextMeshProUGUI characterName;
    public Button left, right;
    public Image characterImage;
    public Sprite[] characterImages;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Now we try to find the characters only if we are in a game level scene
        if (scene.name == "GrandMarsyon" || scene.name == "TheIce" || scene.name == "TheForest")
        {
            FindAndActivateCharacter();
        }
    }

    private void FindAndActivateCharacter()
    {
        // Find all the characters in the new scene with the tag "PlayerCharacter"
        GameObject[] foundCharacters = GameObject.FindGameObjectsWithTag("PlayerCharacter");
        print(characterIndex);
        charactersInScene = foundCharacters;
        // Assuming the characters are in the same order as the characterImages,
        // we deactivate all and then activate the selected one
        foreach (GameObject character in charactersInScene)
        {
            character.SetActive(false);
        }

        // Activate the selected character
        if (foundCharacters.Length > characterIndex && characterIndex >= 0)
        {
            charactersInScene[characterIndex].SetActive(true);
        }   
        else
        {
            Debug.LogWarning("The selected character index is out of range or no characters found.");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        characterImage.sprite = characterImages[0];
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            characterName.text = characterPrefabs[0].name;
        }
        else
        {
            characterName.text = charactersInScene[0].name;
        }

    }

    public void OnClickLeft()
    {
        if (characterIndex <= 0)
        {
            characterIndex = characterImages.Length - 1;

        }
        else
        {
            characterIndex--;
        }
            characterImage.sprite = characterImages[characterIndex];
            CharacterName = characterPrefabs[characterIndex].name;
            characterName.text = CharacterName;
    }
    public void OnClickRight()
    {
        if (characterIndex >= characterImages.Length - 1)
        {
            characterIndex = 0;
        }
        else
        {
            characterIndex++;
        }
        characterImage.sprite = characterImages[characterIndex];
        CharacterName = characterPrefabs[characterIndex].name;
        characterName.text = CharacterName;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
