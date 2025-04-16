using System;
using System.Collections.Generic;
using _Project.Scripts.Network;
using _Project.Scripts.Shared;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.ChoosingTeamPanel
{
    public class TeamView : MonoBehaviour
    {
        [SerializeField] private Transform contentParent;
        [SerializeField] private PlayerTeamView playerTeamViewPrefab;
        [SerializeField] private List<PlayerTeamView> playerTeamViews;
        [SerializeField] private Button chooseTeamButton;
        [SerializeField] private int index;

        private void Awake()
        {
            chooseTeamButton.onClick.AddListener(ChooseTeamCallBack);
        }
        

        public void Init(List<PlayerData> players, string localPlayerId)
        {
            for (var i = playerTeamViews.Count; i < players.Count; i++)
            {
                var newView = Instantiate(playerTeamViewPrefab, contentParent);
                playerTeamViews.Add(newView);
            }
            
            for (var i = 0; i < players.Count; i++)
            {
                playerTeamViews[i].gameObject.SetActive(true);
                playerTeamViews[i].Init(players[i].Name, localPlayerId == players[i].ID);
            }
            
            for (var i = players.Count; i < playerTeamViews.Count; i++)
            {
                playerTeamViews[i].gameObject.SetActive(false);
            }
        }
        
        private void ChooseTeamCallBack()
        {
            TeamSelectionSync.Instance.ChooseTeamServerRpc(new PlayerNetworkData(index, AuthenticationService.Instance.PlayerName), index);
        }
    }
}
