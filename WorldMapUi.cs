using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapUi : MonoBehaviour
{
    RectTransform[] worldMapLocatorRectTransforms;
    Dictionary<SceneGenerator.Maps, RectTransform> mapWorldLocatorDictionary = new Dictionary<SceneGenerator.Maps, RectTransform>();
    RectTransform playerLocatorRectTransform;

    private void Awake()
    {
        worldMapLocatorRectTransforms = new RectTransform[Enum.GetValues(typeof(SceneGenerator.Maps)).Length];

        worldMapLocatorRectTransforms[0] = transform.Find("WorldMapMapOneLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[1] = transform.Find("WorldMapMapTwoLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[2] = transform.Find("WorldMapMapThreeLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[3] = transform.Find("WorldMapMapFourLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[4] = transform.Find("WorldMapMapFiveLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[5] = transform.Find("WorldMapMapSixLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[6] = transform.Find("WorldMapMapSevenLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[7] = transform.Find("WorldMapMapEightLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[8] = transform.Find("WorldMapMapNineLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[9] = transform.Find("WorldMapMapTenLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[10] = transform.Find("WorldMapMapElevenLocation").GetComponent<RectTransform>();
        worldMapLocatorRectTransforms[11] = transform.Find("WorldMapMapTwelveLocation").GetComponent<RectTransform>();

        playerLocatorRectTransform = transform.Find("PlayerLocator(Panel)").GetComponent<RectTransform>();

        int i = 0;
        foreach (SceneGenerator.Maps map in Enum.GetValues(typeof(SceneGenerator.Maps)))
        {
            mapWorldLocatorDictionary[map] = worldMapLocatorRectTransforms[i];
            i++;
        }

        PlayerTeleporter.OnTeleport += PlayerTeleporter_OnTeleport;
    }

    private void PlayerTeleporter_OnTeleport(object sender, PlayerTeleporter.TeleportArgs e) => NextFrame.Create(() => UpdatePlayerWorldMapPosition()); // gotta use next frame because this needs to fire after player position has been updated in plyaerStathandler class

    private void OnEnable() => UpdatePlayerWorldMapPosition();

    private void OnDestroy() => PlayerTeleporter.OnTeleport -= PlayerTeleporter_OnTeleport; // static

    void UpdatePlayerWorldMapPosition() => playerLocatorRectTransform.anchoredPosition = mapWorldLocatorDictionary[PlayerStatHandler.playerMapLocation].anchoredPosition;

}
