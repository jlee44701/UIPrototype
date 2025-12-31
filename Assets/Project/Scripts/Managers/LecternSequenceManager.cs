// using System;
// using UnityEngine;
//
// namespace RuntimeUI {
//     public class LecternSequenceManager : MonoBehaviour {
//         
//         [Header("Preload (Splash Screen)")]
//         [Tooltip("Prefab assets that load first. These can include level management Prefabs or textures, sounds, etc.")]
//         [SerializeField] GameObject[] m_PreloadedAssets;
//         
//         // [Tooltip("Time in seconds to show Splash Screen")]
//         // [SerializeField] float m_LoadScreenTime = 5f;
//         [Space(10)]
//         [Tooltip("Debug state changes in the console")]
//         [SerializeField] bool m_Debug;
//         
//         readonly StateMachine m_StateMachine = new StateMachine();
//
//         IState
//             m_SplashScreenState,
//             m_DrillScreenState;
//         
//         
//         public IState CurrentState => m_StateMachine.CurrentState;
//         #region MonoBehaviour event messages
//         void Start() {
//             // Set this MonoBehaviour to control the coroutines - unused in this demo
//             Coroutines.Initialize(this);
//
//             // Checks for required fields in the Inspector
//             NullRefChecker.Validate(this);
//
//             // Instantiates any assets needed before gameplay
//             InstantiatePreloadedAssets();
//
//             // Sets up States and transitions, runs initial State
//             Initialize();
//         }
//         void OnEnable() {
//             
//         }
//         void OnDisable() {
//             
//         }
//         // private void OnEnable()
//         // {
//         //     SceneEvents.ExitApplication += SceneEvents_ExitApplication;
//         // }
//         //
//         // // Unsubscribe from event channels to prevent errors
//         // private void OnDisable()
//         // {
//         //     SceneEvents.ExitApplication -= SceneEvents_ExitApplication;
//         // }
//         #endregion
//         
//         #region Methods
//
//         public void Initialize()
//         {
//             // Define the Game States
//             SetStates();
//             AddLinks();
//             
//             m_StateMachine.Run(m_DrillScreenState);
//             UIEvents.DustProphet.DrillingViewShown?.Invoke();
//         }
//         
//         private void SetStates()
//         {
//             // Create States for the game. Pass in an Action to execute or null to do nothing
//
//             // Optional names added for debugging
//             // Executes GameEvents.LoadProgressUpdated every frame and GameEvents.PreloadCompleted on exit
//             // m_SplashScreenState = new DelayState(m_LoadScreenTime, SceneEvents.LoadProgressUpdated,
//             //     SceneEvents.PreloadCompleted, "LoadScreenState");
//
//             m_DrillScreenState = new State(null, "DrillScreenState", m_Debug);
//         }
//         
//         // Define links between the states
//         private void AddLinks()
//         {
//
//             // Transition automatically to the StartScreen once the loading time completes
//             m_SplashScreenState.AddLink(new Link(m_DrillScreenState));
//
//             // EventLinks listen for the UI/game event messages to activate the transition to the next state
//
//             // This implementation uses a wrapper around the event to make easier to register/unregister the EventLinks
//
//             
//             
//             ActionWrapper settingsShownWrapper = new ActionWrapper
//             {
//                 Subscribe = handler => UIEvents.App.SettingsShown += handler,
//                 Unsubscribe = handler => UIEvents.App.SettingsShown -= handler
//             };
//
//             ActionWrapper screenClosedWrapper = new ActionWrapper
//             {
//                 Subscribe = handler => UIEvents.App.ScreenClosed += handler,
//                 Unsubscribe = handler => UIEvents.App.ScreenClosed -= handler
//             };
//
//             ActionWrapper gameStartedWrapper = new ActionWrapper
//             {
//                 Subscribe = handler => GameEvents.GameStarted += handler,
//                 Unsubscribe = handler => GameEvents.GameStarted -= handler
//             };
//   
//             ActionWrapper pauseScreenShownWrapper = new ActionWrapper
//             {
//                 Subscribe = handler => UIEvents.App.PauseScreenShown += handler,
//                 Unsubscribe = handler => UIEvents.App.PauseScreenShown -= handler
//             };
//   
//             ActionWrapper gameWonWrapper = new ActionWrapper
//             {
//                 Subscribe = handler => GameEvents.GameWon += handler,
//                 Unsubscribe = handler => GameEvents.GameWon -= handler
//             };
//
//             ActionWrapper gameLostWrapper = new ActionWrapper
//             {
//                 Subscribe = handler => GameEvents.GameLost += handler,
//                 Unsubscribe = handler => GameEvents.GameLost -= handler
//             };
//
//             // Once you have wrappers defined around the events, set up the EventLinks
//   
//             m_StartScreenState.AddLink(new EventLink(mainMenuShownWrapper, m_MainMenuState));
//
//             m_MainMenuState.AddLink(new EventLink(levelSelectionShownWrapper, m_LevelSelectionState));
//             m_MainMenuState.AddLink(new EventLink(settingsShownWrapper, m_MenuSettingsState));
//
//             m_MenuSettingsState.AddLink(new EventLink(screenClosedWrapper, m_MainMenuState));
//
//             m_LevelSelectionState.AddLink(new EventLink(screenClosedWrapper, m_MainMenuState));
//             m_LevelSelectionState.AddLink(new EventLink(gameStartedWrapper, m_GamePlayState));
//
//             m_GamePlayState.AddLink(new EventLink(pauseScreenShownWrapper, m_PauseState));
//             m_GamePlayState.AddLink(new EventLink(settingsShownWrapper, m_GameSettingsState));
//             m_GamePlayState.AddLink(new EventLink(gameWonWrapper, m_GameWinState));
//             m_GamePlayState.AddLink(new EventLink(gameLostWrapper, m_GameLoseState));
//
//             m_GameSettingsState.AddLink(new EventLink(screenClosedWrapper, m_GamePlayState));
//
//             m_PauseState.AddLink(new EventLink(screenClosedWrapper, m_GamePlayState));
//             m_PauseState.AddLink(new EventLink(mainMenuShownWrapper, m_MainMenuState));
//
//             m_GameWinState.AddLink(new EventLink(mainMenuShownWrapper, m_MainMenuState));
//
//             m_GameLoseState.AddLink(new EventLink(mainMenuShownWrapper, m_MainMenuState));
//             m_GameLoseState.AddLink(new EventLink(gameStartedWrapper, m_GamePlayState));
//         }
//         void InstantiatePreloadedAssets()
//         {
//             foreach (var asset in m_PreloadedAssets)
//             {
//                 if (asset)
//                     Instantiate(asset);
//             }
//         }
//         #endregion
//     }
// }
