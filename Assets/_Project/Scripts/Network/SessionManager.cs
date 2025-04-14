using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Shared.Sessions.Data;
using _Project.Scripts.Shared.Sessions.Events;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Network
{
    public class SessionManager : MonoBehaviour
    {
        private ISession _activeSession;

        public ISession ActiveSession {
            get => _activeSession;
            set {
                _activeSession = value;
                Debug.Log($"Active session: {_activeSession}");
            }
        }
        
        [Header("Events")]
        [SerializeField] private SessionCreateRequestedEvent createRequestedEvent;
        [SerializeField] private SessionJoinRequestedEvent joinRequestedEvent;
        [SerializeField] private SessionRefreshRequestedEvent refreshRequestedEvent;
        [SerializeField] private SessionListUpdatedEvent sessionListUpdatedEvent;

        private const string PlayerNamePropertyKey = "playerName";

        private void OnEnable()
        {
            createRequestedEvent.Register(HandleCreateSession);
            joinRequestedEvent.Register(HandleJoinSession);
            refreshRequestedEvent.Register(HandleRefreshSessions);
        }

        private void OnDisable()
        {
            createRequestedEvent.Unregister(HandleCreateSession);
            joinRequestedEvent.Unregister(HandleJoinSession);
            refreshRequestedEvent.Unregister(HandleRefreshSessions);
        }

        private void HandleCreateSession(string sessionName)
        {
            StartSessionAsHost(sessionName).GetAwaiter();
        }
        
        private void HandleJoinSession(string sessionId)
        {
            JoinSessionById(sessionId).GetAwaiter();
        }
        
        private void HandleRefreshSessions()
        {
            QuerySessions().GetAwaiter();
        }


        private async void Start()
        {
            try
            {
                await UnityServices.InitializeAsync(); // Initialize Unity Gaming Services SDKs.
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Anonymously authenticate the player
                Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        private async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties() {
            // Custom game-specific properties that apply to an individual player, ie: name, role, skill level, etc.
            var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
            return new Dictionary<string, PlayerProperty> { { PlayerNamePropertyKey, playerNameProperty } };
        }

        private async Task StartSessionAsHost(string sessionName) {
            var playerProperties = await GetPlayerProperties(); 
        
            var options = new SessionOptions {
                MaxPlayers = 4,
                IsLocked = false,
                IsPrivate = false,
                PlayerProperties = playerProperties,
                Name = sessionName
            }.WithRelayNetwork();
        
            ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);

            Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");
        }

        private async Task JoinSessionById(string sessionId) {
            ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
            Debug.Log($"Session {ActiveSession.Id} joined!");
        }
        
        
        private async Task <IList<ISessionInfo>> QuerySessions() {
            var sessionQueryOptions = new QuerySessionsOptions();
            var results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
            CreateSessionsDataAndNotify(results.Sessions);
            return results.Sessions;
        }

        private void CreateSessionsDataAndNotify(IList<ISessionInfo> sessions)
        {
            var sessionsData = new List<SessionData>();
            foreach (var sessionInfo in sessions)
            {
                var sessionData = new SessionData(sessionInfo.Id, sessionInfo.Name,
                    sessionInfo.MaxPlayers - sessionInfo.AvailableSlots, sessionInfo.IsLocked, sessionInfo.MaxPlayers);
                sessionsData.Add(sessionData);
            }
            sessionListUpdatedEvent?.Raise(sessionsData);
        }

        private async Task LeaveSession() {
            if (ActiveSession != null) {
                try {
                    await ActiveSession.LeaveAsync();
                }
                catch {
                    // Ignored as we are exiting the game
                }
                finally {
                    ActiveSession = null;
                }
            }
        }
        
    }
}
