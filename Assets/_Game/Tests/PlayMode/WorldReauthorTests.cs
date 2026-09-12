using System.Collections;
using System.IO;
using System.Linq;
using Avoidance.Gameplay.Levels;
using Avoidance.Gameplay.Player;
using Avoidance.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Avoidance.Tests.PlayMode
{
    public sealed class WorldReauthorTests
    {
        public static readonly string[] Worlds = {"module.001.first-steps", "module.002.moving-parts",
            "module.003.flow-error", "module.004.solar-foundry", "module.005.foundry-pulse", "module.004.the-spiral"};

        [UnityTest] public IEnumerator CaptureEveryWorld()
        {
            string stage=System.Environment.GetEnvironmentVariable("RYDERS_WORLD_CAPTURE_STAGE") ?? "after";
            string root="Logs/WorldReauthorQA/"+stage;Directory.CreateDirectory(root);
            foreach(string id in Worlds)
            {
                ModuleSelectionState.Select(id,true);
                yield return new UnitySceneLevelLoader().LoadAsync("ModuleRunner");yield return null;
                Object.FindAnyObjectByType<PlayerRuntimeCoordinator>().enabled=false;
                var camera=Camera.main;Assert.That(camera,Is.Not.Null);
                foreach(var c in camera.GetComponents<MonoBehaviour>())c.enabled=false;
                var module=Object.FindAnyObjectByType<ModuleSceneController>().ActiveModule;
                var renderers=Object.FindObjectsByType<MeshRenderer>();
                var filters=Object.FindObjectsByType<MeshFilter>();
                var materials=renderers.SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct().ToArray();
                var report=new System.Collections.Generic.List<string>{"kind,name,detail"};
                long triangles=0;var combined=new System.Collections.Generic.HashSet<Mesh>();
                foreach(var filter in filters)
                {
                    var mesh=filter.sharedMesh;if(mesh==null)continue;
                    var renderer=filter.GetComponent<Renderer>();
                    if(renderer!=null&&renderer.isPartOfStaticBatch&&!combined.Add(mesh))continue;
                    long count=0;for(int i=0;i<mesh.subMeshCount;i++)count+=(long)mesh.GetIndexCount(i)/3;triangles+=count;
                }
                report.Add("scene,instances,"+filters.Length);report.Add("scene,instancedTriangles,"+triangles);
                report.Add("scene,materials,"+materials.Length);report.Add("scene,shadowCasters,"+renderers.Count(r=>r.shadowCastingMode!=UnityEngine.Rendering.ShadowCastingMode.Off));
                report.Add("scene,lodGroups,"+Object.FindObjectsByType<LODGroup>().Length);
                report.Add("machine,gpu,"+SystemInfo.graphicsDeviceName);
                foreach(var material in materials)
                {
                    report.Add("material,"+material.name+","+material.shader.name+" instancing="+material.enableInstancing);
                    foreach(string property in material.GetTexturePropertyNames())
                    {
                        var texture=material.GetTexture(property);if(texture!=null)report.Add("texture,"+texture.name+","+texture.width+"x"+texture.height+" "+property);
                    }
                }
                File.WriteAllLines(root+"/"+id+"-budget.csv",report);
                var route=module.Blocks.Where(b=>!b.StableId.Contains("mastery")&&!b.StableId.Contains("risky")).ToArray();
                var positions=new[]{0,route.Length/2,route.Length/3,route.Length-3,route.Length-1};
                var names=new[]{"opening","mid-route","hero-vista","fast-approach","destination"};
                for(int shot=0;shot<positions.Length;shot++)
                {
                    int index=positions[shot];var p=route[index].Pose.Position;
                    var target=index<route.Length-1?route[index+1].Pose.Position:module.PatchBlock.Pose.Position;
                    camera.transform.position=p+Vector3.up*1.92f;
                    if(shot==2){camera.transform.position=p+new Vector3(9,6,-9);target=p+Vector3.forward*12;}
                    if(shot==4){camera.transform.position=p+new Vector3(0,1.92f,-3);target=module.PatchBlock.Pose.Position;}
                    camera.transform.LookAt(target+Vector3.up*.3f);
                    var rt=new RenderTexture(1560,720,24);var texture=new Texture2D(1560,720,TextureFormat.RGB24,false);
                    var active=RenderTexture.active;float aspect=camera.aspect;
                    camera.targetTexture=rt;camera.aspect=1560f/720;camera.Render();RenderTexture.active=rt;
                    if(shot==0)
                    {
                        var samples=new double[12];var watch=new System.Diagnostics.Stopwatch();
                        for(int sample=0;sample<samples.Length;sample++){watch.Restart();camera.Render();watch.Stop();samples[sample]=watch.Elapsed.TotalMilliseconds;}
                        System.Array.Sort(samples);
                        File.WriteAllText(root+"/"+id+"-render-cpu.txt",$"1560x720 Camera.Render desktop CPU wall time, 12 warm samples. Median {samples[6]:F3} ms; max {samples[11]:F3} ms. Not GPU time, device FPS or sustained S23 performance.");
                    }
                    texture.ReadPixels(new Rect(0,0,1560,720),0,0);texture.Apply();
                    File.WriteAllBytes(root+"/"+id+"-"+names[shot]+".png",texture.EncodeToPNG());
                    camera.targetTexture=null;camera.aspect=aspect;RenderTexture.active=active;
                    Object.Destroy(rt);Object.Destroy(texture);yield return null;
                }
            }
            yield return new UnitySceneLevelLoader().LoadAsync("ModuleSelector");
        }
    }
}
