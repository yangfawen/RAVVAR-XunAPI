/*
 * 用来在资源包中索引资源，方便lua或者其它脚本的获取
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 资源引用
/// </summary>
public class AssetsRef : MonoBehaviour {

    public Object[] list;

    public Object At(int idx) {
        if (list!=null && idx>=0 && idx<list.Length) {
            return list[idx];
        }
        return null;
    }
    public Object Find(string name) {
        for (int i = 0; i < list.Length; i++) {
            if (list[i].name == name) {
                return list[i];
            }
        }
        return null;
    }
}
