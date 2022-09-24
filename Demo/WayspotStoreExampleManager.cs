// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Niantic.ARDK;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Extensions;
using Niantic.ARDK.LocationService;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using Niantic.ARDKExamples.WayspotAnchors;
using UnityEngine;
using UnityEngine.UI;
using VPSTour.lib;
using VPSTour.lib.WayspotAnchorStore;

namespace VPSTour.Demo {
    /// <summary>
    /// This example is a carbon copy of the <see cref="WayspotAnchorExampleManager"/>, except
    /// we add waypoint persistence via <see cref="IPayloadStore"/>.
    ///
    /// Look for //------- Store Logic -----------/ blocks for changes
    /// </summary>
    public class WayspotStoreExampleManager : MonoBehaviour {
        [Tooltip("The anchor that will be placed")] [SerializeField]
        private GameObject _anchorPrefab;

        [Tooltip("Camera used to place the anchors via raycasting")] [SerializeField]
        private Camera _camera;

        [Tooltip("Text used to display the current status of the demo")] [SerializeField]
        private Text _statusLog;

        [Tooltip("Text used to show the current localization state")] [SerializeField]
        private Text _localizationStatus;

        public WayspotAnchorService WayspotAnchorService;
        private IARSession _arSession;

        private readonly HashSet<WayspotAnchorTracker> _wayspotAnchorTrackers =
            new HashSet<WayspotAnchorTracker>();

        private IWayspotAnchorsConfiguration _config;

        private void Awake() {
            // This is necessary for setting the user id associated with the current user.
            // We strongly recommend generating and using User IDs. Accurate user information allows
            //  Niantic to support you in maintaining data privacy best practices and allows you to
            //  understand usage patterns of features among your users.
            // ARDK has no strict format or length requirements for User IDs, although the User ID string
            //  must be a UTF8 string. We recommend avoiding using an ID that maps back directly to the
            //  user. So, for example, don’t use email addresses, or login IDs. Instead, you should
            //  generate a unique ID for each user. We recommend generating a GUID.
            // When the user logs out, clear ARDK's user id with ArdkGlobalConfig.ClearUserIdOnLogout

            //  Sample code:
            //  // GetCurrentUserId() is your code that gets a user ID string from your login service
            //  var userId = GetCurrentUserId();
            //  ArdkGlobalConfig.SetUserIdOnLogin(userId);

            _statusLog.text = "Initializing Session.";
        }

        private void OnEnable() {
            ARSessionFactory.SessionInitialized += HandleSessionInitialized;
        }

        private void OnDisable() {
            ARSessionFactory.SessionInitialized -= HandleSessionInitialized;
        }

        private void OnDestroy() {
            if (WayspotAnchorService != null) {
                WayspotAnchorService.LocalizationStateUpdated -= LocalizationStateUpdated;
                WayspotAnchorService.Dispose();
            }
        }

        private void Update() {
            if (WayspotAnchorService == null) return;

            // Do hit test from where player taps on screen
            var touchSuccess = TryGetTouchInput(out Matrix4x4 localPose);

            if (touchSuccess) {
                if (WayspotAnchorService.LocalizationState == LocalizationState.Localized)
                    PlaceAnchor(localPose); //Create the Wayspot Anchor and place the GameObject
                else
                    _statusLog.text = "Must localize before placing anchor.";
            }
        }

        /// Saves all of the existing wayspot anchors
        public async void SaveWayspotAnchors() {
            _statusLog.text = $"Saving {_wayspotAnchorTrackers.Count} Wayspot Anchors.";
            if (_wayspotAnchorTrackers.Count > 0) {
                var wayspotAnchors = WayspotAnchorService.GetAllWayspotAnchors();

                // Only anchors that have successfully resolved can be saved
                var saveableAnchors =
                    wayspotAnchors.Where(a => a.Status == WayspotAnchorStatusCode.Limited ||
                                              a.Status == WayspotAnchorStatusCode.Success);
                
                //---------------------- Store Logic ----------------------//
                // Commented code from original sample that saves payloads locally
                // var payloads = saveableAnchors.Select(a => a.Payload);
                //
                // WayspotAnchorDataUtility.SaveLocalPayloads(payloads.ToArray());
                
                // This sample always overrides persisted anchors with whatever anchors
                // exist in the scene, so clear the whole store before we save the local
                // anchors to it.
                _statusLog.text = "Clearing Store";
                await Providers.ProvidePayloadStore().Clear();
                _statusLog.text = "Saving anchors";
                await Providers.ProvidePayloadStore().Persist(saveableAnchors.ToArray());
            } else {
                //---------------------- Store Logic ----------------------//
                // Commented code from original sample that saves payloads locally
                // WayspotAnchorDataUtility.SaveLocalPayloads(Array.Empty<WayspotAnchorPayload>());
                await Providers.ProvidePayloadStore().Clear();
            }

            _statusLog.text = $"Saved {_wayspotAnchorTrackers.Count} Wayspot Anchors.";
        }

        /// Loads all of the saved wayspot anchors
        public async void LoadWayspotAnchors() {
            //---------------------- Store Logic ----------------------//
            // Commented code from original sample that saves payloads locally
            // var payloads = WayspotAnchorDataUtility.LoadLocalPayloads();
            Debug.Log("Restoring Wayspot Anchors...");
            _statusLog.text = "Restoring Anchors...";
            var payloads = await Providers.ProvidePayloadStore().Restore();
            
            if (payloads.Length > 0) {
                foreach (var payload in payloads) {
                    var anchors = WayspotAnchorService.RestoreWayspotAnchors(payload);
                    if (anchors.Length == 0) return; // error raised in CreateWayspotAnchors

                    CreateWayspotAnchorGameObject(anchors[0], Vector3.zero, Quaternion.identity,
                                                  false);
                }

                _statusLog.text = $"Loaded {_wayspotAnchorTrackers.Count} anchors.";
            } else {
                _statusLog.text = "No anchors to load.";
            }
            Debug.Log("Restored");
        }

        /// Clears all of the active wayspot anchors
        public void ClearAnchorGameObjects() {
            if (_wayspotAnchorTrackers.Count == 0) {
                _statusLog.text = "No anchors to clear.";
                return;
            }

            foreach (var anchor in _wayspotAnchorTrackers) Destroy(anchor.gameObject);

            var wayspotAnchors = _wayspotAnchorTrackers.Select(go => go.WayspotAnchor).ToArray();
            WayspotAnchorService.DestroyWayspotAnchors(wayspotAnchors);

            _wayspotAnchorTrackers.Clear();
            _statusLog.text = "Cleared Wayspot Anchors.";
        }

        public void PauseARSession() {
            if (_arSession.State == ARSessionState.Running) {
                _arSession.Pause();
                _statusLog.text = $"AR Session Paused.";
            } else {
                _statusLog.text = $"Cannot pause AR Session.";
            }
        }

        public void ResumeARSession() {
            if (_arSession.State == ARSessionState.Paused) {
                _arSession.Run(_arSession.Configuration);
                _statusLog.text = $"AR Session Resumed.";
            } else {
                _statusLog.text = $"Cannot resume AR Session.";
            }
        }

        public void RestartWayspotAnchorService() {
            WayspotAnchorService.Restart();
        }

        private void HandleSessionInitialized(AnyARSessionInitializedArgs args) {
            _statusLog.text = "Session initialized";
            _arSession = args.Session;
            _arSession.Ran += HandleSessionRan;
        }

        private void HandleSessionRan(ARSessionRanArgs args) {
            _arSession.Ran -= HandleSessionRan;
            WayspotAnchorService = CreateWayspotAnchorService();
            WayspotAnchorService.LocalizationStateUpdated += OnLocalizationStateUpdated;
            _statusLog.text = "Session running";
        }

        private void OnLocalizationStateUpdated(LocalizationStateUpdatedArgs args) {
            _localizationStatus.text = "Localization status: " + args.State;
        }

        private WayspotAnchorService CreateWayspotAnchorService() {
            var locationService = LocationServiceFactory.Create(_arSession.RuntimeEnvironment);
            locationService.Start();

            if (_config == null) _config = WayspotAnchorsConfigurationFactory.Create();

            var wayspotAnchorService =
                new WayspotAnchorService
                    (
                     _arSession,
                     locationService,
                     _config
                    );

            wayspotAnchorService.LocalizationStateUpdated += LocalizationStateUpdated;

            return wayspotAnchorService;
        }

        private void LocalizationStateUpdated(LocalizationStateUpdatedArgs args) {
            _localizationStatus.text = args.State.ToString();
        }

        private void PlaceAnchor(Matrix4x4 localPose) {
            var anchors = WayspotAnchorService.CreateWayspotAnchors(localPose);
            if (anchors.Length == 0) return; // error raised in CreateWayspotAnchors

            var position = localPose.ToPosition();
            var rotation = localPose.ToRotation();
            CreateWayspotAnchorGameObject(anchors[0], position, rotation, true);

            _statusLog.text = "Anchor placed.";
        }

        private GameObject CreateWayspotAnchorGameObject(IWayspotAnchor anchor,
                                                         Vector3 position,
                                                         Quaternion rotation,
                                                         bool startActive) {
            var go = Instantiate(_anchorPrefab, position, rotation);

            var tracker = go.GetComponent<WayspotAnchorTracker>();
            if (tracker == null) {
                Debug.Log("Anchor prefab was missing WayspotAnchorTracker, so one will be added.");
                tracker = go.AddComponent<WayspotAnchorTracker>();
            }

            tracker.gameObject.SetActive(startActive);
            tracker.AttachAnchor(anchor);
            _wayspotAnchorTrackers.Add(tracker);

            return go;
        }

        private bool TryGetTouchInput(out Matrix4x4 localPose) {
            if (_arSession == null || PlatformAgnosticInput.touchCount <= 0) {
                localPose = Matrix4x4.zero;
                return false;
            }

            var touch = PlatformAgnosticInput.GetTouch(0);
            if (touch.IsTouchOverUIObject()) {
                localPose = Matrix4x4.zero;
                return false;
            }

            if (touch.phase != TouchPhase.Began) {
                localPose = Matrix4x4.zero;
                return false;
            }

            var currentFrame = _arSession.CurrentFrame;
            if (currentFrame == null) {
                localPose = Matrix4x4.zero;
                return false;
            }

            if (_arSession.RuntimeEnvironment == RuntimeEnvironment.Playback) {
                // Playback doesn't support plane detection yet, so instead of hit testing against planes,
                // just place the anchor in front of the camera.
                localPose =
                    Matrix4x4.TRS
                        (
                         _camera.transform.position + (_camera.transform.forward * 2),
                         Quaternion.identity,
                         Vector3.one
                        );
            } else {
                var results = currentFrame.HitTest
                    (
                     _camera.pixelWidth,
                     _camera.pixelHeight,
                     touch.position,
                     ARHitTestResultType.ExistingPlane
                    );

                int count = results.Count;
                if (count <= 0) {
                    localPose = Matrix4x4.zero;
                    return false;
                }

                var result = results[0];
                localPose = result.WorldTransform;
            }

            return true;
        }

        public void SetConfig(IWayspotAnchorsConfiguration config) {
            _config = config;
        }
    }
}