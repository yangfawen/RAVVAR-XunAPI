/*
 *此脚本作为一个lua入口来启动一个lua案例。
 *原因1，目前的容器代码在不断的更新以修正bug，所以不宜直接打包进assetbundle，
 *否则加载的时候可能app代码和资源包中代码不一致，会有序列化问题；
 *原因2，在制作资源包时，通过直接拖拽的方式去关联诸多的脚本，不利于理解和维护。
 *因此，可以通过写一个入口文件的方式来用代码加载各个Lua类，当然这不是必须的。
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SLua;
using OnceBuild;
/// <summary>
/// Lua代码执行
/// </summary>
public class LuaRunner : MonoBehaviour {

    public TextAsset LuaText;
    public string LuaString;

    private bool inited = false;

	void Awake () {
        Init();
	}

    public bool Init() {
        if (inited) {
            return false;
        }
        else {
            if (LuaText!=null) {
                inited = true;
                var luaMain = SluaAdapter.Instance.DoString(LuaText.text) as LuaFunction;
                if (luaMain!=null) {
                    luaMain.call(gameObject);
                }
            }
            else if (LuaString!=null) {
                inited = true;
                var luaMain = SluaAdapter.Instance.DoString(LuaString) as LuaFunction;
                if (luaMain!=null) {
                    luaMain.call(gameObject);
                }
            }
            return inited;
        }
    }
}
