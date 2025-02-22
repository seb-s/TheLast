﻿using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResSvc : MonoBehaviour
{
    #region 单例模式
    private static ResSvc _instance = null;

    public static ResSvc Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.Find("GameManager").GetComponent<ResSvc>();
            }
            return _instance;
        }
    }
    #endregion

    public void InitSvc()
    {
        InitDialogueCfgs(Application.streamingAssetsPath + PathDefine.SceneDialogueCfgs);
        Debug.Log("Init ResSvc Done");
    }

    //更新进度条的委托
    private Action progressUpdate = null;
    //存储预制体的字典
    private Dictionary<string, GameObject> prefabDicts = new Dictionary<string, GameObject>();
    //存储图片资源的字典
    private Dictionary<string, Sprite> spriteDicts = new Dictionary<string, Sprite>();
    //存储贴图的字典
    private Dictionary<string, Texture> textureDicts = new Dictionary<string, Texture>();
    //存储音效资源的字典
    private Dictionary<string, AudioClip> audioClipDicts = new Dictionary<string, AudioClip>();
    //存储动画的字典
    private Dictionary<string, AnimationClip> animationClipDicts = new Dictionary<string, AnimationClip>();


    public void AsyncLoadScene(int sceneIndex,Action finish,bool isWait)
    {
        //显示加载界面
        GameManager.Instance.loadingPanel.SetWindowState(true);
        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneIndex);
        if(isWait)
        {
                    sceneAsync.allowSceneActivation = false;
        }

        progressUpdate = () =>
        {
            float progress = sceneAsync.progress;
            //设置进度条
            GameManager.Instance.loadingPanel.SetProgress(progress);
            if(progress >= 0.9f && isWait)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    sceneAsync.allowSceneActivation = true;
                }
            }

            if(progress == 1)
            {
                if(finish != null)
                {
                    finish();
                }
                progressUpdate = null;
                sceneAsync = null;
                //加载完成隐藏加载界面
                GameManager.Instance.loadingPanel.SetWindowState(false);
            }
        };
    }

    /// <summary>
    /// 加载预制体
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cache"></param>
    /// <returns></returns>
    public GameObject LoadPrefab(string path,bool cache = false)
    {
        GameObject prefab = null;
        if(!prefabDicts.TryGetValue(path,out prefab))
        {
            prefab = Resources.Load<GameObject>(path);
            if(cache)
            {
                prefabDicts.Add(path, prefab);
            }
        }
        GameObject go = null;
        if(prefab != null)
        {
            go = Instantiate(prefab);
        }
        return go;
    }

    public GameObject LoadPrefab(string path,Vector3 spawnPos,Quaternion quaternion, bool cache = false)
    {
        GameObject prefab = null;
        if (!prefabDicts.TryGetValue(path, out prefab))
        {
            prefab = Resources.Load<GameObject>(path);
            if (cache)
            {
                prefabDicts.Add(path, prefab);
            }
        }
        GameObject go = null;
        if (prefab != null)
        {
            go = Instantiate(prefab,spawnPos,quaternion);
        }
        return go;
    }

    /// <summary>
    /// 加载Sprite资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cache"></param>
    /// <returns></returns>
    public Sprite LoadSprite(string path,bool cache = false)
    {
        Sprite sprite = null;
        if(!spriteDicts.TryGetValue(path,out sprite))
        {
            sprite = Resources.Load<Sprite>(path);
            if(cache)
            {
                spriteDicts.Add(path, sprite);
            }
        }
        return sprite;
    }

    /// <summary>
    /// 加载纹理贴图资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cache"></param>
    /// <returns></returns>
    public Texture LoadTexture(string path,bool cache = false)
    {
        Texture texture = null;
        if(!textureDicts.TryGetValue(path,out texture))
        {
            texture = Resources.Load<Texture>(path);
            if(cache)
            {
                textureDicts.Add(path, texture);
            }
        }
        return texture;
    }

    /// <summary>
    /// 加载音效资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="cache"></param>
    /// <returns></returns>
    public AudioClip LoadAudioClip(string path,bool cache = false)
    {
        AudioClip audioClip = null;
        if(!audioClipDicts.TryGetValue(path,out audioClip))
        {
            audioClip = Resources.Load<AudioClip>(path);
            if(cache)
            {
                audioClipDicts.Add(path, audioClip);
            }
        }
        return audioClip;
    }

    /// <summary>
    ///加载动画资源
    /// </summary>
    public AnimationClip LoadAnimationClip(string path,bool cache = true)
    {
        AnimationClip aniClip = null;
        if(!animationClipDicts.TryGetValue(path,out aniClip))
        {
            aniClip = Resources.Load<AnimationClip>(path);
            if(cache)
            {
                animationClipDicts.Add(path, aniClip);
            }
        }
        return aniClip;
    }

    private void Update()
    {
        if(progressUpdate != null)
        {
            progressUpdate();
        }
    }

    #region 初始化配置文件

    #region 敌人波次信息
    private Dictionary<int, EnemyWave> enemyWaveDicts = new Dictionary<int, EnemyWave>();

    public void InitEnemyWaveCfgs(string path)
    {
        enemyWaveDicts.Clear();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);

        XmlNode root = xmlDoc.SelectSingleNode("root");
        XmlNodeList nodeList = root.ChildNodes;
        foreach(XmlNode node in nodeList)
        {
            int ID = int.Parse(node.Attributes["ID"].Value);
            EnemyWave enemyWave = new EnemyWave
            {
                waveIndex = ID,
                enemyBattleProps = new BattleProps(),
                enemyPosList = new List<Vector3>(),
            };
            XmlNodeList fieldNodeList = node.ChildNodes;
            foreach(XmlNode fieldNode in fieldNodeList)
            {
                switch (fieldNode.Name)
                {
                    case "delayTime":
                        enemyWave.delayTime = float.Parse(fieldNode.InnerText);
                        break;
                    case "spawnInterval":
                        enemyWave.enemySpawnInterval = float.Parse(fieldNode.InnerText);
                        break;
                    case "enemyPosList":
                        string[] enemyPosArr = fieldNode.InnerText.Split('|');
                        foreach(string posStr in enemyPosArr)
                        {
                            string[] coordinates = posStr.Split(',');
                            float xPos = float.Parse(coordinates[0]);
                            float yPos = float.Parse(coordinates[1]);
                            float zPos = float.Parse(coordinates[2]);
                            Vector3 enemyPos = new Vector3(xPos, yPos, zPos);
                            enemyWave.enemyPosList.Add(enemyPos);
                        }
                        break;
                    case "enemyCount":
                        enemyWave.enemyCount = int.Parse(fieldNode.InnerText);
                        break;
                    case "enemyHp":
                        enemyWave.enemyBattleProps.hp = int.Parse(fieldNode.InnerText);
                        break;
                    case "enemyDamage":
                        enemyWave.enemyBattleProps.damage = int.Parse(fieldNode.InnerText);
                        break;
                }
            }
            if(!enemyWaveDicts.ContainsKey(ID))
            {
                enemyWaveDicts.Add(ID, enemyWave);
            }
        }
    }

    public EnemyWave GetEnemyWaveCfg(int id)
    {
        EnemyWave enemyWave = null;
        if(enemyWaveDicts.TryGetValue(id,out enemyWave))
        {
            return enemyWave;
        }
        return null;
    }

    public List<EnemyWave> GetAllEnemyWaveCfgs()
    {
        List<EnemyWave> results = new List<EnemyWave>();
        foreach(var pair in enemyWaveDicts)
        {
            results.Add(pair.Value);
        }
        return results;
    }
    #endregion


    #region 对话配置文件
    private Dictionary<int, DialogueCfg> dialogueCfgDicts = new Dictionary<int, DialogueCfg>();

    public void InitDialogueCfgs(string path)
    {
        dialogueCfgDicts.Clear();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);

        XmlNode root = xmlDoc.SelectSingleNode("root");
        XmlNodeList nodeList = root.ChildNodes;
        foreach(XmlNode node in nodeList)
        {
            int ID = int.Parse(node.Attributes["ID"].Value);
            DialogueCfg dialogueCfg = new DialogueCfg {
                dialoguesList = new List<DialogueInfo>()
            };
            XmlNodeList fieldNodeList = node.ChildNodes;
            foreach(XmlNode fieldNode in fieldNodeList)
            {
                switch(fieldNode.Name)
                {
                    case "sceneID":
                        dialogueCfg.sceneID = int.Parse(fieldNode.InnerText);
                        break;
                    case "dialogArr":
                        string[] dialogArr = fieldNode.InnerText.Split('#');
                        for(int index = 0; index < dialogArr.Length; index++)
                        {
                            if (index == 0)
                                continue;
                            string[] infoArr = dialogArr[index].Split('|');

                            DialogueInfo dialogueInfo = new DialogueInfo { };
                            dialogueInfo.roleID = int.Parse(infoArr[0]);
                            dialogueInfo.name = infoArr[1];
                            dialogueInfo.detail = infoArr[2];
                            dialogueInfo.spritePath = infoArr[3];
                            dialogueInfo.bgPath = infoArr[4];

                            dialogueCfg.dialoguesList.Add(dialogueInfo);
                        }
                        break;
                }
            }
            if(!dialogueCfgDicts.ContainsKey(ID))
            {
                dialogueCfgDicts.Add(ID, dialogueCfg);
            }
        }
    }

    public DialogueCfg GetCurDialogueCfg(int id)
    {
        DialogueCfg dialogueCfg = null;
        if(dialogueCfgDicts.TryGetValue(id,out dialogueCfg))
        {
            return dialogueCfg;
        }
        return null;
    }

    #endregion
    #endregion
}
