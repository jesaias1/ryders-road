using System;
using System.Linq;
using Avoidance.Gameplay.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace Avoidance.UI
{
    public static class CampaignTrialMenu
    {
        public static GameObject Create(Transform parent, Action back, Action launch)
        {
            var panel = Box(parent, "Campaign Trial Menu", new Vector2(.08f,.15f), new Vector2(.92f,.85f));
            panel.AddComponent<Image>().color = BrandPresentation.PanelNavy;
            string road = "module.004.solar-foundry";
            var mode = CampaignTrialMode.FlowLanding;
            var title = Label(panel.transform, "Trial Selection", "", .83f, .98f, 30);
            var hint = Label(panel.transform, "Trial Hint", "", .19f, .35f, 22);
            var modules = Resources.LoadAll<ModuleDefinition>("Modules");
            void Refresh()
            {
                title.text = "CAMPAIGN FLOW TRIAL  /  " + modules.Single(m => m.StableModuleId == road).DisplayName;
                hint.text = (mode == CampaignTrialMode.Accepted ? "A · Accepted motor and your saved controls"
                    : mode == CampaignTrialMode.PreviousFlow ? "D · Previous 0.9.9 Flow motor · manual pitch"
                    : mode == CampaignTrialMode.FlowManual ? "B · New air control · manual pitch"
                    : "C · Same new air control · landing view")
                    + "\nPractice only · separate session times · Campaign progress stays unchanged";
                foreach(var button in panel.GetComponentsInChildren<Button>())
                    if(button.name.StartsWith("Trial Road ") || button.name.StartsWith("Trial Mode "))
                        button.GetComponent<Image>().color = button.name=="Trial Road "+road || button.name=="Trial Mode "+mode
                            ? new Color(.06f,.40f,.48f,.98f) : new Color(.055f,.22f,.30f,.98f);
            }
            var ids = ModuleSelectionState.GetCampaignModuleIds();
            for (int i=0; i<ids.Length; i++)
            {
                var id=ids[i];
                Button(panel.transform, "Trial Road " + id, modules.Single(m=>m.StableModuleId==id).DisplayName.Replace("Campaign ",""),
                    new Vector2(.03f+i*.24f,.62f),new Vector2(.25f+i*.24f,.79f),()=> {road=id;Refresh();});
            }
            var modes=new[]{CampaignTrialMode.Accepted,CampaignTrialMode.FlowManual,CampaignTrialMode.FlowLanding,CampaignTrialMode.PreviousFlow};
            var labels=new[]{"A · ACCEPTED","B · NEW MANUAL","C · NEW LANDING","D · OLD FLOW"};
            for(int i=0;i<modes.Length;i++)
            {
                var option=modes[i];
                Button(panel.transform,"Trial Mode "+option,labels[i],new Vector2(.03f+i*.24f,.40f),new Vector2(.25f+i*.24f,.57f),()=> {mode=option;Refresh();});
            }
            Button(panel.transform,"Trial Back","BACK",new Vector2(.03f,.035f),new Vector2(.30f,.17f),()=> {panel.SetActive(false);back();});
            Button(panel.transform,"Trial Start","START TRIAL",new Vector2(.57f,.035f),new Vector2(.97f,.17f),()=> {CampaignFlowTrial.Launch(road,mode);launch();});
            Refresh();panel.SetActive(false);return panel;
        }

        private static GameObject Box(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;return go;
        }
        private static Text Label(Transform parent,string name,string value,float min,float max,int size)
        {
            var go=Box(parent,name,new Vector2(.02f,min),new Vector2(.98f,max));var text=go.AddComponent<Text>();
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=TextAnchor.MiddleCenter;
            text.text=value;text.color=Color.white;text.raycastTarget=false;return text;
        }
        private static void Button(Transform parent,string name,string title,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action)
        {
            var go=Box(parent,name,min,max);go.AddComponent<Image>().color=new Color(.055f,.22f,.30f,.98f);
            var button=go.AddComponent<Button>();button.onClick.AddListener(action);Label(go.transform,name+" Label",title,0,1,24);
        }
    }
}
