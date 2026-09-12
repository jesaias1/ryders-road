using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Bootstrap;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.Gameplay.Camera;
using Avoidance.Gameplay.Blocks;
using Avoidance.Gameplay.Visuals;
using Avoidance.Input;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class BootstrapPlayModeTests
    {
        [UnityTest]
        public IEnumerator Bootstrap_LoadsModuleSelectorScene()
        {
            yield return LoadBootstrapFresh("ModuleSelector");

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("ModuleSelector"));
            var bootstrap = Object.FindAnyObjectByType<GameBootstrap>();
            Assert.That(bootstrap, Is.Not.Null);
            Assert.That(bootstrap.IsInitialized, Is.True);
            Assert.That(FindObjectNamed("Selector Sky Haze"), Is.False);
            Assert.That(FindObjectNamed("Selector R Beacon"), Is.False);
            Assert.That(FindObjectNamed("Selector Hero Pillar Left"), Is.False);
            Assert.That(FindObjectNamed("Selector Hero Pillar Right"), Is.False);
        }

        [UnityTest]
        public IEnumerator ModuleRunner_LoadsSelectedPhase2Modules()
        {
            foreach (var moduleId in new[]
                     {
                         "module.001.first-steps",
                         "module.002.moving-parts",
                         "module.003.flow-error",
                         ModuleSelectionState.SpiralModuleId
                     })
            {
                ModuleSelectionState.Select(moduleId);
                yield return LoadScene("ModuleRunner");
                yield return WaitForPlayer();

                var controller = Object.FindAnyObjectByType<ModuleSceneController>();
                Assert.That(controller, Is.Not.Null);
                Assert.That(controller.ActiveModule.StableModuleId, Is.EqualTo(moduleId));
                var motor = Object.FindAnyObjectByType<ParkourMotor>();
                Assert.That(motor, Is.Not.Null);
                var cameraRig = motor.GetComponentInChildren<FirstPersonCameraRig>();
                var hands = motor.GetComponentInChildren<FirstPersonHands>();
                Assert.That(cameraRig, Is.Not.Null, moduleId);
                cameraRig.ResetView(motor.transform.rotation);
                Assert.That(cameraRig.Pitch, Is.EqualTo(9f).Within(0.05f), moduleId);
                Assert.That(cameraRig.CurrentFieldOfView, Is.EqualTo(94f).Within(0.1f), moduleId);
                Assert.That(motor.GetComponent<PlayerGroundingCue>(), Is.Null, moduleId);
                Assert.That(hands, Is.Not.Null, moduleId);
                Assert.That(hands.ArmsVisible, Is.False, moduleId);
                Assert.That(FindObjectNamed(PlayerGroundingCue.ShadowObjectName), Is.False, moduleId);
                Assert.That(FindObjectNamed("Collider Truth Readability Shell"), Is.False, moduleId);

                var authoredBlock = controller.ActiveModule.Blocks.First();
                var startSupport = FindObjectByName(authoredBlock.StableId);
                Assert.That(startSupport, Is.Not.Null, moduleId);
                Assert.That(startSupport.GetComponent<BoxCollider>(), Is.Not.Null, moduleId);
                Assert.That(startSupport.transform.localScale, Is.EqualTo(authoredBlock.Size), moduleId);

                cameraRig.ApplyLook(
                    new CameraTestInput(new Vector2(0f, 48f)),
                    PlayerInputMode.Touch,
                    0.016f);
                Assert.That(cameraRig.Pitch, Is.LessThan(9f), moduleId);
            }
        }

        [UnityTest]
        public IEnumerator MovementLab_RemainsLoadable()
        {
            yield return LoadMovementLabFresh();

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MovementLab"));
            Assert.That(Object.FindAnyObjectByType<ParkourMotor>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator NullSpace_RestoresPlayerAtLatestRestorePoint()
        {
            yield return LoadMovementLabFresh();

            var player = Object.FindAnyObjectByType<ParkourMotor>();
            Assert.That(player, Is.Not.Null);
            player.transform.position = new Vector3(0f, -20f, 0f);
            ParkourMotor restoredPlayer = null;
            var timeout = Time.realtimeSinceStartup + 2f;
            while (Time.realtimeSinceStartup < timeout)
            {
                restoredPlayer = Object.FindAnyObjectByType<ParkourMotor>();
                if (restoredPlayer != null
                    && restoredPlayer.transform.position.y > 0f
                    && restoredPlayer.HorizontalSpeed < 0.001f)
                {
                    break;
                }

                yield return null;
            }

            Assert.That(restoredPlayer, Is.Not.Null);
            Assert.That(restoredPlayer.transform.position.y, Is.GreaterThan(0f));
            Assert.That(restoredPlayer.HorizontalSpeed, Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator Module003_FullBronzeRouteUsesSolidTruthfulSupportsAndNoWorldFloor()
        {
            ModuleSelectionState.Select("module.003.flow-error");
            yield return LoadScene("ModuleRunner");
            yield return WaitForPlayer();
            yield return null;

            var requiredSupports = new[]
            {
                "m03.start",
                "m03.opening.01",
                "m03.opening.02",
                "m03.opening.03",
                "m03.arrival.approach",
                "m03.arrival.court",
                "m03.sanctuary.bridge",
                "m03.crossing.approach",
                "m03.moving-setup",
                "moving.m03.motion-gallery",
                "m03.motion-exit",

                "m03.temple.entry",
                "crumble.m03.01",
                "crumble.m03.02",
                "crumble.m03.03",
                "m03.temple.exit",

                "m03.spine.approach",
                "m03.boost-runup",
                "boost.m03.first",
                "m03.boost-landing",
                "m03.final.01",
                "m03.final.02",
                "m03.final.03",
                "m03.final.04",
                "m03.final.05",
                "m03.final.06",
                "m03.final.06"
            };

            foreach (var supportName in requiredSupports)
            {
                var support = FindObjectByName(supportName);
                Assert.That(support, Is.Not.Null, supportName);
                var collider = support.GetComponent<Collider>();
                Assert.That(collider, Is.Not.Null, supportName);
                Assert.That(collider.isTrigger, Is.False, supportName);
                Assert.That(
                    support.GetComponentsInChildren<Renderer>(true).Any(renderer => renderer.enabled),
                    Is.True,
                    supportName);
            }

            Assert.That(FindObjectNamed("water.m03.lower-flow"), Is.False);
            Assert.That(FindObjectNamed("biome.ancient-abyss.mist.ocean-main"), Is.False);
            Assert.That(
                Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Any(filter =>
                    filter.sharedMesh != null
                    && filter.sharedMesh.name == "Mesh_RR_RadialMistDisc"),
                Is.False);
            Assert.That(FindObjectNamed("biome.ancient-abyss.colossal.abyss-tower"), Is.False);
            Assert.That(FindObjectNamed("ancient-abyss.mist-field.lower-city"), Is.False);
            Assert.That(FindObjectNamed("ancient-abyss.mist-field.hero-depth"), Is.False);
            Assert.That(FindObjectNamed("ancient-abyss.dust.route-low"), Is.False);
            Assert.That(FindObjectNamed("ancient-abyss.dust.summit"), Is.False);
            Assert.That(Object.FindAnyObjectByType<PlayerGroundingCue>(), Is.Null);
            Assert.That(Object.FindObjectsByType<CrumblingBlock>(FindObjectsSortMode.None).Length, Is.EqualTo(3));

            Assert.That(FindObjectNamed("moving.m03.motion-gallery.ancient-bridge-rail"), Is.False);

            foreach (var placeAnchor in new[]
                     {
                         "M03_Arrival_Main_Foundation",
                         "M03_Crossing_Near_Foundation",
                         "M03_Temple_Main_Foundation",
                         "M03_Spine_Launch_Foundation",
                         "M03_Sanctum_Crown_Foundation"
                     })
            {
                var place = FindObjectByName(placeAnchor);
                Assert.That(place, Is.Not.Null, placeAnchor);
                Assert.That(place.GetComponentsInChildren<Avoidance.Gameplay.Worlds.AuthoredSurface>().All(surface => surface.HasMatchingCollision), Is.True, placeAnchor);
            }

            var vfx = Object.FindAnyObjectByType<GameplayVfxService>();
            Assert.That(vfx, Is.Not.Null);
            Assert.That(vfx.PoolSize, Is.InRange(4, 8));

            foreach (var supportName in requiredSupports.Where(name =>
                         !name.StartsWith("boost.", System.StringComparison.Ordinal)
                         && !name.StartsWith("crumble.", System.StringComparison.Ordinal)))
            {
                var support = FindObjectByName(supportName);
                Assert.That(support.GetComponent<RuntimeVisualPulse>(), Is.Null, supportName);
                Assert.That(support.GetComponentsInChildren<ParticleSystem>(true), Is.Empty, supportName);
            }
        }

        [UnityTest]
        public IEnumerator Module003_CapturesFiveProductionGameplayViews()
        {
            ModuleSelectionState.Select("module.003.flow-error");
            yield return LoadScene("ModuleRunner");
            yield return WaitForPlayer();
            yield return null;

            var camera = Camera.main;
            Assert.That(camera, Is.Not.Null);
            var cameraRig = camera.GetComponent<FirstPersonCameraRig>();
            var cameraRigWasEnabled = cameraRig != null && cameraRig.enabled;
            var previousFieldOfView = camera.fieldOfView;
            if (cameraRig != null)
            {
                cameraRig.enabled = false;
            }

            try
            {
                var outputDirectory = Path.GetFullPath("Logs/Phase091VisualQA");
                Directory.CreateDirectory(outputDirectory);
                var views = new[]
                {
                    new ProductionView("01-arrival-sanctuary", new Vector3(0f, 1.7f, -7f), new Vector3(1f, 2f, 15f)),
                    new ProductionView("02-broken-crossing", new Vector3(7f, 3.45f, 30f), new Vector3(-5f, 3.4f, 43f)),
                    new ProductionView("03-collapsed-temple", new Vector3(-5f, 4.7f, 49f), new Vector3(4f, 5.2f, 67f)),
                    new ProductionView("04-energy-spine", new Vector3(2f, 7.1f, 76f), new Vector3(-1f, 10f, 96f)),
                    new ProductionView("05-patch-sanctum", new Vector3(-4f, 12.2f, 112f), new Vector3(8f, 15f, 129f))
                };

                foreach (var view in views)
                {
                    var path = Path.Combine(outputDirectory, view.Name + ".png");
                    Assert.That(RenderProductionView(camera, view.Position, view.Target, path), Is.True, view.Name);
                    Assert.That(new FileInfo(path).Length, Is.GreaterThan(25000), view.Name);
                    yield return null;
                }
            }
            finally
            {
                camera.targetTexture = null;
                camera.fieldOfView = previousFieldOfView;
                if (cameraRig != null)
                {
                    cameraRig.enabled = cameraRigWasEnabled;
                }
            }
        }

        [UnityTest]
        public IEnumerator Module003_BoostTriggerLandsRealMotorOnBronzeDestination() => VerifyBoost(false);

        [UnityTest]
        public IEnumerator FoundationBoostTriggerLandsRealMotorOnBronzeDestination() => VerifyBoost(true);

        private IEnumerator VerifyBoost(bool foundation)
        {
            if(foundation) CampaignFlowTrial.Launch("module.003.flow-error",CampaignTrialMode.Foundation);
            else ModuleSelectionState.Select("module.003.flow-error");
            yield return LoadScene("ModuleRunner");
            yield return WaitForPlayer();
            var motor = Object.FindAnyObjectByType<ParkourMotor>();
            motor.GetComponent<PlayerRuntimeCoordinator>().enabled = false;
            var module = Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
            var definition = module.BoostBlocks.Single();
            var boost = Object.FindAnyObjectByType<JumpBoostBlock>();
            var landing = GameObject.Find("m03.boost-landing").GetComponent<BoxCollider>();
            var direction = Vector3.ProjectOnPlane(definition.LaunchDirection, Vector3.up).normalized;
            foreach (var approachSpeed in new[] { 3f, 5f, motor.Profile.BaseRunSpeed })
            {
                // Re-enter the trigger through the physics engine, never invoke TryLaunch here.
                motor.ResetMotion(new Vector3(0f, 50f, -30f), Quaternion.identity, 0f);
                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();
                boost.ResetForModule();
                motor.ResetMotion(definition.Pose.Position + Vector3.up * (definition.Size.y * 0.5f + 0.05f),
                    Quaternion.LookRotation(direction), 0f);
                motor.ApplyLaunch(direction * approachSpeed, true);
                Physics.SyncTransforms();
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.That(motor.VerticalSpeed, Is.GreaterThan(10f), "Actual Boost trigger did not launch.");
                var landed = false;
                for (var step = 0; step < 150; step++)
                {
                    motor.Simulate(new NullPlayerInputSource(), 0.02f);
                    if (motor.VerticalSpeed <= 0f && motor.IsGrounded)
                    {
                        landed = Physics.Raycast(motor.transform.position + Vector3.up * 0.2f,
                            Vector3.down, out var hit, 0.6f, Physics.DefaultRaycastLayers,
                            QueryTriggerInteraction.Ignore) && hit.collider == landing;
                        break;
                    }
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(landed, Is.True, $"Approach {approachSpeed}: ended at {motor.transform.position}");
                var position = motor.transform.position;
                var margin = Mathf.Min(position.x - landing.bounds.min.x, landing.bounds.max.x - position.x,
                    position.z - landing.bounds.min.z, landing.bounds.max.z - position.z);
                Assert.That(margin, Is.GreaterThan(0.5f), "Bronze landing needs room for the capsule and human error.");
            }
        }

        [UnityTest]
        public IEnumerator Spiral_CrumbleActivatesFromRealControllerFloorContactAndResets()
        {
            ModuleSelectionState.Select(ModuleSelectionState.SpiralModuleId);
            yield return LoadScene("ModuleRunner");
            yield return WaitForPlayer();

            var player = Object.FindAnyObjectByType<ParkourMotor>();
            var character = player.GetComponent<CharacterController>();
            var crumbleBlocks = Object.FindObjectsByType<CrumblingBlock>(FindObjectsSortMode.None);
            Assert.That(crumbleBlocks.Length, Is.EqualTo(19));
            var crumble = crumbleBlocks[0];

            character.enabled = false;
            player.transform.position = crumble.transform.position + Vector3.up * 1.8f;
            character.enabled = true;
            var timeout = Time.realtimeSinceStartup + 2f;
            while (crumble.Phase == CrumblingBlockPhase.Stable
                && Time.realtimeSinceStartup < timeout)
            {
                character.Move(Vector3.down * 0.15f);
                yield return new WaitForFixedUpdate();
            }

            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Stage1));
            crumble.Tick(1.05f);
            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Gone));
            Assert.That(crumble.SolidColliderEnabled, Is.False);
            crumble.ResetForModule();
            Assert.That(crumble.Phase, Is.EqualTo(CrumblingBlockPhase.Stable));
            Assert.That(crumble.SolidColliderEnabled, Is.True);
        }

        [UnityTest]
        public IEnumerator Spiral_EntersTenTimesOnSolidStartAndRestoresPromptlyAfterFall()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                ModuleSelectionState.Select(ModuleSelectionState.SpiralModuleId);
                yield return LoadScene("ModuleRunner");
                yield return WaitForPlayer();
                yield return new WaitForFixedUpdate();

                var player = Object.FindAnyObjectByType<ParkourMotor>();
                var controller = Object.FindAnyObjectByType<ModuleSceneController>();
                Assert.That(player, Is.Not.Null, $"Spiral entry {attempt + 1}");
                Assert.That(controller, Is.Not.Null, $"Spiral entry {attempt + 1}");
                Assert.That(controller.ActiveModule.StableModuleId, Is.EqualTo(ModuleSelectionState.SpiralModuleId));
                Assert.That(player.transform.position.y, Is.GreaterThan(0.35f), $"Spiral entry {attempt + 1}");
                Assert.That(
                    Mathf.Abs(Mathf.DeltaAngle(player.transform.eulerAngles.y, controller.ActiveModule.StartPoint.Rotation.eulerAngles.y)),
                    Is.LessThan(0.1f),
                    $"Spiral entry {attempt + 1} did not retain authored StartAnchor yaw");
                Assert.That(
                    Physics.Raycast(
                        player.transform.position + Vector3.up * 0.1f,
                        Vector3.down,
                        out _,
                        0.25f,
                        Physics.DefaultRaycastLayers,
                        QueryTriggerInteraction.Ignore),
                    Is.True,
                    $"Spiral entry {attempt + 1} has no solid support");
            }

            var activePlayer = Object.FindAnyObjectByType<ParkourMotor>();
            var restore = activePlayer.GetComponent<Avoidance.Gameplay.Respawn.RestoreController>();
            var cameraRig = activePlayer.GetComponentInChildren<FirstPersonCameraRig>();
            const float preDeathYaw = 127f;
            const float preDeathPitch = -31f;
            cameraRig.ResetView(Quaternion.Euler(0f, preDeathYaw, 0f), preDeathPitch);
            var character = activePlayer.GetComponent<CharacterController>();
            character.enabled = false;
            activePlayer.transform.position = new Vector3(0f, -9f, 0f);
            character.enabled = true;
            Assert.That(activePlayer.transform.position.y, Is.LessThan(-7.5f));
            var timeout = Time.realtimeSinceStartup + 2f;
            while (restore.RestoreCount == 0 && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(restore.RestoreCount, Is.EqualTo(1));
            Assert.That(activePlayer.transform.position.y, Is.GreaterThan(0.35f));
            Assert.That(activePlayer.HorizontalSpeed, Is.LessThan(0.001f));
            Assert.That(
                Mathf.Abs(Mathf.DeltaAngle(activePlayer.transform.eulerAngles.y, preDeathYaw)),
                Is.LessThan(0.1f));
            Assert.That(cameraRig.Pitch, Is.EqualTo(preDeathPitch).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator Module003_AuditsActualArchitecturalAndRouteLandings()
        {
            ModuleSelectionState.Select("module.003.flow-error");
            yield return LoadScene("ModuleRunner"); yield return WaitForPlayer(); yield return null;
            Physics.SyncTransforms();
            var report = new System.Text.StringBuilder("# Runtime collision truth / Module 003\n\n");
            foreach (var block in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
            {
                if (block.isTrigger || block.gameObject.layer != LayerMask.NameToLayer("Ground")) continue;
                report.AppendLine($"ROUTE {block.name} collider={block.bounds}");
                foreach (var renderer in block.GetComponentsInChildren<Renderer>())
                    if (renderer.enabled) report.AppendLine($"  {renderer.name} visible={renderer.bounds}");
            }
            File.WriteAllText("Logs/Phase091VisualQA/runtime-bounds.md", report.ToString());
            var surfaces = Object.FindObjectsByType<Avoidance.Gameplay.Worlds.AuthoredSurface>(FindObjectsSortMode.None);
            Assert.That(surfaces.Length, Is.GreaterThan(25));
            foreach (var surface in surfaces)
            {
                Assert.That(surface.HasMatchingCollision, Is.True, surface.CollisionDetails);
                var collider = surface.GetComponent<MeshCollider>();
                var bounds = collider.bounds;
                var hits = 0;
                for (var x = 0; x < 11; x++) for (var z = 0; z < 11; z++)
                {
                    var origin = new Vector3(Mathf.Lerp(bounds.min.x, bounds.max.x, (x + 0.5f) / 11), bounds.max.y + 1,
                        Mathf.Lerp(bounds.min.z, bounds.max.z, (z + 0.5f) / 11));
                    if (collider.Raycast(new Ray(origin, Vector3.down), out var hit, bounds.size.y + 2) && hit.normal.y > 0.55f) hits++;
                }
                // Sparse combined foundations may fall between the coarse world-bounds grid.
                if(hits==0)
                {
                    var mesh=collider.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;
                    for(int i=0;i<triangles.Length;i+=3)
                    {
                        var a=surface.transform.TransformPoint(vertices[triangles[i]]);var b=surface.transform.TransformPoint(vertices[triangles[i+1]]);var c=surface.transform.TransformPoint(vertices[triangles[i+2]]);
                        if(Vector3.Cross(b-a,c-a).normalized.y<.55f)continue;
                        if(collider.Raycast(new Ray((a+b+c)/3+Vector3.up*.1f,Vector3.down),out var sample,.2f))hits++;
                    }
                }
                Assert.That(hits, Is.GreaterThan(0), surface.transform.parent.name + " has no supporting top samples");
                report.AppendLine($"PLAYABLE | {surface.transform.parent.name}/{surface.name} | exact {collider.sharedMesh.name} | {hits} supporting grid hits | {bounds}");
            }
            foreach (var block in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None))
            {
                if (block.isTrigger || block.gameObject.layer != LayerMask.NameToLayer("Ground")) continue;
                var bounds = block.bounds;
                Assert.That(block.Raycast(new Ray(bounds.center + Vector3.up * (bounds.extents.y + 0.2f), Vector3.down), out var hit, bounds.size.y + 1), Is.True, block.name);
                report.AppendLine($"PLAYABLE | {block.name} | authored BoxCollider | top={hit.point.y} | {bounds}");
            }
            Directory.CreateDirectory("Logs/Phase091VisualQA");
            File.WriteAllText("Logs/Phase091VisualQA/runtime-landings.md", report.ToString());
            foreach (var supportId in new[] { "m03.start", "m03.arrival.approach", "m03.arrival.court", "m03.moving-setup", "m03.boost-runup" })
            {
                var support = FindObjectByName(supportId);
                var bounds = support.GetComponent<BoxCollider>().bounds;
                var visuals = support.GetComponentsInChildren<Renderer>().Where(renderer => renderer.enabled).ToArray();
                var visualBounds=visuals[0].bounds;foreach(var visual in visuals.Skip(1))visualBounds.Encapsulate(visual.bounds);
                Assert.That(Vector3.Distance(visualBounds.center, bounds.center), Is.LessThan(0.01f), supportId);
                Assert.That(Vector3.Distance(visualBounds.size, bounds.size), Is.LessThan(0.01f), supportId);
            }
            foreach (var shrine in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(item => item.name == "Restore Shrine Visual"))
            {
                var support = shrine.parent.GetComponent<BoxCollider>().bounds;
                var visible = shrine.GetComponentInChildren<Renderer>().bounds;
                Assert.That(visible.min.z, Is.GreaterThanOrEqualTo(support.min.z), shrine.parent.name);
                Assert.That(visible.max.z, Is.LessThanOrEqualTo(support.max.z), shrine.parent.name);
            }
            var challenge = Object.FindAnyObjectByType<FlowChallenge>();
            Assert.That(challenge.Total, Is.EqualTo(5));
            var pickup = Object.FindObjectsByType<FlowPickup>(FindObjectsSortMode.None).First();
            var motor = Object.FindAnyObjectByType<ParkourMotor>();
            motor.GetComponent<PlayerRuntimeCoordinator>().enabled = false;
            motor.ResetMotion(pickup.transform.position, Quaternion.identity, 0);
            Physics.SyncTransforms(); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.That(challenge.Count, Is.EqualTo(1));
            Assert.That(pickup.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator ProductionFrontend_CapturesMenuCampaignHudAndResults()
        {
            yield return LoadBootstrapFresh("ModuleSelector");
            yield return new WaitForSecondsRealtime(0.8f);
            Directory.CreateDirectory("Logs/Phase091VisualQA");
            yield return CaptureProduct("06-main-menu");
            var campaign = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).First(x => x.name == "CAMPAIGN Button");
            campaign.onClick.Invoke(); yield return null;
            yield return CaptureProduct("07-campaign");
            var films = Object.FindObjectsByType<LoadingPresentation>(FindObjectsSortMode.None);
            Assert.That(films.Length, Is.EqualTo(1));
            ModuleSelectionState.Select("module.003.flow-error");
            var loader = new UnitySceneLevelLoader();
            var routine = loader.LoadAsync("ModuleRunner");
            Assert.That(routine.MoveNext(), Is.True);
            yield return null; yield return CaptureProduct("08-loading");
            while (routine.MoveNext()) yield return routine.Current;
            yield return WaitForPlayer(); yield return new WaitForSecondsRealtime(0.8f);
            yield return CaptureProduct("09-gameplay-hud");
            var hud = Object.FindAnyObjectByType<ModuleHud>();
            hud.ShowResults(new Avoidance.Gameplay.Timing.ModuleRunResult
            {
                Rank = Avoidance.Gameplay.Ranking.ModuleRank.Gold, CompletionSeconds = 98.321,
                PersonalBestSeconds = 98.321, IsNewPersonalBest = true,
                NextRank = Avoidance.Gameplay.Ranking.ModuleRank.Diamond, SecondsFromNextRank = 22.321
            });
            yield return new WaitForSecondsRealtime(0.3f);
            yield return CaptureProduct("10-results");
        }
        private static IEnumerator CaptureProduct(string name)
        {
            yield return null;
            var camera = Camera.main;
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(canvas => canvas.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
            var render = new RenderTexture(1560, 720, 24);
            var texture = new Texture2D(1560, 720, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            var aspect = camera.aspect;
            try
            {
                camera.targetTexture = render; camera.aspect = 1560f / 720;
                foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f; }
                Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = render;
                texture.ReadPixels(new Rect(0, 0, 1560, 720), 0, 0); texture.Apply();
                File.WriteAllBytes("Logs/Phase091VisualQA/" + name + ".png", texture.EncodeToPNG());
            }
            finally
            {
                foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null; }
                camera.targetTexture = null; camera.aspect = aspect; RenderTexture.active = previous;
                Object.Destroy(render); Object.Destroy(texture);
            }
        }

        private static IEnumerator LoadBootstrapFresh(string expectedScene)
        {
            var existing = Object.FindAnyObjectByType<GameBootstrap>();
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
                yield return null;
            }

            yield return LoadScene("Bootstrap");
            var timeout = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != expectedScene
                && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
        }

        private static IEnumerator LoadMovementLabFresh()
        {
            var existing = Object.FindAnyObjectByType<GameBootstrap>();
            if (existing != null)
            {
                Object.Destroy(existing.gameObject);
                yield return null;
            }

            yield return LoadScene("MovementLab");

            var timeout = Time.realtimeSinceStartup + 10f;
            while ((SceneManager.GetActiveScene().name != "MovementLab"
                    || Object.FindAnyObjectByType<ParkourMotor>() == null)
                && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (load != null && !load.isDone)
            {
                yield return null;
            }
        }

        private static IEnumerator WaitForPlayer()
        {
            var timeout = Time.realtimeSinceStartup + 10f;
            while (Object.FindAnyObjectByType<ParkourMotor>() == null
                && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }
        }

        private static bool FindObjectNamed(string objectName)
        {
            return FindObjectByName(objectName) != null;
        }

        private static bool RenderProductionView(
            Camera camera,
            Vector3 position,
            Vector3 target,
            string outputPath)
        {
            const int width = 1560;
            const int height = 720;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                camera.transform.SetPositionAndRotation(
                    position,
                    Quaternion.LookRotation((target - position).normalized, Vector3.up));
                camera.fieldOfView = FirstPersonCameraRig.DefaultPreferredFieldOfView;
                camera.targetTexture = renderTexture;
                camera.Render();
                RenderTexture.active = renderTexture;
                image.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                image.Apply(false, false);
                File.WriteAllBytes(outputPath, image.EncodeToPNG());

                var minimumLuminance = 1f;
                var maximumLuminance = 0f;
                for (var y = 0; y < height; y += 48)
                {
                    for (var x = 0; x < width; x += 48)
                    {
                        var luminance = image.GetPixel(x, y).grayscale;
                        minimumLuminance = Mathf.Min(minimumLuminance, luminance);
                        maximumLuminance = Mathf.Max(maximumLuminance, luminance);
                    }
                }
                return maximumLuminance - minimumLuminance > 0.18f;
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.Destroy(renderTexture);
                Object.Destroy(image);
            }
        }

        private readonly struct ProductionView
        {
            public ProductionView(string name, Vector3 position, Vector3 target)
            {
                Name = name;
                Position = position;
                Target = target;
            }

            public string Name { get; }
            public Vector3 Position { get; }
            public Vector3 Target { get; }
        }

        private static GameObject FindObjectByName(string objectName)
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (var index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == objectName)
                {
                    return transforms[index].gameObject;
                }
            }

            return null;
        }

        private sealed class CameraTestInput : IPlayerInputSource
        {
            public CameraTestInput(Vector2 lookDelta)
            {
                LookDelta = lookDelta;
            }

            public Vector2 Move => Vector2.zero;
            public Vector2 LookDelta { get; }
            public bool JumpPressed => false;
            public void ResetState() { }
        }
    }
}
